using c8;
using C8;
using Capnp;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace XRInternal {

public class XRNativeBridge {

  private bool enableRemote_ = false;
  public XRNativeBridge(bool enableRemote) {
    enableRemote_ = enableRemote;
    if (enableRemote_) {
      // silence no-read warning
    }
  }

  // Fallback interfaces for unsupported modes.
  void XRCreateUnsupported() { }
  void XRConfigureUnsupported() { }
  void XRResumeUnsupported() { }
  void XRRecenterUnsupported() { }
  void XRPauseUnsupported() { }
  void XRDestroyUnsupported() { }
  void XRSetManagedCameraTextureUnsupported(
    System.IntPtr texHandle, int width, int height, int renderingSystem) { }
  void XRRenderFrameForDisplayUnsupported() { }
  IntPtr XRGetRenderEventFuncUnsupported() { return IntPtr.Zero; }
  bool XRIsStreamingSupportedUnsupported() { return false; }
  bool XRIsRemoteConnectedUnsupported() { return false; }
  void XRSetEditorAppInfoUnsupported(MessageBuilder message) {}

  void XRGetCurrentRealityUnsupported() { }

  static XREnvironment.Reader XRGetEnvironmentUnsupported() {
    var builder = new MessageBuilder().initRoot(XREnvironment.factory);
    builder.setRealityImageWidth(ApiLimits.IMAGE_PROCESSING_WIDTH);
    builder.setRealityImageHeight(ApiLimits.IMAGE_PROCESSING_HEIGHT);
    return builder.asReader();
  }

  XrQueryResponse.Reader XRQueryUnsopported(MessageBuilder m) {
    return new MessageBuilder().initRoot(XrQueryResponse.factory).asReader();
  }

  void XRGetAppEnvironmentUnsupported() { }
  void XRSetAppEnvironmentUnsupported() { }

  void XRGetRemoteUnsupported() {
    if (xrRemote.getDevice().getScreenWidth() != 0) {
      return;
    }
    var builder = new MessageBuilder().initRoot(XrRemoteApp.factory);
    builder.getDevice().setScreenWidth(ApiLimits.IMAGE_PROCESSING_WIDTH);
    builder.getDevice().setScreenHeight(ApiLimits.IMAGE_PROCESSING_HEIGHT);
    builder.getDevice().setOrientation(XrAppDeviceInfo.XrDeviceOrientation.PORTRAIT);
    builder.getDevice().setScreenOrientation(XrAppDeviceInfo.XrScreenOrientation.PORTRAIT);
    xrRemote = builder.asReader();
  }

// iOS native interfaces.
#if (UNITY_IPHONE && !UNITY_EDITOR)
  [DllImport("__Internal")]
  private static extern void c8XRIos_create(int renderingSystem);

  [DllImport("__Internal")]
  private static extern void c8XRIos_configureXR(ref NativeByteArray config);

  [DllImport("__Internal")]
  private static extern void c8XRIos_resume();

  [DllImport("__Internal")]
  private static extern void c8XRIos_recenter();

  [DllImport("__Internal")]
  private static extern void c8XRIos_getCurrentRealityXR(ref NativeByteArray reality);

  [DllImport("__Internal")]
  private static extern void c8XRIos_query(
    ref NativeByteArray request, ref NativeByteArray response);

  [DllImport("__Internal")]
  private static extern void c8XRIos_getXREnvironment(ref NativeByteArray env);

  [DllImport("__Internal")]
  private static extern void c8XRIos_getXRAppEnvironment(ref NativeByteArray env);

  [DllImport("__Internal")]
  private static extern void c8XRIos_setXRAppEnvironment(ref NativeByteArray env);

  [DllImport("__Internal")]
  private static extern void c8XRIos_pause();

  [DllImport("__Internal")]
  private static extern void c8XRIos_destroy();

  [DllImport("__Internal")]
  private static extern void c8XRIos_setManagedCameraRGBATexture(
     System.IntPtr texHandle, int width, int height, int renderingSystem);

  [DllImport("__Internal")]
  private static extern void c8XRIos_setManagedCameraYTexture(
     System.IntPtr texHandle, int width, int height, int renderingSystem);

  [DllImport("__Internal")]
  private static extern void c8XRIos_setManagedCameraUVTexture(
     System.IntPtr texHandle, int width, int height, int renderingSystem);

  [DllImport("__Internal")]
  private static extern void c8XRIos_renderFrameForDisplay();

  [DllImport("__Internal")]
  private static extern IntPtr c8XRIos_getRenderEventFunc();

  void XRCreate(int renderingSystem) { c8XRIos_create(renderingSystem); }
  void XRConfigure(MessageBuilder configMessageBuilder) {
    NativeByteArray b = Serialize.allocNativeByteArrayAndWrite(configMessageBuilder);
    c8XRIos_configureXR(ref b);
    Serialize.freeAllocedNativeByteArray(b);
  }
  void XRResume() { c8XRIos_resume(); }
  void XRRecenter() { c8XRIos_recenter(); }
  void XRGetCurrentReality() {
    NativeByteArray bytes = new NativeByteArray();
    c8XRIos_getCurrentRealityXR(ref bytes);
    byte[] ret = new byte[bytes.size];
    Marshal.Copy(bytes.bytes, ret, 0, bytes.size);
    MessageReader r = Serialize.read(bytes);
    xrResponse = r.getRoot(RealityResponse.factory);
  }
  XrQueryResponse.Reader XRQuery(MessageBuilder m) {
    NativeByteArray requestBytes = Serialize.allocNativeByteArrayAndWrite(m);
    NativeByteArray responseBytes = new NativeByteArray();
    c8XRIos_query(ref requestBytes, ref responseBytes);
    Serialize.freeAllocedNativeByteArray(requestBytes);
    byte[] ret = new byte[responseBytes.size];
    Marshal.Copy(responseBytes.bytes, ret, 0, responseBytes.size);
    return Serialize.read(responseBytes).getRoot(XrQueryResponse.factory);
 }
  static XREnvironment.Reader XRGetEnvironment() {
    NativeByteArray bytes = new NativeByteArray();
    c8XRIos_getXREnvironment(ref bytes);
    byte[] ret = new byte[bytes.size];
    Marshal.Copy(bytes.bytes, ret, 0, bytes.size);
    MessageReader r = Serialize.read(bytes);
    return r.getRoot(XREnvironment.factory);
  }
  void XRGetAppEnvironment() {
    NativeByteArray bytes = new NativeByteArray();
    c8XRIos_getXRAppEnvironment(ref bytes);
    byte[] ret = new byte[bytes.size];
    Marshal.Copy(bytes.bytes, ret, 0, bytes.size);
    XRAppEnvironment.Reader r = Serialize.read(bytes).getRoot(XRAppEnvironment.factory);
    xrAppEnvironment.setRoot(XRAppEnvironment.factory, r);
  }
  void XRSetAppEnvironment() {
    NativeByteArray bytes = Serialize.allocNativeByteArrayAndWrite(xrAppEnvironment);
    c8XRIos_setXRAppEnvironment(ref bytes);
    Serialize.freeAllocedNativeByteArray(bytes);
  }
  void XRGetRemote() { XRGetRemoteUnsupported(); }
  bool XRIsStreamingSupported() { return XRIsStreamingSupportedUnsupported(); }
  bool XRIsRemoteConnected() { return XRIsRemoteConnectedUnsupported(); }
  void XRPause() { c8XRIos_pause(); }
  void XRDestroy() { c8XRIos_destroy();}
  void XRRenderFrameForDisplay() { c8XRIos_renderFrameForDisplay(); }
  void XRSetManagedCameraRGBATexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    c8XRIos_setManagedCameraRGBATexture(texHandle, width, height, renderingSystem);
  }
  void XRSetManagedCameraYTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    c8XRIos_setManagedCameraYTexture(texHandle, width, height, renderingSystem);
  }
  void XRSetManagedCameraUVTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    c8XRIos_setManagedCameraUVTexture(texHandle, width, height, renderingSystem);
  }
  IntPtr XRGetRenderEventFunc() {
    return c8XRIos_getRenderEventFunc();
  }
  void XRSetEditorAppInfo(MessageBuilder builder) { XRSetEditorAppInfoUnsupported(builder); }

