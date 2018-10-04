using C8;
using Capnp;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Runtime.InteropServices;
#endif

namespace XRInternal {

public class XREditorBridge {

  // Max time to display "Waiting for remote" billboard
  const int GUI_TIMEOUT_IN_SECONDS = 10;

  static void DebugLog(string message) {
    //UnityEngine.Debug.Log("[XREditorBridge] <" + Thread.CurrentThread.Name + "> " + message);
  }

  public void Start() {
#if UNITY_EDITOR
    InternalStart();
#endif
  }

  public void Update() {
#if UNITY_EDITOR

    tick_ = true;
#endif
  }

  public void CheckADB() {
#if UNITY_EDITOR
    if (!timeout() && !XRRemoteUtils.SetupADB() && !remote_.IsConnected()) {
      remote_.Start(1);
    }
#endif
  }

  public void OnApplicationQuit() {
#if UNITY_EDITOR
    Disconnect();
#endif
  }

  public void SendDeviceInfo(XrRemoteApp.Reader remote) {
#if UNITY_EDITOR
    if (remote_.IsConnected()) {
      BufferForSend(remote);
    }
#endif
  }

  public void SetPlayerAspect(XrRemoteApp.Reader remote) {
#if UNITY_EDITOR
    // When the client keeps running and the server restarts, remote.hasDevice() is false
    XRRemoteUtils.SetGameViewAspectRatio(remote);
#endif
  }

  public Vector2 GameViewSize() {
#if UNITY_EDITOR
    return XRRemoteUtils.GameViewSize();
#else
    return new Vector2(0.0f, 0.0f);
#endif
  }

  public void OnGUI() {
#if UNITY_EDITOR
    if (!timeout()) {
      InternalOnGUI();
    }
#endif
  }

  public MessageBuilder EditorAppInfo() {
#if UNITY_EDITOR
    return InternalEditorAppInfo();
#else
    return null;
#endif
  }

#if UNITY_EDITOR

  const int XR_SERVER_PORT = 23285;

  private XRInternalRemote remote_ = new XRInternalRemote();

  private DateTime startTime_;
  private String computerName_;
  private static bool HAS_ADB = EditorPrefs.GetString("AndroidSdkRoot") != String.Empty;
  private static String ADB_COMMAND = EditorPrefs.GetString("AndroidSdkRoot") + "/platform-tools/adb";
  private static bool ON_WINDOWS = SystemInfo.operatingSystem.Contains("Windows");

  private bool tick_ = false; // bools are atomic
  private bool threadRunning_ = false;
  private System.Threading.Thread thread_ = null;

  private UnityEditor.UIOrientation orientation_ = UnityEditor.UIOrientation.Portrait;

  // Return true if after timeout
  private bool timeout() {
    if (startTime_ == DateTime.MinValue) {
      startTime_ = System.DateTime.Now;
    }
    return (System.DateTime.Now - startTime_).TotalSeconds > GUI_TIMEOUT_IN_SECONDS;
  }

  private void InternalStart() {
    startTime_ = DateTime.MinValue; // on first Update it will assign to current time.
    orientation_ = UnityEditor.PlayerSettings.defaultInterfaceOrientation;
    computerName_ = XRRemoteUtils.getHostname();

    if (!XRRemoteUtils.SetupADB()) {
      remote_.Start(1);
      thread_ = new System.Threading.Thread(ThreadRun);
      thread_.Start();
      thread_.Name = "CHILD";
    }
  }

  private void ThreadRun() {
    threadRunning_ = true;
    while (threadRunning_) {
      if (tick_) {
        if (!remote_.IsConnected()) {
          XRRemoteUtils.SetupADB();
        }
        remote_.Update();
        tick_ = false;
      } else {
        System.Threading.Thread.Sleep(100);
      }
    }
  }

  private void Disconnect() {
    threadRunning_ = false;
    XRRemoteUtils.TeardownADB();
    remote_.Disconnect();
  }

