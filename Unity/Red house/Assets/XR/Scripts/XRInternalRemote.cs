using C8;
using Capnp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace XRInternal {

public class XRInternalRemote {

  const byte PLAYER_MESSAGE_HANDSHAKE = 1;
  const byte PLAYER_MESSAGE_SCREEN_ORIENTATION = 3;
  const byte PLAYER_MESSAGE_SCREEN_IMAGE = 10;

  const byte EDITOR_MESSAGE_HANDSHAKE = 1;
  const byte EDITOR_MESSAGE_DEVICE_SIZE = 2;
  const byte EDITOR_MESSAGE_DEVICE_ORIENTATION = 4;
  const byte EDITOR_MESSAGE_TOUCHES = 10;
  const byte EDITOR_MESSAGE_CAMERAS = 20;

  public const int PLAYER_PORT  = 7201;
  const string PLAYER_ID = "UnityRemote";
  const int PLAYER_PROTOCOL = 0;
  const int PLAYER_MESSAGE_HEADER_BYTES = 5;

  const int XR_SERVER_PORT = 23285;

  const int INITIAL_BUFFER_SIZE = 131072;  // 128 K

  private Socket playerSocket_ = null;
  private TcpClient tcp_ = null;

  private byte[] copyBuffer_;
  MemoryStream readBuffer_;
  MemoryStream writeBuffer_;
  MemoryStream messageBuffer_;

  // Data provided by the editor to the bridge, to be sent to the player.
  private XrInternalRemoteData.ScreenSize deviceScreenSize_;
  private XrInternalRemoteData.DeviceOrientation deviceOrientation_;
  private List<XrInternalRemoteData.Touch> touches_;

  // Data sent from the player, to be provided to the editor.
  private XrInternalRemoteData.ScreenPreview screenPreview_;
  private ScreenOrientation screenOrientation_;

  static void DebugLog(string message) {
    // Debug.Log("[XRInternalRemote] <" + Thread.CurrentThread.Name + "> " + message);
  }

  public void Start(int maxConnections) {
    Disconnect();
    playerSocket_ = OpenSocket(maxConnections);
    copyBuffer_ = new byte[INITIAL_BUFFER_SIZE];
    readBuffer_ = new MemoryStream(INITIAL_BUFFER_SIZE);
    writeBuffer_ = new MemoryStream(INITIAL_BUFFER_SIZE);
    messageBuffer_ = new MemoryStream(INITIAL_BUFFER_SIZE);
    deviceScreenSize_ = new XrInternalRemoteData.ScreenSize();
    deviceOrientation_ = new XrInternalRemoteData.DeviceOrientation();
    touches_ = new List<XrInternalRemoteData.Touch>();
    screenPreview_ = new XrInternalRemoteData.ScreenPreview();
    screenOrientation_ = new ScreenOrientation();
  }

  public void AddTouch(XrInternalRemoteData.Touch touch) {
    touches_.Add(touch);
  }

  public void SetDeviceInfo(int width, int height, int orientation) {
    deviceScreenSize_.screenWidth = width;
    deviceScreenSize_.screenHeight = height;
    deviceOrientation_.deviceOrientation = orientation;
  }

  public void Update() {
    if (!IsConnected()) {
      return;
    }

    try {
      ReadPlayerMessages();
      SendBufferedEvents();
    } catch(Exception e) {
      DebugLog("Player socket disconnected with error " + e);
      Disconnect();
    }
  }

  public MessageBuilder ScreenPreview() {
    MessageBuilder msg = new MessageBuilder();
    var preview = msg.initRoot(CompressedImageData.factory);
    if (screenPreview_.data != null && screenPreview_.data.Length > 0) {
      lock (screenPreview_.data) {
        try {
          preview.setWidth(screenPreview_.width);
          preview.setHeight(screenPreview_.height);
          byte[] bytes = new byte[screenPreview_.data.Length];
          screenPreview_.data.CopyTo(bytes, 0);
          preview.setData(bytes);

        } catch (Exception e) {
          DebugLog("Exception when processing screen preview " + e);
        }
      }
    }
    return msg;
  }

  public ScreenOrientation ScreenOrientation() {
    return screenOrientation_;
  }

  public bool IsConnected() {
    return tcp_ != null;
  }

  public void Disconnect() {
    if (IsConnected()) {
      tcp_.GetStream().Close();
      tcp_.Client.Close();
      tcp_.Close();
      tcp_ = null;
      writeBuffer_.Position = 0;
      writeBuffer_.SetLength(0);
      readBuffer_.Position = 0;
      readBuffer_.SetLength(0);
    }
    if (playerSocket_ != null) {
      playerSocket_.Close();
    }
  }

  // ************* Private methods below *********************************** //

  private void SendHandshake() {
    // When first connected, buffer handshake data for send.
    DebugLog("Send Handshake");
    TcpMessageBuilder.NewMessage(EDITOR_MESSAGE_HANDSHAKE, messageBuffer_)
      .Add(PLAYER_ID)
      .Add(PLAYER_PROTOCOL)
      .Write(writeBuffer_, copyBuffer_);
    TcpMessageBuilder.NewMessage(EDITOR_MESSAGE_CAMERAS, messageBuffer_)
      .Add((uint)0)
      .Write(writeBuffer_, copyBuffer_);
  }

  private Socket OpenSocket(int connections) {
    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    socket.Blocking = false;
    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    socket.Bind(new IPEndPoint(IPAddress.Any, PLAYER_PORT));
    socket.Listen(connections);  // Max incoming connections
    socket.BeginAccept(AcceptCallback, socket);
    return socket;
  }

  private void AcceptCallback(IAsyncResult ar) {
		Socket listener = (Socket) ar.AsyncState;
	  Socket tcpSocket = listener.EndAccept(ar);
		tcpSocket.Blocking = true;
		tcp_ = new TcpClient {Client = tcpSocket};
    DebugLog("Connected to Player on " + tcpSocket.LocalEndPoint);
		if (IsConnected()) {
      SendHandshake();
		}
	}

  private void ReadPlayerMessages() {
    if (tcp_.Client.Available == 0) {
      return;
    }

    readBuffer_.Position = readBuffer_.Length;
    PartialCopyTo(tcp_.GetStream(), readBuffer_, copyBuffer_, tcp_.Available);

    readBuffer_.Position = 0;
    while (HasNextMessage(readBuffer_)) {
      var reader = new BinaryReader(readBuffer_);
      var tag = reader.ReadByte();
      var size = reader.ReadUInt32();

      switch (tag) {
        case PLAYER_MESSAGE_HANDSHAKE:
          DebugLog("Got player handshake");
          if (ReadPlayerString(reader) != PLAYER_ID || reader.ReadUInt32() != PLAYER_PROTOCOL) {
            throw new ApplicationException("Bad player id or version");
          }
          break;
        case PLAYER_MESSAGE_SCREEN_IMAGE:
          DebugLog("Got screen image");
          screenPreview_.width = reader.ReadInt32();
          screenPreview_.height = reader.ReadInt32();
          screenPreview_.data = new byte[reader.ReadInt32()];
          reader.Read(screenPreview_.data, 0, screenPreview_.data.Length);
          break;
        case PLAYER_MESSAGE_SCREEN_ORIENTATION:
          screenOrientation_ = (ScreenOrientation) reader.ReadInt32();
          // The following are unused
#pragma warning disable 0219
          var autorotateToPortrait = reader.ReadInt32() != 0;
          var autorotateToPortraitUpsideDown = reader.ReadInt32() != 0;
          var autorotateToLandscapeLeft = reader.ReadInt32() != 0;
          var autorotateToLandscapeRight = reader.ReadInt32() != 0;
#pragma warning restore 0219
          break;
        default:
          DebugLog("Ignored player message " + tag + " of size " + size);
          reader.ReadBytes((int)size);
          break;
      }
    }

    // Move any partial message data to the beginning of the buffer.
    var partialBytes = readBuffer_.Length - readBuffer_.Position;
    Array.Copy(
      readBuffer_.GetBuffer(), readBuffer_.Position, readBuffer_.GetBuffer(), 0, partialBytes);
    readBuffer_.Position = 0;
    readBuffer_.SetLength(partialBytes);
  }

  private static string ReadPlayerString(BinaryReader reader) {
    return Encoding.UTF8.GetString(reader.ReadBytes((int)reader.ReadUInt32()));
  }

  private void SendBufferedEvents() {
    // Buffer device size info for send.
    if (deviceScreenSize_.screenWidth != 0 || deviceScreenSize_.screenHeight == 0) {
      DebugLog("Send device size");
      TcpMessageBuilder.NewMessage(EDITOR_MESSAGE_DEVICE_SIZE, messageBuffer_)
        .Add(deviceScreenSize_.screenWidth)
        .Add(deviceScreenSize_.screenHeight)
        .Write(writeBuffer_, copyBuffer_);
      deviceScreenSize_.screenWidth = 0;
      deviceScreenSize_.screenHeight = 0;
    }

    // Buffer dvice orientation info for send.
    if (deviceOrientation_.deviceOrientation != (int)DeviceOrientation.Unknown) {
      DebugLog("SendDeviceOrientation");
      TcpMessageBuilder.NewMessage(EDITOR_MESSAGE_DEVICE_ORIENTATION, messageBuffer_)
        .Add(deviceOrientation_.deviceOrientation)
        .Write(writeBuffer_, copyBuffer_);
      deviceOrientation_.deviceOrientation = (int)DeviceOrientation.Unknown;
    }

    // Buffer touch info for send.
    foreach (var touch in touches_) {
      DebugLog("SendTouchInput");
      TcpMessageBuilder.NewMessage(EDITOR_MESSAGE_TOUCHES, messageBuffer_)
        .Add(touch.positionX)
        .Add(touch.positionY)
        .Add(touch.frameCount)
        .Add(touch.fingerId)
        .Add(touch.phase)
        .Add(touch.tapCount)
        .Write(writeBuffer_, copyBuffer_);
    }
    touches_.Clear();

    // Send buffered data and clear the buffer.
    writeBuffer_.Position = 0;
    PartialCopyTo(writeBuffer_, tcp_.GetStream(), copyBuffer_, (int)writeBuffer_.Length);
    writeBuffer_.Position = 0;
    writeBuffer_.SetLength(0);
  }

  private static void PartialCopyTo(Stream src, Stream dst, byte[] scratch, int bytes) {
    var bytesRemaining = bytes;
    while (bytesRemaining > 0) {
      var bytesRead = src.Read(scratch, 0, Math.Min(scratch.Length, bytesRemaining));
      dst.Write(scratch, 0, bytesRead);
      bytesRemaining -= bytesRead;
    }
  }

  private static long NextMessageSize(MemoryStream buffer) {
    var r = new BinaryReader(buffer);
    var p = buffer.Position;

    r.ReadByte();
    var size = r.ReadUInt32();
    buffer.Position = p;

    return size + PLAYER_MESSAGE_HEADER_BYTES;
  }

  private static bool HasNextMessage(MemoryStream buffer) {
    var bufferRemaining = buffer.Length - buffer.Position;
    if (bufferRemaining < PLAYER_MESSAGE_HEADER_BYTES) {
      return false;
    }

    return bufferRemaining >= NextMessageSize(buffer);
  }

  private class TcpMessageBuilder {
    BinaryWriter binaryWriter_;
    MemoryStream messageBuffer_;
    byte tag_;

    public static TcpMessageBuilder NewMessage(byte tag, MemoryStream messageBuffer) {
      return new TcpMessageBuilder(tag, messageBuffer);
    }

    public TcpMessageBuilder Add(int i) { binaryWriter_.Write(i); return this;}
    public TcpMessageBuilder Add(uint u) { binaryWriter_.Write(u); return this;}
    public TcpMessageBuilder Add(long l) { binaryWriter_.Write(l); return this;}
    public TcpMessageBuilder Add(float f) { binaryWriter_.Write(f); return this;}
    public TcpMessageBuilder Add(byte[] b) { binaryWriter_.Write(b); return this;}

    public TcpMessageBuilder Add(string s) {
      binaryWriter_.Write((uint)s.Length);
      binaryWriter_.Write(Encoding.UTF8.GetBytes(s));
      return this;
    }

    public void Write(Stream outStream, byte[] copyBuffer) {
      // Header: tag + length of body
      outStream.WriteByte(tag_);
      uint bodyLength = (uint)messageBuffer_.Length;
      outStream.WriteByte((byte)((bodyLength >> 0) & 0xFF));
      outStream.WriteByte((byte)((bodyLength >> 8) & 0xFF));
      outStream.WriteByte((byte)((bodyLength >> 16) & 0xFF));
      outStream.WriteByte((byte)((bodyLength >> 24) & 0xFF));

      // Message Body
      messageBuffer_.Position = 0;
      PartialCopyTo(messageBuffer_, outStream, copyBuffer, (int)bodyLength);
    }

    private TcpMessageBuilder(byte tag, MemoryStream messageBuffer) {
      messageBuffer_ = messageBuffer;
      binaryWriter_ = new BinaryWriter(messageBuffer_);
      tag_ = tag;
      messageBuffer_.Position = 0;
      messageBuffer_.SetLength(0);
    }
  }

  public class XrInternalRemoteData {
    public struct ScreenSize {
      public int screenWidth;
      public int screenHeight;
    }

    public struct DeviceOrientation {
      public int deviceOrientation;  // Casts to DeviceOrientation enum.
    }

    public struct Touch {
      public float positionX;
      public float positionY;
      public long frameCount;
      public int fingerId;
      public int phase;  // Casts to TouchPhase enum.
      public int tapCount;
    }

    public struct ScreenPreview {
      public int width;
      public int height;
      public byte[] data;
    }
  }

} // end XRInternalRemote

} // namespace XRInternal