// Android JNI interfaces.
#elif (UNITY_ANDROID && !UNITY_EDITOR)

  private XRAndroidWrapper xrAndroid;

  void XRCreate(int renderingSystem) { xrAndroid = XRAndroidWrapper.create(renderingSystem); }
  void XRConfigure(MessageBuilder configMessageBuilder) { xrAndroid.configureXR(configMessageBuilder); }
  void XRResume() { xrAndroid.resume(); }
  void XRRecenter() { xrAndroid.recenter(); }
  void XRGetCurrentReality() { xrResponse = xrAndroid.getCurrentRealityXR(); }
  XrQueryResponse.Reader XRQuery(MessageBuilder m) { return xrAndroid.query(m); }
  static XREnvironment.Reader XRGetEnvironment() { return XRAndroidWrapper.getXREnvironment(); }
  void XRGetAppEnvironment() {
    xrAppEnvironment.setRoot(XRAppEnvironment.factory, xrAndroid.getXRAppEnvironment());
  }
  void XRSetAppEnvironment() {
    xrAndroid.setXRAppEnvironment(xrAppEnvironment);
  }
  void XRGetRemote() { XRGetRemoteUnsupported(); }
  bool XRIsStreamingSupported() { return XRIsStreamingSupportedUnsupported(); }
  bool XRIsRemoteConnected() { return XRIsRemoteConnectedUnsupported(); }
  void XRPause() { xrAndroid.pause(); }
  void XRDestroy() { xrAndroid.destroy(); }
  void XRRenderFrameForDisplay() { xrAndroid.renderFrameForDisplay(); }
  void XRSetManagedCameraRGBATexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    xrAndroid.setManagedCameraRGBATexture(texHandle, width, height, renderingSystem);
  }
  void XRSetManagedCameraYTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    xrAndroid.setManagedCameraYTexture(texHandle, width, height, renderingSystem);
  }
  void XRSetManagedCameraUVTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    xrAndroid.setManagedCameraUVTexture(texHandle, width, height, renderingSystem);
  }
  IntPtr XRGetRenderEventFunc() { return c8XRAndroid_getRenderEventFunc(); }

  [DllImport ("XRPlugin")]
  private static extern IntPtr c8XRAndroid_getRenderEventFunc();

  void XRSetEditorAppInfo(MessageBuilder builder) { XRSetEditorAppInfoUnsupported(builder); }