  private void InternalOnGUI() {

    // previouslyConnected ? true : false
    int fontSize = Screen.height < 600 ? 14 : 18;

    GUIStyle title = new GUIStyle();
    title.fontSize = fontSize + 4;
    title.alignment = TextAnchor.MiddleCenter;
    title.normal.textColor = Color.white;

    GUIStyle info = new GUIStyle();
    info.fontSize = fontSize;
    info.wordWrap = true;
    info.alignment = TextAnchor.UpperCenter;
    info.fontStyle = FontStyle.Bold;
    info.normal.textColor = Color.white;

    GUIStyle link = new GUIStyle();
    link.fontSize = fontSize;
    link.wordWrap = true;
    link.alignment = TextAnchor.UpperCenter;
    link.fontStyle = FontStyle.Bold;
    link.normal.textColor = new Color32(254, 224, 25, 255);

    Texture2D texture = new Texture2D(1,1);
    texture.SetPixel(1,1, new Color32(90, 24, 142, 220));
    texture.Apply();

    var w = 0.7f * Screen.width;
    var h = 0.6f * Screen.height;
    Rect bounds = new Rect(0.15f*Screen.width, 0.2f*Screen.height, w, h);
    GUI.DrawTexture(bounds, texture);

    int spacer = Screen.height/3;
    GUILayout.BeginArea(bounds);

    EditorGUI.DropShadowLabel(new Rect(0, 0, w, h), "\nHost name:\n" + computerName_ + "\n"
      + "\nWaiting for remote\nto connect on\nUSB or WIFI", info);
    EditorGUI.DropShadowLabel(new Rect(0, spacer + 20, w, h), "Get the app:", info);
    EditorGUI.DropShadowLabel(new Rect(0, spacer + 40, w, h), "8thwall.com/remote", link);
    GUILayout.Space(spacer + 40);
    if (GUILayout.Button("                      ", link)) {
      Application.OpenURL("https://www.8thwall.com/remote");
    }

    EditorGUI.DropShadowLabel(new Rect(0, spacer + 120, w, h), "For help, visit", info);
    EditorGUI.DropShadowLabel(new Rect(0, spacer + 140, w, h), "docs.8thwall.com", link);
    GUILayout.Space(83);
    if (GUILayout.Button("                   ", link)) {
      Application.OpenURL("https://docs.8thwall.com");
    }
    GUILayout.EndArea();
  }

  private void BufferForSend(XrRemoteApp.Reader remote) {
    // Unity processes screen width 1440 @ 7fps, 770 @ 15fps, 360 @ 20fps
    remote_.SetDeviceInfo(
      remote.getDevice().getScreenWidth(),
      remote.getDevice().getScreenHeight(),
      (int)ToDeviceOrientation(remote.getDevice().getOrientation()));

    foreach (var t in remote.getTouches()) {
      XRInternalRemote.XrInternalRemoteData.Touch touch =
        new XRInternalRemote.XrInternalRemoteData.Touch();
      touch.positionX = t.getPositionX();
      touch.positionY = t.getPositionY();
      touch.frameCount = t.getTimestamp();
      touch.fingerId = t.getFingerId();
      touch.phase = (int)ToTouchPhase(t.getPhase());
      touch.tapCount = t.getTapCount();
      remote_.AddTouch(touch);
    }
  }

  private static DeviceOrientation ToDeviceOrientation(
    XrAppDeviceInfo.XrDeviceOrientation orientation) {
    switch (orientation) {
      case XrAppDeviceInfo.XrDeviceOrientation.PORTRAIT:
        return DeviceOrientation.Portrait;
      case XrAppDeviceInfo.XrDeviceOrientation.PORTRAIT_UPSIDE_DOWN:
        return DeviceOrientation.PortraitUpsideDown;
      case XrAppDeviceInfo.XrDeviceOrientation.LANDSCAPE_LEFT:
        return DeviceOrientation.LandscapeLeft;
      case XrAppDeviceInfo.XrDeviceOrientation.LANDSCAPE_RIGHT:
        return DeviceOrientation.LandscapeRight;
      case XrAppDeviceInfo.XrDeviceOrientation.FACE_UP:
        return DeviceOrientation.FaceUp;
      case XrAppDeviceInfo.XrDeviceOrientation.FACE_DOWN:
        return DeviceOrientation.FaceDown;
      default:
        return DeviceOrientation.Unknown;
    }
  }

  private static TouchPhase ToTouchPhase(XrTouch.XrTouchPhase phase) {
    switch (phase) {
      case XrTouch.XrTouchPhase.BEGAN:
        return TouchPhase.Began;
      case XrTouch.XrTouchPhase.MOVED:
        return TouchPhase.Moved;
      case XrTouch.XrTouchPhase.ENDED:
        return TouchPhase.Ended;
      case XrTouch.XrTouchPhase.STATIONARY:
        return TouchPhase.Stationary;
      case XrTouch.XrTouchPhase.CANCELLED:
        return TouchPhase.Canceled;
      default:
        // Touch phase has no default, just return the lowest value.
        return TouchPhase.Began;
    }
  }