// When playing in the Unity Editor
#elif (UNITY_EDITOR)
  [DllImport("XRStreamingPlugin")]
  private static extern IntPtr c8XRStreaming_setRenderingSystem(int renderingSystem);

  [DllImport("XRStreamingPlugin")]
  private static extern IntPtr c8XRStreaming_create();

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_destroy();

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_configureXR(ref NativeByteArray config);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_setManagedCameraRGBATexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_setManagedCameraYTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_setManagedCameraUVTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_resume();

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_recenter();

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_getCurrentRealityXR(ref NativeByteArray response);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_query(
    ref NativeByteArray request, ref NativeByteArray response);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_getXREnvironment(ref NativeByteArray env);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_getXRAppEnvironment(ref NativeByteArray env);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_setXRAppEnvironment(ref NativeByteArray env);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_getXRRemote(ref NativeByteArray remote);

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_pause();

  [DllImport("XRStreamingPlugin")]
  private static extern void c8XRStreaming_renderFrameForDisplay();

  [DllImport("XRStreamingPlugin")]
  private static extern IntPtr c8XRStreaming_getRenderEventFunc();

  [DllImport("XRStreamingPlugin")]
  private static extern IntPtr c8XRStreaming_setEditorAppInfo(ref NativeByteArray preview);

  enum StreamingSupport {
    UNSPECIFIED,
    UNSUPPORTED,
    SUPPORTED
  };

  StreamingSupport streamingSupported_ = StreamingSupport.UNSPECIFIED;

  bool IsMetalEditorSupportEnabled() {
    // Versions below 5.6 of Unity don't seem to allow direct access to this asset through
    // AssetdDatabase.LoadAssetAtPath. AssetDatabase.LoadAllAssetsAtPath, however, does provide
    // access. To ensure this is setup properly, we use the latter.
    UnityEngine.Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
    UnityEngine.Object asset = assets[0];

    if (asset == null) {
      Debug.Log("Error: Can't load Player Settings!");
      return false;
    }

    UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(asset);
    UnityEditor.SerializedProperty metalEditorSupport = so.FindProperty("metalEditorSupport");

    if (metalEditorSupport != null) {
      if (metalEditorSupport.boolValue) {
        return metalEditorSupport.boolValue;
      }
    }
    return false;
  }

  static readonly string versionInfoPath = "XR/8thWallXR.asset";
  StreamingSupport CheckStreamingSupport() {
    if (!enableRemote_) {
      return StreamingSupport.UNSUPPORTED;
    }

    string assetFilePath = Path.Combine(Application.dataPath, versionInfoPath);
    // In certain config, the auto-updater is unavailable
    if (File.Exists(assetFilePath)) {
      // Loading the version info from our asset
      UnityEngine.Object[] assetInfos = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(
        Path.Combine("Assets", versionInfoPath));
      if (assetInfos.Length > 0) {
        string version = null;
        foreach (var assetObj in assetInfos) {
          var versionField = assetObj.GetType().GetField("version");
          if (versionField != null) {
            version = (string)versionField.GetValue(assetObj);
            break;
          }
        }
        if (version != null) {
          XREnvironment.Reader pluginEnv = GetXREnvironment();
          if (pluginEnv.hasEngineInfo()) {
            string pluginVersion = pluginEnv.getEngineInfo().getXrVersion().ToString();
            // When the build system doesn't inject the version number, we get 0.0.0.0
            if (!pluginVersion.Equals("0.0.0.0") && (version != pluginVersion)) {
              // Version doesn't match up
              Debug.LogWarning("Restart Unity to re-enable 8th Wall remote in the " + 
                "editor. XR version " + version + " doesn't match the version loaded by Unity " + pluginVersion);
              return StreamingSupport.UNSUPPORTED;
            }
          }
        }
      }
    }

    var orientation = UnityEditor.PlayerSettings.defaultInterfaceOrientation;
    if (orientation == UnityEditor.UIOrientation.AutoRotation) {
      Debug.LogError(
        "For 8thWall XR, interface orientation must be fixed to portrait or landscape.");
      return StreamingSupport.UNSUPPORTED;
    }

    return StreamingSupport.SUPPORTED;
  }

  void XRCreate(int renderingSystem) {
    if (!XRIsStreamingSupported()) {
      XRCreateUnsupported();
      return;
    }
    c8XRStreaming_setRenderingSystem(renderingSystem);
    c8XRStreaming_create();
  }

  void XRDestroy() {
    if (!XRIsStreamingSupported()) {
      XRDestroyUnsupported();
      return;
    }
   c8XRStreaming_destroy();
  }

  void XRConfigure(MessageBuilder configMessageBuilder) {
    if (!XRIsStreamingSupported()) {
      XRConfigureUnsupported();
      return;
    }
    NativeByteArray b = Serialize.allocNativeByteArrayAndWrite(configMessageBuilder);
    c8XRStreaming_configureXR(ref b);
    Serialize.freeAllocedNativeByteArray(b);
  }

  void XRSetManagedCameraRGBATexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    if (!XRIsStreamingSupported()) {
      XRSetManagedCameraTextureUnsupported(texHandle, width, height, renderingSystem);
      return;
    }
    c8XRStreaming_setManagedCameraRGBATexture(texHandle, width, height, renderingSystem);
  }

  void XRSetManagedCameraYTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    if (!XRIsStreamingSupported()) {
      XRSetManagedCameraTextureUnsupported(texHandle, width, height, renderingSystem);
      return;
    }
    c8XRStreaming_setManagedCameraYTexture(texHandle, width, height, renderingSystem);
  }

  void XRSetManagedCameraUVTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    if (!XRIsStreamingSupported()) {
      XRSetManagedCameraTextureUnsupported(texHandle, width, height, renderingSystem);
      return;
    }
    c8XRStreaming_setManagedCameraUVTexture(texHandle, width, height, renderingSystem);
  }

  void XRResume() {
    if (!XRIsStreamingSupported()) {
      XRResumeUnsupported();
      return;
    }
   c8XRStreaming_resume();
  }

  void XRRecenter() {
    if (!XRIsStreamingSupported()) {
      XRRecenterUnsupported();
      return;
    }
    c8XRStreaming_recenter();
  }

  void XRGetCurrentReality() {
    if (!XRIsStreamingSupported()) {
      XRGetCurrentRealityUnsupported();
      return;
    }
    NativeByteArray bytes = new NativeByteArray();
    c8XRStreaming_getCurrentRealityXR(ref bytes);
    if (bytes.bytes == IntPtr.Zero) {
      return;
    }
    byte[] ret = new byte[bytes.size];
    Marshal.Copy(bytes.bytes, ret, 0, bytes.size);
    MessageReader r = Serialize.read(bytes);
    xrResponse = r.getRoot(RealityResponse.factory);
  }

  XrQueryResponse.Reader XRQuery(MessageBuilder m) {
    NativeByteArray requestBytes = Serialize.allocNativeByteArrayAndWrite(m);
    NativeByteArray responseBytes = new NativeByteArray();
    c8XRStreaming_query(ref requestBytes, ref responseBytes);
    Serialize.freeAllocedNativeByteArray(requestBytes);
    byte[] ret = new byte[responseBytes.size];
    Marshal.Copy(responseBytes.bytes, ret, 0, responseBytes.size);
    return Serialize.read(responseBytes).getRoot(XrQueryResponse.factory);
  }

  static XREnvironment.Reader XRGetEnvironment() {
    NativeByteArray bytes = new NativeByteArray();
    c8XRStreaming_getXREnvironment(ref bytes);
    byte[] ret = new byte[bytes.size];
    Marshal.Copy(bytes.bytes, ret, 0, bytes.size);
    MessageReader r = Serialize.read(bytes);
    return r.getRoot(XREnvironment.factory);
  }

  void XRGetAppEnvironment() {
    if (!XRIsStreamingSupported()) {
      XRGetAppEnvironmentUnsupported();
      return;
    }

    NativeByteArray bytes = new NativeByteArray();
    c8XRStreaming_getXRAppEnvironment(ref bytes);
    if (bytes.bytes == IntPtr.Zero) {
      return;
    }
    byte[] ret = new byte[bytes.size];
    Marshal.Copy(bytes.bytes, ret, 0, bytes.size);
    XRAppEnvironment.Reader r = Serialize.read(bytes).getRoot(XRAppEnvironment.factory);
    xrAppEnvironment.setRoot(XRAppEnvironment.factory, r);
  }

  void XRSetAppEnvironment() {
    if (!XRIsStreamingSupported()) {
      XRSetAppEnvironmentUnsupported();
      return;
    }

    NativeByteArray bytes = Serialize.allocNativeByteArrayAndWrite(xrAppEnvironment);
    c8XRStreaming_setXRAppEnvironment(ref bytes);
    Serialize.freeAllocedNativeByteArray(bytes);
  }

  void XRGetRemote() {
    if (!XRIsStreamingSupported()) {
      XRGetRemoteUnsupported();
      return;
    }

    NativeByteArray bytes = new NativeByteArray();
    c8XRStreaming_getXRRemote(ref bytes);
    byte[] ret = new byte[bytes.size];
    Marshal.Copy(bytes.bytes, ret, 0, bytes.size);
    MessageReader r = Serialize.read(bytes);
    xrRemote = r.getRoot(XrRemoteApp.factory);
  }

  void XRPause() {
    if (!XRIsStreamingSupported()) {
      XRPauseUnsupported();
      return;
    }
   c8XRStreaming_pause();
  }

  void XRRenderFrameForDisplay() {
    if (!XRIsStreamingSupported()) {
      XRRenderFrameForDisplayUnsupported();
      return;
    }
    c8XRStreaming_renderFrameForDisplay();
  }

  IntPtr XRGetRenderEventFunc() {
    if (!XRIsStreamingSupported()) {
      return XRGetRenderEventFuncUnsupported();
    }
    return c8XRStreaming_getRenderEventFunc();
  }

  bool XRIsStreamingSupported() {
    if (streamingSupported_ == StreamingSupport.UNSPECIFIED) {
      streamingSupported_ = CheckStreamingSupport();
    }
    return streamingSupported_ == StreamingSupport.SUPPORTED;
  }

  bool XRIsRemoteConnected() {
    if (!XRIsStreamingSupported()) {
      return false;
    }

   return XRGetEnvironment().getRealityImageWidth() > 0;
  }

  void XRSetEditorAppInfo(MessageBuilder message) {
    if (!XRIsStreamingSupported()) {
      XRSetEditorAppInfoUnsupported(message);
      return;
    }
    var b = Serialize.allocNativeByteArrayAndWrite(message);
    c8XRStreaming_setEditorAppInfo(ref b);
    Serialize.freeAllocedNativeByteArray(b);
  }