  public MessageBuilder InternalEditorAppInfo() {
    MessageBuilder msg = new MessageBuilder();
    var builder = msg.getRoot(XrEditorAppInfo.factory);

    switch (orientation_) {
      case UnityEditor.UIOrientation.Portrait:
        builder.setScreenOrientation(XrAppDeviceInfo.XrScreenOrientation.PORTRAIT);
        break;
      case UnityEditor.UIOrientation.LandscapeLeft:
        builder.setScreenOrientation(XrAppDeviceInfo.XrScreenOrientation.LANDSCAPE_LEFT);
        break;
      case UnityEditor.UIOrientation.PortraitUpsideDown:
        builder.setScreenOrientation(XrAppDeviceInfo.XrScreenOrientation.PORTRAIT_UPSIDE_DOWN);
        break;
      case UnityEditor.UIOrientation.LandscapeRight:
        builder.setScreenOrientation(XrAppDeviceInfo.XrScreenOrientation.LANDSCAPE_RIGHT);
        break;
      default:
        break;
    }

    builder.setScreenPreview(remote_.ScreenPreview().getRoot(CompressedImageData.factory).asReader());
    return msg;
  }

  private class XRRemoteUtils {

    static object gameViewSizesInstance;
    static MethodInfo getGroup;

    static XRRemoteUtils() {
      var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
      var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
      var instanceProp = singleType.GetProperty("instance");
      getGroup = sizesType.GetMethod("GetGroup");
      gameViewSizesInstance = instanceProp.GetValue(null, null);
    }

    private static String ExecuteAdb(String arguments) {
      return ExecuteCommand(ADB_COMMAND, arguments);
    }

    public static String getHostname() {
#if UNITY_STANDALONE_OSX
      String computerName = ExecuteCommand("/usr/sbin/scutil", "--get ComputerName");
      if (computerName.Length > 0) {
        return computerName;
      }
#endif
      return System.Environment.MachineName.Split('.')[0];
    }

    private static String ExecuteCommand(String command, String arguments) {
      try {
        Process proc = new Process();
        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        proc.StartInfo.CreateNoWindow = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.FileName = command;
        proc.StartInfo.Arguments = arguments;
        proc.EnableRaisingEvents = true;
        proc.Start();

        StringBuilder sb = new StringBuilder();
        while (!proc.HasExited) {
          sb.Append(proc.StandardOutput.ReadToEnd());
        }
        proc.WaitForExit();
        if (proc.ExitCode != 0) {
          throw new Exception("\"" + command + " " + arguments + "\" exited with " + proc.ExitCode);
        }
        return sb.ToString();
      } catch (Exception e){
        UnityEngine.Debug.LogError(command + " threw exception " + e);
        return e.ToString();
      }
    }

    private static int GetAdbDeviceCount() {
      int count = ExecuteAdb("devices -l").Split(new string[] {" model:"}, StringSplitOptions.None).Length - 1;
      if (count > 1) {
        UnityEngine.Debug.Log("Found " + count + " attached ADB devices.  Only one device may be connected at a time to use as a remote.");
      }
      return count;
    }

    // Returns true if ADB is active (i.e., we have a USB connection)
    public static bool SetupADB() {
      if (!HAS_ADB) {
        return false;
      }
      // Only 1 ADB device is allowed.  For 0 or 2+ do nothing.
      if (GetAdbDeviceCount() != 1) {
        return false;
      }
      String forward = "tcp:" + XRInternalRemote.PLAYER_PORT;
      String reverse = "tcp:" + XR_SERVER_PORT;
      if (ExecuteAdb("reverse --list").IndexOf(reverse) < 0) {
        // Add reverse link for USB connection
        DebugLog("Add adb reverse " + reverse);
        ExecuteAdb("reverse " + reverse + " " + reverse);
      }
      if (ExecuteAdb("forward --list").IndexOf(forward) > 0) {
        DebugLog("Unity has detected an Android device and created an adb forward " + forward);
        if (ON_WINDOWS) {
          // On Windows the Unity remote server is on the phone
          // Do not remove the adb forward, return true to not
          // start the internal Unity remote server.
          return true;
        } else {
          // On MacOS the Unity remote server is always internal
          // Remove the Unity assigned adb forward so USB connections will
          // use the internal Unity remote server.
          DebugLog("Remove adb forward " + forward);
          ExecuteAdb("forward --remove " + forward);
          return false;
        }
      }
      return false;
    }

    public static void TeardownADB() {
      if (!HAS_ADB) {
        return;
      }
      String reverse = "tcp:" + XR_SERVER_PORT;
      if (GetAdbDeviceCount() == 1 &&
          ExecuteAdb("reverse --list").IndexOf(reverse) > 0) {
        DebugLog("Remove adb reverse " + reverse);
        ExecuteAdb("reverse --remove " + reverse);
      }

      if (ON_WINDOWS) {
        ExecuteAdb("kill-server");
      }
    }