// Unsupported
#else
  void XRCreate(int renderingSystem) { XRCreateUnsupported(); }
  void XRConfigure(MessageBuilder configMessageBuilder) { XRConfigureUnsupported(); }
  void XRResume() { XRResumeUnsupported(); }
  void XRRecenter() { XRRecenterUnsupported(); }
  void XRGetCurrentReality() { XRGetCurrentRealityUnsupported(); }
  XrQueryResponse.Reader XRQuery(MessageBuilder m) { return XRQueryUnsopported(m); }
  static XREnvironment.Reader XRGetEnvironment() { return XRGetEnvironmentUnsupported(); }
  void XRGetAppEnvironment() { XRGetAppEnvironmentUnsupported(); }
  void XRSetAppEnvironment() { XRSetAppEnvironmentUnsupported(); }
  void XRGetRemote() { XRGetRemoteUnsupported(); }
  bool XRIsStreamingSupported() { return XRIsStreamingSupportedUnsupported(); }
  bool XRIsRemoteConnected() { return XRIsRemoteConnectedUnsupported(); }
  void XRPause() { XRPauseUnsupported(); }
  void XRDestroy() { XRDestroyUnsupported(); }
  void XRSetManagedCameraRGBATexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    XRSetManagedCameraTextureUnsupported(texHandle, width, height, renderingSystem);
  }
  void XRSetManagedCameraYTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    XRSetManagedCameraTextureUnsupported(texHandle, width, height, renderingSystem);
  }
  void XRSetManagedCameraUVTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    XRSetManagedCameraTextureUnsupported(texHandle, width, height, renderingSystem);
  }
  void XRRenderFrameForDisplay() { XRRenderFrameForDisplayUnsupported(); }
  IntPtr XRGetRenderEventFunc() { return XRGetRenderEventFuncUnsupported(); }
  void XRSetEditorAppInfo(MessageBuilder builder) { XRSetEditorAppInfoUnsupported(builder); }

#endif  // UNITY_IPHONE or UNITY_ANDROID

  private MessageBuilder xrAppEnvironment;
  private MessageBuilder xrConfig;

  private bool configured;
  private bool running;
  private RealityResponse.Reader xrResponse;
  private XrQueryResponse.Reader queryResponse;
  private XrRemoteApp.Reader xrRemote;

  public void Create(int renderingSystem) {
    running = false;

    xrAppEnvironment = new MessageBuilder();
    xrAppEnvironment.initRoot(XRConfiguration.factory);

    var xrBuilder =  new MessageBuilder().initRoot(RealityResponse.factory);
    xrBuilder.getEventId().setEventTimeMicros(600000000000);
    xrBuilder.getXRResponse().getCamera().getExtrinsic().getRotation().setW(1.0f);
    xrBuilder.getXRResponse().getCamera().getIntrinsic().initMatrix44f(16);
    xrBuilder.getXRResponse().getCamera().getIntrinsic().getMatrix44f().set(0, 2.92424f);
    xrBuilder.getXRResponse().getCamera().getIntrinsic().getMatrix44f().set(5, 1.64488f);
    xrBuilder.getXRResponse().getCamera().getIntrinsic().getMatrix44f().set(9, 0.0015625f);
    xrBuilder.getXRResponse().getCamera().getIntrinsic().getMatrix44f().set(10, -1.0006f);
    xrBuilder.getXRResponse().getCamera().getIntrinsic().getMatrix44f().set(11, -1.0f);
    xrBuilder.getXRResponse().getCamera().getIntrinsic().getMatrix44f().set(14, -0.60018f);

    xrResponse = xrBuilder.asReader();
    xrRemote = new MessageBuilder().initRoot(XrRemoteApp.factory).asReader();
    XRCreate(renderingSystem);
  }

  /** @param newConfig a MessageBuilder that has a XRConfiguration
   */
  public void CommitConfiguration(MessageBuilder newConfigMessageBuilder) {
    XRConfigure(newConfigMessageBuilder);
  }

  public XRAppEnvironment.Builder GetMutableXRAppEnvironment() { return xrAppEnvironment.getRoot(XRAppEnvironment.factory); }

  public void CommitAppEnvironment() {
    XRSetAppEnvironment();
  }

  public void SetManagedCameraRGBATexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    XRSetManagedCameraRGBATexture(texHandle, width, height, renderingSystem);
  }

  public void SetManagedCameraYTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    XRSetManagedCameraYTexture(texHandle, width, height, renderingSystem);
  }

  public void SetManagedCameraUVTexture(
    System.IntPtr texHandle, int width, int height, int renderingSystem) {
    XRSetManagedCameraUVTexture(texHandle, width, height, renderingSystem);
  }

  public void Resume() {
    if (running) {
      return;
    }
    running = true;
    XRResume();
  }

  public void Recenter() {
    XRRecenter();
  }

  public RealityResponse.Reader GetCurrentRealityXR() {
    XRGetCurrentReality();
    return xrResponse;
  }

  public XrQueryResponse.Reader Query(MessageBuilder m) { return XRQuery(m); }

  public static XREnvironment.Reader GetXREnvironment() {
    return XRGetEnvironment();
  }

  public XRAppEnvironment.Reader GetXRAppEnvironment() {
    XRGetAppEnvironment();
    return xrAppEnvironment.getRoot(XRAppEnvironment.factory).asReader();
  }

  public XrRemoteApp.Reader GetXRRemote() {
    XRGetRemote();
    return xrRemote;
  }

  public void SetEditorAppInfo(MessageBuilder message) {
    XRSetEditorAppInfo(message);
  }

  public bool IsStreamingSupported() {
    return XRIsStreamingSupported();
  }

  public bool IsRemoteConnected() {
    return XRIsRemoteConnected();
  }

  public IntPtr GetRenderEventFunc() { return XRGetRenderEventFunc(); }

  public void RenderFrameForDisplay() { XRRenderFrameForDisplay(); }

  public void Pause() {
    if (!running) {
      return;
    }
    XRPause();
    running = false;
  }

  public void Destroy() { XRDestroy(); }
}

}  // namespace XRInternal