    public static Vector2 GameViewSize() {
      var gvType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
      var GetSizeOfMainGameView = gvType.GetMethod("GetSizeOfMainGameView",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
      return (Vector2)GetSizeOfMainGameView.Invoke(null,null);
    }

    public enum GameViewSizeType {
        AspectRatio,
        FixedResolution
    }

    // This is needed to identify which Platform is governing the currently selected aspect
    // ratios. Typically this matches the build setting, but not always.
    public static GameViewSizeGroupType GetGameViewSizeGroupType() {
      var gvType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
      var currentSizeGroupTypeProp = gvType.GetProperty("currentSizeGroupType",
              BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      return (GameViewSizeGroupType)currentSizeGroupTypeProp.GetValue(null, null);
    }

    public static void SetGameViewAspectRatio(XrRemoteApp.Reader remote) {
      int width = remote.getDevice().getScreenWidth();
      int height = remote.getDevice().getScreenHeight();
      if (width == 0 || height == 0) {
        return;
      }

      GameViewSizeGroupType buildPlatform = GetGameViewSizeGroupType();
      string name = "XR Remote";

      if (!SizeExists(buildPlatform, width, height)) {
        AddCustomSize(GameViewSizeType.AspectRatio, buildPlatform, width, height, name);
      }

      SetSize(FindSize(buildPlatform, width, height));
    }

    public static void SetSize(int index) {
      var gvType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
      var selectedSizeIndexProp = gvType.GetProperty("selectedSizeIndex",
              BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      var gvWnd = EditorWindow.GetWindow(gvType);
      selectedSizeIndexProp.SetValue(gvWnd, index, null);
    }

    static object GetGroup(GameViewSizeGroupType type) {
      return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
    }

    public static void AddCustomSize(
      GameViewSizeType viewSizeType,
      GameViewSizeGroupType sizeGroupType,
      int width,
      int height,
      string text) {
      var group = GetGroup(sizeGroupType);
      var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
      var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
      var ctor = gvsType.GetConstructor(
        new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
      var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
      addCustomSize.Invoke(group, new object[] { newSize });
    }

    public static bool SizeExists(GameViewSizeGroupType sizeGroupType, string text) {
      return FindSize(sizeGroupType, text) != -1;
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, string text) {
      var group = GetGroup(sizeGroupType);
      var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
      var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
      for(int i = 0; i < displayTexts.Length; i++) {
        string display = displayTexts[i];
        // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
        // so if we're querying a custom size text we substring to only get the name
        // You could see the outputs by just logging
        // DebugLog(display);
        int pren = display.IndexOf('(');
        // -1 to remove the space that's before the parens. This is very implementation-depdenent
        if (pren != -1) {
          display = display.Substring(0, pren-1);
        }
        if (display == text) {
          return i;
        }
      }
      return -1;
    }

    public static bool SizeExists(GameViewSizeGroupType sizeGroupType, int width, int height) {
      return FindSize(sizeGroupType, width, height) != -1;
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height) {
      var group = GetGroup(sizeGroupType);
      var groupType = group.GetType();
      var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
      var getCustomCount = groupType.GetMethod("GetCustomCount");
      int sizesCount = (int)getBuiltinCount.Invoke(group, null)
       + (int)getCustomCount.Invoke(group, null);
      var getGameViewSize = groupType.GetMethod("GetGameViewSize");
      var gvsType = getGameViewSize.ReturnType;
      var widthProp = gvsType.GetProperty("width");
      var heightProp = gvsType.GetProperty("height");
      var indexValue = new object[1];
      for(int i = 0; i < sizesCount; i++)
      {
        indexValue[0] = i;
        var size = getGameViewSize.Invoke(group, indexValue);
        int sizeWidth = (int)widthProp.GetValue(size, null);
        int sizeHeight = (int)heightProp.GetValue(size, null);
        if (sizeWidth == width && sizeHeight == height)
          return i;
      }
      return -1;
    }

    static void PrintDebugInfo() {
      // Print all possible values for GameViewSizeGroupType Enum
      foreach (var value in Enum.GetValues(typeof(GameViewSizeGroupType))) {
        DebugLog((GameViewSizeGroupType) value + " - " + (int) value);
      }
      // Print the selected platform in build settings
      DebugLog("selectedBuildTargetGroup: " + EditorUserBuildSettings.selectedBuildTargetGroup +
                " - " + (int)EditorUserBuildSettings.selectedBuildTargetGroup);
    }
  }
#endif
}
}

