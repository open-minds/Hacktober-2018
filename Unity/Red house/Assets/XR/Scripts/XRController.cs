using c8;
using C8;
using Capnp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRInternal;

public class XRController : MonoBehaviour {
  protected XRNativeBridge bridge;
  private XREditorBridge editorBridge;
  private bool running;
  private long lastRealityMicros;
  private long updateNumber;
  // Either the Remote, RGBA, or the (Y+UV) textures should be set.
  private Texture2D realityRGBATexture = null;
  private Texture2D realityYTexture = null;
  private Texture2D realityUVTexture = null;
  private RealityResponse.Reader currentXRResponse = null;
  private Dictionary<long, XRSurface> currentSurfaces = null;
  private XREnvironment.Reader xrEnvironment = null;
  private long currentRealityUpdateNumber;
  private long currentSurfacesUpdateNumber;
  private Camera cam = null;
  private Vector3 origin = new Vector3(0, 0, 0);
  private Quaternion facing = new Quaternion(0, 0, 0, 1);
  private float scale = 1.0f;
  private bool explicitlyPaused = false;
  private bool startedInUnity = false;
  private string overrideAppKey = null;
  private bool remoteConnected = false;
  private Dictionary<String, XRDetectionImage> detectionImages_ = null;
  private Dictionary<long, XRSurface> xrSurfaceMap_ = new Dictionary<long, XRSurface>();
  private bool disableNativeAr = false;
  private float captureAspect = 0.0f;


  /**
   * Allow the 8thWall Remote app to stream AR data to the Unity editor. This value can only be
   * changed prior to pressing 'play' in the Unity Editor.
   */
  [Tooltip("Allow the 8thWall Remote app to stream AR data to the Unity editor")]
  public bool enableRemote = true;

  /**
   * Only use the XR controller in this scene to enable development with the 8thWall Remote app;
   * Don't enable any AR features.
   */
  [Tooltip("Only use XR for remote development in this scene; don't turn on AR.")]
  public bool remoteOnly = false;

  /**
   * Enable lighting estimation in the AR engine. Changes to this value will take place after the
   * next call to ConfigureXR.
   */
  [Tooltip("Enable lighting estimation in the AR engine")]
  public bool enableLighting = true;

  /**
   * Enable camera motion estimation in the AR engine. Changes to this value will take place after
   * the next call to ConfigureXR.
   */
  [Tooltip("Enable camera motion estimation in the AR engine")]
  public bool enableCamera = true;

  /**
   * Enable horizontal surface finding in the AR engine. Changes to this value will take place after
   * the next call to ConfigureXR.
   */
  [Tooltip("Enable horizontal surface finding in the AR engine")]
  public bool enableSurfaces = true;

  /**
   * Enable vertical surface finding in the AR engine. Changes to this value will take place after
   * the next call to ConfigureXR.
   */
  [Tooltip("Enable vertical surface finding in the AR engine")]
  public bool enableVerticalSurfaces = false;

  /**
   * Allow the camera to autofocus if it's able. This may improve the quality of the camera feed
   * but might decrease tracking performance. Not all AR engines support auto focus.
   */
  [Tooltip("Allow the camera to autofocus")]
  public bool enableCameraAutofocus = false;

  /**
   * Indicates that AR related features are currently disabled when playing a scene in the Editor.
   * AR features can be disabled because "enableRemote" is set to false, or because there is no
   * connected 8th Wall Remote. When running on device, this always returns false (i.e. AR features
   * are always enabled).
   */
  public bool DisabledInEditor() {
  #if UNITY_EDITOR
    return !(EnableRemote() && remoteConnected);
  #else
    return false;
  #endif
  }

  /**
   * Get the instrinsic matrix of the Camera. This specifies how to set the field of view of the
   * unity camera so that digital objects are properly overlayed on top of the AR camera feed. The
   * returned intrinsic matrix is suitable for the Unity Camera that was previously configured using
   * UpdateCameraProjectionMatrix.
   */
  public Matrix4x4 GetCameraIntrinsics() {
    if (cam == null) {
      return Camera.main.projectionMatrix;
    }

    var r = GetCurrentReality();

    if (r.getEventId().getEventTimeMicros() == 0) {
      return cam.projectionMatrix;
    }

    Matrix4x4 np = cam.projectionMatrix;

    var intrinsics = r.getXRResponse().getCamera().getIntrinsic().getMatrix44f();

    for (int i = 0; i < 4; ++i) {
      for (int j = 0; j < 4; ++j) {
        np[i, j] = intrinsics.get(j * 4 + i);
      }
    }

    return np;
  }

  /**
   * Get the position of the Camera in Unity's world coordinate system.
   */
  public Vector3 GetCameraPosition() {
    var r = GetCurrentReality();
    if (r.getEventId().getEventTimeMicros() == 0) {
      return origin;
    }
    var pos =  r.getXRResponse().getCamera().getExtrinsic().getPosition();
    return new Vector3(pos.getX(), pos.getY(), pos.getZ());
  }

  /**
   * Returns tracking state (and reason, if applicable) of underlying AR engine as an
   * XRTrackingState struct.
   */
  public XRTrackingState GetTrackingState() {
    var re = GetCurrentReality();
    var status = re.getXRResponse().getCamera().getTrackingState().getStatus();
    var reason = re.getXRResponse().getCamera().getTrackingState().getReason();

    if (re.getEventId().getEventTimeMicros() == 0) {
      return new XRTrackingState(
        XRTrackingState.Status.LIMITED,
        XRTrackingState.Reason.INITIALIZING);
    }

    XRTrackingState.Status s = XRTrackingState.Status.UNSPECIFIED;
    XRTrackingState.Reason r = XRTrackingState.Reason.UNSPECIFIED;

    switch (status) {
      case XrTrackingState.XrTrackingStatus.NOT_AVAILABLE:
        s = XRTrackingState.Status.NOT_AVAILABLE;
        break;
      case XrTrackingState.XrTrackingStatus.LIMITED:
        s = XRTrackingState.Status.LIMITED;
        break;
      case XrTrackingState.XrTrackingStatus.NORMAL:
        s = XRTrackingState.Status.NORMAL;
        break;
      default:
        break;
    }

    switch (reason) {
      case XrTrackingState.XrTrackingStatusReason.INITIALIZING:
        r = XRTrackingState.Reason.INITIALIZING;
        break;
      case XrTrackingState.XrTrackingStatusReason.RELOCALIZING:
        r = XRTrackingState.Reason.RELOCALIZING;
        break;
      case XrTrackingState.XrTrackingStatusReason.TOO_MUCH_MOTION:
        r = XRTrackingState.Reason.TOO_MUCH_MOTION;
        break;
      case XrTrackingState.XrTrackingStatusReason.NOT_ENOUGH_TEXTURE:
        r = XRTrackingState.Reason.NOT_ENOUGH_TEXTURE;
        break;
      default:
        break;
    }
    return new XRTrackingState(s, r);
  }

  /**
   * Get the rotation of the Camera in Unity's world coordinate system.
   */
  public Quaternion GetCameraRotation() {
    var r = GetCurrentReality();
    var rot = r.getXRResponse().getCamera().getExtrinsic().getRotation();
    if (rot.getX() == 0.0f && rot.getY() == 0.0f && rot.getZ() == 0.0f && rot.getW() == 0.0f) {
      return Quaternion.identity;
    }
    if (r.getEventId().getEventTimeMicros() == 0) {
      return facing;
    }
    return new Quaternion(rot.getX(), rot.getY(), rot.getZ(), rot.getW());
  }

  /**
   * Deprecated in XR 8.0.
   * Replace with UpdateCameraProjectionMatrix(cam, origin, new Quaternion(0, 0, 0, 1), scale).
   *
   * Configure the XRController for the unity scene.
   *
   * The Camera provides information about how AR overlay data will be presented, so that subsequent
   * calls to GetCameraIntrinsics return appropriate values. Origin specifies an initial camera
   * position in the virtual scene so that the virtual scene can be properly aligned to the real
   * world. Scale provides information about how units in Unity's coordinate system relate to
   * distances in the real world.
   *
   * For example, if scale is set to 10, moving the device 1 physical meter will cause the unity
   * Camera to move by 10 unity units, while moving the device by 10cm will cause the unity Camera
   * to move by 1 unity unit. Note that scale only applies when GetCapabilities().positionTracking
   * is PositionTracking.ROTATION_AND_POSITION. When a device uses
   * PositionTracking.ROTATION_AND_POSITION_NO_SCALE, the scene is scaled by the height of the
   * origin value.
   */
  public void UpdateCameraProjectionMatrix(Camera cam, Vector3 origin, float scale) {
    UpdateCameraProjectionMatrix(cam, origin, new Quaternion(0, 0, 0, 1), scale);
  }

  /**
   * Configure the XRController for the unity scene.
   *
   * The Camera provides information about how AR overlay data will be presented, so that subsequent
   * calls to GetCameraIntrinsics return appropriate values. Origin and Facing specify an initial
   * camera position and orientation in the virtual scene so that the virtual scene can be properly
   * aligned to the real world.
   *
   * When the engine is started, the camera will start in the scene at the provided origin, facing
   * along the x/z direction as specified by facing. Tilts and in-plane rotations in the facing
   * rotation are ignored. Scale provides information about how units in Unity's coordinate system
   * relate to distances in the real world.
   *
   * For example, if scale is set to 10, moving the device 1 physical meter will cause the unity
   * Camera to move by 10 unity units, while moving the device by 10cm will cause the unity Camera
   * to move by 1 unity unit. Note that scale only applies when GetCapabilities().positionTracking
   * is PositionTracking.ROTATION_AND_POSITION. When a device uses
   * PositionTracking.ROTATION_AND_POSITION_NO_SCALE, the scene is scaled by the height of the
   * origin value.
   */
  public void UpdateCameraProjectionMatrix(
    Camera cam, Vector3 origin, Quaternion facing, float scale) {
    if (!GetCapabilities().IsPositionTrackingRotationAndPosition() || disableNativeAr) {
      float scaleAdjustment = origin.y;
      this.scale = scaleAdjustment;
    } else {
       this.scale = scale;
    }
    this.origin = origin;
    this.facing = facing;
    this.cam = cam;
    ConfigureXR();
  }

  /**
   * Get the amount that a camera feed texture should be rotated to appear upright in a given app's
   * UI based on the app's orientation (e.g. portrait or landscape right) on the current device.
   */
  public XRTextureRotation GetTextureRotation() {
    var r = GetCurrentReality();
    switch (r.getAppContext().getRealityTextureRotation()) {
      case c8.AppContext.RealityTextureRotation.R0:
        return XRTextureRotation.R0;
      case c8.AppContext.RealityTextureRotation.R90:
        return XRTextureRotation.R90;
      case c8.AppContext.RealityTextureRotation.R180:
        return XRTextureRotation.R180;
      case c8.AppContext.RealityTextureRotation.R270:
        return XRTextureRotation.R270;
      default:
        return XRTextureRotation.UNSPECIFIED;
    }
  }

  /**
   * Returns the exposure of the environment as a value in the range -1 to 1.
   */
  public float GetLightExposure() {
    var r = GetCurrentReality();
    return r.getXRResponse().getLighting().getGlobal().getExposure();
  }

  /**
   * Returns the light temperature of the environment. Temperature measures the color of light
   * on a red to blue spectrum where low values (around 1000) are very red, high values (around
   * 15000) are very blue, and values around 6500 are close to white.
   */
  public float GetLightTemperature() {
    var r = GetCurrentReality();
    if (r.getEventId().getEventTimeMicros() == 0) {
      return 6500.0f;
    }
    return r.getXRResponse().getLighting().getGlobal().getTemperature();
  }

  /**
   * Returns the id of the currently active surface, or 0 if there is no active surface. The active
   * surface is the detected surface that is currently in the center of the device's camera feed.
   */
  public long GetActiveSurfaceId() {
    var r = GetCurrentReality();
    return r.getXRResponse().getSurfaces().getActiveSurface().getId().getEventTimeMicros();
  }

  /**
   * Deprecated in XR 7.0.
   * Replaced by: xr.GetSurface(xr.GetActiveSurfaceId()).mesh
   *
   * Returns the Mesh of the active surface, or null if there is no active surface.
   */
  public Mesh GetActiveSurfaceMesh() {
    return GetSurface(GetActiveSurfaceId()).mesh;
  }

  /**
   * Deprecated in XR 7.0.
   * Replaced by: xr.GetSurface(id).mesh
   *
   * Returns the Mesh of the surface with the requested ID, or null if no surface with that id
   * exists.
   */
  public Mesh GetSurfaceWithId(long id) {
    return GetSurface(id).mesh;
  }

  /**
   * Returns the XRSurface of the surface with the requested ID, or XRSurface.NO_SURFACE if no
   * surface with that id exists.
   */
  public XRSurface GetSurface(long id) {
    Dictionary<long, XRSurface> surface = GetSurfaceMap();
    if (!surface.Any()) {
      return XRSurface.NO_SURFACE;
    }
    return surface.ContainsKey(id) ? surface[id] : XRSurface.NO_SURFACE;
  }

  /**
   * Returns all surfaces known to the AR engine.
   */
  public List<XRSurface> GetSurfaces() {
    return new List<XRSurface>(GetSurfaceMap().Values);
  }

  /**
   * Returns aspect ratio (width/height) of captured image.
   */
  public float GetRealityTextureAspectRatio() {
    if (captureAspect > 0.0f) {
      return captureAspect;
    }

    Texture2D realityTexture = ShouldUseRealityRGBATexture()
      ? GetRealityRGBATexture()
      : GetRealityYTexture();

    return (float)realityTexture.width / (float)realityTexture.height;
  }

  /**
   * Returns if capture feed is encoded as a single RGBA texture (e.g. ARCore). If false, the
   * capture feed is stored in two separate textures containing the Y and UV color components. These
   * two textures should be combined using an appropriate shader prior to display.
   */
  public bool ShouldUseRealityRGBATexture() {
  #if (UNITY_IPHONE && !UNITY_EDITOR)
    return false;
  #elif (UNITY_ANDROID && !UNITY_EDITOR)
    // ARCore devices still use RGBA texture.
    return (xrEnvironment.getRealityImageShader() == XREnvironment.ImageShaderKind.ARCORE);
  #elif (UNITY_EDITOR || UNITY_STANDALONE_OSX)
    return true;
  #else
    return false; // Unsupported
  #endif
  }

  /**
   * Returns what the device's camera is capturing as an RGBA texture.
   */
  public Texture2D GetRealityRGBATexture() {
    var appEnvironment = bridge.GetXRAppEnvironment();
    var envTex = appEnvironment.getManagedCameraTextures().getRgbaTexture();
    if (realityRGBATexture != null
        && realityRGBATexture.height == envTex.getHeight()
        && realityRGBATexture.width == envTex.getWidth()) {
      return realityRGBATexture;
    }

    // Create a texture
    IntPtr envTexPtr = (IntPtr)envTex.getPtr();
    if (envTexPtr != IntPtr.Zero) {
      realityRGBATexture = Texture2D.CreateExternalTexture(
        envTex.getWidth(),
        envTex.getHeight(),
        TextureFormat.RGBA32,
        false,
        false,
        envTexPtr);
    } else {
      realityRGBATexture = new Texture2D(
        envTex.getWidth(),
        envTex.getHeight(),
        TextureFormat.RGBA32,
        false);
    }

    captureAspect = envTex.getWidth() * 1.0f / envTex.getHeight();

    // Set point filtering just so we can see the pixels clearly
    realityRGBATexture.filterMode = FilterMode.Point;
    // Call Apply() so it's actually uploaded to the GPU
    realityRGBATexture.Apply();

    if (envTexPtr == IntPtr.Zero) {
      // Pass texture pointer to the plugin only if it doesn't manage one itself.
      bridge.SetManagedCameraRGBATexture(
          realityRGBATexture.GetNativeTexturePtr(),
          realityRGBATexture.width,
          realityRGBATexture.height,
          GetRenderingSystem());
    }

    return realityRGBATexture;
  }

  /**
   * Set App Key. Needs to be set prior to Start() in the Unity lifecycle (i.e. in Awake or
   * OnEnable). You will need to create a unique license key on 8thWall.com for each 8th Wall app
   * that you develop.
   */
  public void SetAppKey(string key) {
    overrideAppKey = key;
  }

  /**
   * Returns what the device's camera is capturing as a Y texture stored in the R channel.
   */
  public Texture2D GetRealityYTexture() {
    if (realityYTexture != null) {
      return realityYTexture;
    }

    // Create a texture
    var appEnvironment = bridge.GetXRAppEnvironment();
    var envTex = appEnvironment.getManagedCameraTextures().getYTexture();
    IntPtr envTexPtr = (IntPtr)envTex.getPtr();

    if (envTexPtr != IntPtr.Zero) {
      realityYTexture = Texture2D.CreateExternalTexture(
        envTex.getWidth(),
        envTex.getHeight(),
        TextureFormat.R8,
        false,
        false,
        envTexPtr);
    } else {
      int w = envTex.getWidth();
      int h = envTex.getHeight();
      realityYTexture = new Texture2D(w, h, TextureFormat.R8, false);
      byte[] blackBytes = new byte[w * h];
      for (int i = 0; i < blackBytes.Length; i++) {
        blackBytes[i] = 0;
      }
      realityYTexture.LoadRawTextureData(blackBytes);
      realityYTexture.Apply();
    }

    captureAspect = envTex.getWidth() * 1.0f / envTex.getHeight();

    // Set point filtering just so we can see the pixels clearly
    realityYTexture.filterMode = FilterMode.Point;
    // Call Apply() so it's actually uploaded to the GPU
    realityYTexture.Apply();

    if (envTexPtr == IntPtr.Zero) {
      // Pass texture pointer to the plugin only if it doesn't manage one itself.
      bridge.SetManagedCameraYTexture(
          realityYTexture.GetNativeTexturePtr(),
          realityYTexture.width,
          realityYTexture.height,
          GetRenderingSystem());
    }

    return realityYTexture;
  }

  /**
   * Returns what the device's camera is capturing as a UV texture stored in the RG channels.
   */
  public Texture2D GetRealityUVTexture() {
    if (realityUVTexture != null) {
      return realityUVTexture;
    }

    // Create a texture
    var appEnvironment = bridge.GetXRAppEnvironment();
    var envTex = appEnvironment.getManagedCameraTextures().getUvTexture();
    IntPtr envTexPtr = (IntPtr)envTex.getPtr();

    if (envTexPtr != IntPtr.Zero) {
      realityUVTexture = Texture2D.CreateExternalTexture(
        envTex.getWidth(),
        envTex.getHeight(),
        TextureFormat.RG16,
        false,
        false,
        envTexPtr);
    } else {
      int w = envTex.getWidth();
      int h = envTex.getHeight();
      realityUVTexture = new Texture2D(w, h, TextureFormat.RG16, false);
      byte[] grayBytes = new byte[w * h * 2];
      for (int i = 0; i < grayBytes.Length; i++) {
        grayBytes[i] = 128;
      }
      realityUVTexture.LoadRawTextureData(grayBytes);
      realityUVTexture.Apply();
    }
    // Set point filtering just so we can see the pixels clearly
    realityUVTexture.filterMode = FilterMode.Point;
    // Call Apply() so it's actually uploaded to the GPU
    realityUVTexture.Apply();

    if (envTexPtr == IntPtr.Zero) {
      // Pass texture pointer to the plugin only if it doesn't manage one itself.
      bridge.SetManagedCameraUVTexture(
          realityUVTexture.GetNativeTexturePtr(),
          realityUVTexture.width,
          realityUVTexture.height,
          GetRenderingSystem());
    }

    return realityUVTexture;
  }

  /**
   * Returns the appropriate Video shader for drawing the AR scene background.
   */
  public Shader GetVideoShader() {
    if (xrEnvironment == null) {
      return Shader.Find("Unlit/XRCameraYUVShader");
    }
    switch(xrEnvironment.getRealityImageShader()) {
      case XREnvironment.ImageShaderKind.ARCORE:
        return Shader.Find("Unlit/ARCoreCameraShader");
      default:
        return (ShouldUseRealityRGBATexture()
            ? Shader.Find("Unlit/XRCameraRGBAShader")
            : Shader.Find("Unlit/XRCameraYUVShader"));
    }
  }

  /**
   * Returns the appropriate Video texture shader for drawing AR video textures on objects.
   */
  public Shader GetVideoTextureShader() {
    switch(xrEnvironment.getRealityImageShader()) {
      case XREnvironment.ImageShaderKind.ARCORE:
        return Shader.Find("Unlit/ARCoreTextureShader");
      default:
        return (ShouldUseRealityRGBATexture()
            ? Shader.Find("Unlit/XRTextureRGBAShader")
            : Shader.Find("Unlit/XRTextureYUVShader"));;
    }
  }

  /**
   * For Non-ARKit/ARCore phones, reset surface to original position relative to camera.
   */
  public void Recenter() {
    if (GetCapabilities().IsSurfaceEstimationFixedSurfaces()) {
      bridge.Recenter();
    }
  }

  /**
   * Returns the AR capabilities available to the device, e.g. position tracking and surface
   * estimation.
   */
  public XRCapabilities GetCapabilities() {
    return GetDeviceCapabilities();
  }

  /**
   * Estimate the 3D position (in unity units) of a point on the camera feed. X and Y are specified
   * as numbers between 0 and 1, where (0, 0) is the upper left corner and (1, 1) is the lower right
   * corner of the camera feed as rendered in the camera that was specified with
   * UpdateCameraProjectionMatrix.
   *
   * Mutltiple 3d position esitmates may be returned for a single hit test based on the source
   * of data being used to estimate the position. The data source that was used to estimate the
   * position is indicated by the XRHitTestResult.Type.
   *
   * Example usage:
   *
   * List<XRHitTestResult> hits = new List<XRHitTestResult>();
   * if (Input.touchCount != 0) {
   *   var t = Input.GetTouch(0);
   *   if (t.phase == TouchPhase.Began) {
   *     float x = t.position.x / Screen.width;
   *     float y = (Screen.height - t.position.y) / Screen.height;
   *     hits.AddRange(xr.HitTest(x, y));
   *   }
   * }
   */
  public List<XRHitTestResult> HitTest(float x, float y) {
    List<XRHitTestResult.Type> types = new List<XRHitTestResult.Type>();
    return HitTest(x, y, types);
  }

  /**
   * Estimate the 3D position (in unity units) of a point on the camera feed, optionally filtering
   * the results by the source of information that is used to estimate the 3d position. If no
   * types are specified, all hit test results are returned.
   *
   * X and Y are specified as numbers between 0 and 1, where (0, 0) is the upper left corner and
   * (1, 1) is the lower right corner of the camera feed as rendered in the camera that was
   * specified with UpdateCameraProjectionMatrix.
   *
   * Mutltiple 3d position esitmates may be returned for a single hit test based on the source
   * of data being used to estimate the position. The data source that was used to estimate the
   * position is indicated by the XRHitTestResult.Type.
   *
   * Example usage:
   *
   * List<XRHitTestResult> hits = new List<XRHitTestResult>();
   * List<XRHitTestResult.Type> types = new List<XRHitTestResult.Type>();
   * types.Add(XRHitTestResult.Type.DETECTED_SURFACE);
   * if (Input.touchCount != 0) {
   *   var t = Input.GetTouch(0);
   *   if (t.phase == TouchPhase.Began) {
   *     float x = t.position.x / Screen.width;
   *     float y = (Screen.height - t.position.y) / Screen.height;
   *     hits.AddRange(xr.HitTest(x, y, types));
   *   }
   * }
   */
  public List<XRHitTestResult> HitTest(float x, float y, List<XRHitTestResult.Type> includedTypes) {
    MessageBuilder queryMessage = new MessageBuilder();
    var query = queryMessage.initRoot(XrQueryRequest.factory);
    var ht = query.getHitTest();
    ht.setX(x);
    ht.setY(y);
    if (includedTypes.Count > 0) {
      query.getHitTest().initIncludedTypes(includedTypes.Count);
      for (int i = 0; i < includedTypes.Count; ++i) {
        var type = includedTypes[i];
        switch (type) {
          case XRHitTestResult.Type.FEATURE_POINT:
            ht.getIncludedTypes().set(i, XrHitTestResult.ResultType.FEATURE_POINT);
            break;
          case XRHitTestResult.Type.ESTIMATED_SURFACE:
            ht.getIncludedTypes().set(i, XrHitTestResult.ResultType.ESTIMATED_SURFACE);
            break;
          case XRHitTestResult.Type.DETECTED_SURFACE:
            ht.getIncludedTypes().set(i, XrHitTestResult.ResultType.DETECTED_SURFACE);
            break;
          default:
            // pass
            break;
        }
      }
    }
    var response = bridge.Query(queryMessage);
    List<XRHitTestResult> results = new List<XRHitTestResult>();
    foreach (var hit in response.getHitTest().getHits()) {
      var type = XRHitTestResult.Type.UNSPECIFIED;
      switch (hit.getType()) {
        case XrHitTestResult.ResultType.FEATURE_POINT:
          type = XRHitTestResult.Type.FEATURE_POINT;
          break;
        case XrHitTestResult.ResultType.ESTIMATED_SURFACE:
          type = XRHitTestResult.Type.ESTIMATED_SURFACE;
          break;
        case XrHitTestResult.ResultType.DETECTED_SURFACE:
          type = XRHitTestResult.Type.DETECTED_SURFACE;
          break;
        default:
          type = XRHitTestResult.Type.UNSPECIFIED;
          break;
      }
      Vector3 position = new Vector3(
        hit.getPlace().getPosition().getX(),
        hit.getPlace().getPosition().getY(),
        hit.getPlace().getPosition().getZ());
      Quaternion rotation = new Quaternion(
        hit.getPlace().getRotation().getX(),
        hit.getPlace().getRotation().getY(),
        hit.getPlace().getRotation().getZ(),
        hit.getPlace().getRotation().getW());
      float distance = hit.getDistance();
      results.Add(new XRHitTestResult(type, position, rotation, distance));
    }
    return results;
  }

  /**
   * Returns the estimated 3d location of some points in the world, as estimated by the AR engine.
   */
  public List<XRWorldPoint> GetWorldPoints() {
    var r = GetCurrentReality();
    List<XRWorldPoint> pts = new List<XRWorldPoint>();
    foreach (var pt in r.getFeatureSet().getPoints()) {
      var p = pt.getPosition();
      pts.Add(new XRWorldPoint(
        pt.getId(),
        new Vector3(p.getX(), p.getY(), p.getZ()),
        pt.getConfidence()));
    }
    return pts;
  }

  /**
   * Returns the image-targets that have been detected after calling SetDetectionImages.
   */
  public List<XRDetectedImageTarget> GetDetectedImageTargets() {
    var r = GetCurrentReality();
    List<XRDetectedImageTarget> pts = new List<XRDetectedImageTarget>();
    foreach (var target in r.getXRResponse().getDetection().getImages()) {
      pts.Add(new XRDetectedImageTarget(
        target.getId(),
        target.getName().ToString(),
        new Vector3(
          target.getPlace().getPosition().getX(),
          target.getPlace().getPosition().getY(),
          target.getPlace().getPosition().getZ()),
        new Quaternion(
          target.getPlace().getRotation().getX(),
          target.getPlace().getRotation().getY(),
          target.getPlace().getRotation().getZ(),
          target.getPlace().getRotation().getW()),
        target.getWidthInMeters(),
        target.getHeightInMeters()));
    }
    return pts;
  }

  /**
   * Static method that returns the AR capabilities available to the device, e.g. position tracking
   * and surface estimation.
   */
  public static XRCapabilities GetDeviceCapabilities() {
    XRCapabilities.PositionTracking position = XRCapabilities.PositionTracking.UNSPECIFIED;
    XRCapabilities.SurfaceEstimation surface = XRCapabilities.SurfaceEstimation.UNSPECIFIED;
    XRCapabilities.TargetImageDetection imageDetect =
      XRCapabilities.TargetImageDetection.UNSPECIFIED;

    XREnvironment.Reader env = XRNativeBridge.GetXREnvironment();

    switch (env.getCapabilities().getPositionTracking()) {
      case c8.XRCapabilities.PositionalTrackingKind.ROTATION_AND_POSITION:
        position = XRCapabilities.PositionTracking.ROTATION_AND_POSITION;
        break;
      case c8.XRCapabilities.PositionalTrackingKind.ROTATION_AND_POSITION_NO_SCALE:
        position = XRCapabilities.PositionTracking.ROTATION_AND_POSITION_NO_SCALE;
        break;
      default:
        position = XRCapabilities.PositionTracking.UNSPECIFIED;
        break;
    }

    switch (env.getCapabilities().getSurfaceEstimation()) {
      case c8.XRCapabilities.SurfaceEstimationKind.FIXED_SURFACES:
        surface = XRCapabilities.SurfaceEstimation.FIXED_SURFACES;
        break;
      case c8.XRCapabilities.SurfaceEstimationKind.HORIZONTAL_ONLY:
        surface = XRCapabilities.SurfaceEstimation.HORIZONTAL_ONLY;
        break;
      case c8.XRCapabilities.SurfaceEstimationKind.HORIZONTAL_AND_VERTICAL:
        surface = XRCapabilities.SurfaceEstimation.HORIZONTAL_AND_VERTICAL;
        break;
      default:
        surface = XRCapabilities.SurfaceEstimation.UNSPECIFIED;
        break;
    }

    switch (env.getCapabilities().getTargetImageDetection()) {
      case c8.XRCapabilities.TargetImageDetectionKind.UNSUPPORTED:
        imageDetect = XRCapabilities.TargetImageDetection.UNSUPPORTED;
        break;
      case c8.XRCapabilities.TargetImageDetectionKind.FIXED_SIZE_IMAGE_TARGET:
        imageDetect = XRCapabilities.TargetImageDetection.FIXED_SIZE_IMAGE_TARGET;
        break;
      default:
        imageDetect = XRCapabilities.TargetImageDetection.UNSPECIFIED;
        break;
    }

    return new XRCapabilities(position, surface, imageDetect);
  }

  /**
   *  Reconfigure XR based on the currently selected options (lighting, camera, surfaces, etc.).
   *  All configuration changes are best-effort based on the setting and device. This means that
   *  changes might take effect immediately (next frame), soon (in a few frames), on camera session
   *  restart (the next call to pause and resume), or never (if a setting is not supported on a given
   *  device).
   */
  public void ConfigureXR() {
    if (bridge == null) {
      return;
    }

    {
      var configMessageBuilder = new MessageBuilder();
      var config = configMessageBuilder.initRoot(XRConfiguration.factory);
      var configMask = config.getMask();
      configMask.setLighting(enableLighting);
      configMask.setCamera(enableCamera);
      configMask.setSurfaces(enableSurfaces);
      configMask.setVerticalSurfaces(enableVerticalSurfaces);

      config.getCameraConfiguration().setAutofocus(enableCameraAutofocus);
      config.setMobileAppKey(GetMobileAppKey());

      if (cam != null) {
        var graphicsIntrinsicsConfig = config.getGraphicsIntrinsics();
        graphicsIntrinsicsConfig.setTextureWidth((int)cam.pixelRect.width);
        graphicsIntrinsicsConfig.setTextureHeight((int)cam.pixelRect.height);
        graphicsIntrinsicsConfig.setNearClip(cam.nearClipPlane);
        graphicsIntrinsicsConfig.setFarClip(cam.farClipPlane);
        graphicsIntrinsicsConfig.setDigitalZoomHorizontal(1.0f);
        graphicsIntrinsicsConfig.setDigitalZoomVertical(1.0f);
      }

      bridge.CommitConfiguration(configMessageBuilder);
    }

    {
      var configMessageBuilder = new MessageBuilder();
      var config = configMessageBuilder.initRoot(XRConfiguration.factory);

      var coords = config.getCoordinateConfiguration();
      coords.getOrigin().getRotation().setW(facing.w);
      coords.getOrigin().getRotation().setX(facing.x);
      coords.getOrigin().getRotation().setY(facing.y);
      coords.getOrigin().getRotation().setZ(facing.z);
      coords.getOrigin().getPosition().setX(origin.x);
      coords.getOrigin().getPosition().setY(origin.y);
      coords.getOrigin().getPosition().setZ(origin.z);
      coords.setScale(scale);

      bridge.CommitConfiguration(configMessageBuilder);
    }
  }

  /**
   * Get the target-images that were last sent for detection to the AR engine via
   * SetDetectionImages, or an empty map if none were set.
   *
   * For example, to add an image to the image-targets being detected, call
   *
   * var images = xr.GetDetectionImages();
   * images.Add(
   *   "new-image-name",
   *   XRDetectionImage.FromDetectionTexture(
   *     new XRDetectionTexture(newImageTexture, newImageWidthInMeters)));
   * xr.SetDetectionImages(images);
   */
  public Dictionary<String, XRDetectionImage> GetDetectionImages() {
    return detectionImages_ != null
      ? new Dictionary<String, XRDetectionImage>(detectionImages_)
      : new Dictionary<String, XRDetectionImage>();
  }

  /**
   * Sets the target-images that will be detected.
   */
  public void SetDetectionImages(Dictionary<String, XRDetectionImage> images) {
    detectionImages_ = new Dictionary<String, XRDetectionImage>(images);

    if (detectionImages_ == null) {
      return;
    }

    int numImages = detectionImages_.Count;

    var configMessageBuilder = new MessageBuilder();
    var config = configMessageBuilder.initRoot(XRConfiguration.factory);
    var detectionImageSet = config.getImageDetection().initImageDetectionSet(numImages);
    int i = 0;
    foreach (var imageItem in detectionImages_) {
      string name = imageItem.Key;
      var detectionImage = imageItem.Value;

      var xrDetectionImage = detectionImageSet.get(i);
      xrDetectionImage.setName(name);
      xrDetectionImage.setRealWidthInMeter(detectionImage.targetWidthInMeters);

      var cid = xrDetectionImage.getImage();
      cid.setWidth(detectionImage.widthInPixels);
      cid.setHeight(detectionImage.heightInPixels);
      cid.setEncoding(ToImageDataEncoding(detectionImage.encoding));
      cid.setData(detectionImage.imageData);
      i++;
    }
    bridge.CommitConfiguration(configMessageBuilder);
  }

  /**
   * Disable the native AR engine (e.g. ARKit / ARCore) and force the use of 8th Wall's instant
   * surface tracker. This can be useful if you want to test compatibility with legacy devices,
   * or if you want to use 8th Wall's instant surface tracking.
   *
   * This should only be called during scene setup, e.g. in Awake or OnEnable.
   */
  public void DisableNativeArEngine(bool isDisabled) {
    disableNativeAr = isDisabled;
    SetEngineMode();
  }

  // Awake is called first at app startup.
  void Awake() {
    running = false;
    bridge = new XRNativeBridge(enableRemote);
    bridge.Create(GetRenderingSystem());
    if (EnableRemote()) {
      editorBridge = new XREditorBridge();
    }
    xrEnvironment = XRNativeBridge.GetXREnvironment();
    Application.targetFrameRate = 60;
  }

  void OnEnable() {
    SetEngineMode();
  }

  // Start is called after OnEnable, but it is only called once for a given script, while OnEnable
  // is called after every call to OnDisable.
  IEnumerator Start() {
    startedInUnity = true;
    lastRealityMicros = 0;
    updateNumber = 0;
    currentRealityUpdateNumber = -1;
    currentSurfacesUpdateNumber = -1;
    ConfigureXR();
    if (EnableRemote()) {
      editorBridge.Start();
    }
    yield return StartCoroutine("CallPluginAtEndOfFrames");
  }

  void Update() {
    if (!explicitlyPaused) {
      RunIfPaused();
    }

    updateNumber++;

    if (EnableRemote()) {
      bridge.SetEditorAppInfo(editorBridge.EditorAppInfo());

      bool firstConnect = false;
      if (!remoteConnected) {
        remoteConnected = bridge.IsRemoteConnected();
        firstConnect = remoteConnected;
      }
      if (remoteConnected) {
        var remoteData = bridge.GetXRRemote();
        if (firstConnect) {
          editorBridge.SetPlayerAspect(remoteData);
          xrEnvironment = XRNativeBridge.GetXREnvironment();
        }
        editorBridge.SendDeviceInfo(remoteData);
        editorBridge.Update();

        // Send camera aspect info to editor on every frame, in case the preview size changes.
        ConfigureXR();
      } else {
        editorBridge.CheckADB();
      }
    }

    var r = GetCurrentReality();
    if (lastRealityMicros >= r.getEventId().getEventTimeMicros()) {
      return;
    }
    lastRealityMicros = r.getEventId().getEventTimeMicros();
  }

  void OnGUI() {
    if (EnableRemote() && !remoteConnected) {
      editorBridge.OnGUI();
    }
  }

  void OnApplicationPause(bool isPaused) {
    explicitlyPaused = isPaused;
    if (!isPaused && startedInUnity) {
      RunIfPaused();
      return;
    }

    if (!running) {
      return;
    }
    running = false;
    bridge.Pause();
  }

  void OnDisable() {
    if (running) {
      OnApplicationPause(true);
    }
  }

  void OnDestroy() {
    bridge.Destroy();
    if (EnableRemote()) {
      editorBridge.OnApplicationQuit();
    }
  }

  void OnApplicationQuit() {
    bridge.Destroy();
  }

  private static XRSurface.Type GetSurfaceType(c8.Surface.SurfaceType surfaceType) {
    switch (surfaceType) {
      case c8.Surface.SurfaceType.HORIZONTAL_PLANE:
        return XRSurface.Type.HORIZONTAL_PLANE;
      case c8.Surface.SurfaceType.VERTICAL_PLANE:
        return XRSurface.Type.VERTICAL_PLANE;
      default:
        return XRSurface.Type.UNSPECIFIED;
    }
  }

  private void RunIfPaused() {
    if (running) {
      return;
    }
    running = true;
    bridge.Resume();
  }

  private RealityResponse.Reader GetCurrentReality() {
    if (currentXRResponse == null || currentRealityUpdateNumber < updateNumber) {
      currentRealityUpdateNumber = updateNumber;
      currentXRResponse = bridge.GetCurrentRealityXR();
    }
    return currentXRResponse;
  }

  /** Call GetSurfacesFromXRResponse() and cache the result. Calling this method
      more than once per Update() will hit the cache
   */
  private Dictionary<long, XRSurface> GetSurfaceMap() {
    if (currentSurfaces == null || currentSurfacesUpdateNumber < updateNumber) {
      currentSurfacesUpdateNumber = updateNumber;
      currentSurfaces = GetSurfacesFromXRResponse();
    }
    return currentSurfaces;
  }

  /** This method doesn't cache its result.
   */
  private Dictionary<long, XRSurface> GetSurfacesFromXRResponse() {
    var r = GetCurrentReality();

    var surfaceSet = r.getXRResponse().getSurfaces().getSet();
    var surfaces = surfaceSet.getSurfaces();
    var faces = surfaceSet.getFaces();
    var vertexList = surfaceSet.getVertices();
    var textureCoordsList = surfaceSet.getTextureCoords();
    // Boundary vertices are available but not used right now

    var surfacesToRemove = new HashSet<long>(xrSurfaceMap_.Keys);
    foreach (var surface in surfaces) {
      long id = surface.getId().getEventTimeMicros();
      surfacesToRemove.Remove(id);

      var quat = surface.getNormal().getRotation();

      // Extract basic info about this mesh.
      int beginFaceIndex = surface.getFacesBeginIndex();
      int endFaceIndex = surface.getFacesEndIndex();
      int beginVerticesIndex = surface.getVerticesBeginIndex();
      int endVerticesIndex = surface.getVerticesEndIndex();

      // Support texture coords when the device has it (e.g. ARKit >1.5)
      int beginTextureCoordsIndex = 0;
      bool hasTextureCoords = false;
      if (surface.getTextureCoordsEndIndex() > 0) {
        hasTextureCoords = true;
        beginTextureCoordsIndex = surface.getTextureCoordsBeginIndex();
      }

      // Build the vertex and normal arrays.
      int nVertices = endVerticesIndex - beginVerticesIndex;
      Vector3[] vertices = new Vector3[nVertices];
      Vector3[] normals = new Vector3[nVertices];
      Vector2[] uvs = new Vector2[nVertices];

      for (int j = 0; j < nVertices; ++j) {
        int vertexIndex = beginVerticesIndex + j;
        vertices[j] = new Vector3(
            vertexList.get(vertexIndex).getX(),
            vertexList.get(vertexIndex).getY(),
            vertexList.get(vertexIndex).getZ());
        normals[j] = Vector3.up;

        float u = vertices[j][0];
        float v = vertices[j][2];
        if (hasTextureCoords) {
          int textureIndex = beginTextureCoordsIndex + j;
          u = textureCoordsList.get(textureIndex).getU();
          v = textureCoordsList.get(textureIndex).getV();
        }
        uvs[j] = new Vector2(u, v);
      }

      // We can just directly copy over the triangles (they are stored in consecutive sets of three
      // vertex indices) as long as we offset the vertex indices.
      int nFaces = endFaceIndex - beginFaceIndex;
      int[] triangles = new int[nFaces * 3];
      for (int j = 0; j < nFaces; ++j) {
        int v0 = faces.get(j + beginFaceIndex).getV0() - beginVerticesIndex;
        int v1 = faces.get(j + beginFaceIndex).getV1() - beginVerticesIndex;
        int v2 = faces.get(j + beginFaceIndex).getV2() - beginVerticesIndex;
        triangles[3 * j] = v0;
        triangles[3 * j + 1] = v1;
        triangles[3 * j + 2] = v2;
      }

      var surfaceType = GetSurfaceType(surface.getSurfaceType());
      var surfaceRotation = new Quaternion(quat.getX(), quat.getY(), quat.getZ(), quat.getW());

      Mesh mesh = null;
      if (xrSurfaceMap_.ContainsKey(id)) {
        XRSurface existingSurface = xrSurfaceMap_[id];
        mesh = existingSurface.mesh;
        mesh.Clear();
      } else {
        mesh = new Mesh();
      }
      mesh.vertices = vertices;
      mesh.normals = normals;
      mesh.uv = uvs;
      mesh.triangles = triangles;
      xrSurfaceMap_[id] = new XRSurface(id, surfaceType, surfaceRotation, mesh);
    }

    // Remove surfaces we no longer receive from the map
    foreach (long surfaceId in surfacesToRemove) {
      xrSurfaceMap_.Remove(surfaceId);
    }

    return xrSurfaceMap_;
  }

  private bool ShouldIssuePluginEvent() {
    // Check that we have an RGBA texture and don't manage an RGBA texture.
    if (realityRGBATexture != null
      && xrEnvironment.getRealityImage().getRgbaTexture().getPtr() == 0) {
      return true;
    }
    // Check that we have Y, UV textures and don't manage Y, UV textures.
    if (realityYTexture != null
      && realityUVTexture != null
      && xrEnvironment.getRealityImage().getYTexture().getPtr() == 0
      && xrEnvironment.getRealityImage().getUvTexture().getPtr() == 0) {
      return true;
    }
    return false;
  }

  private IEnumerator CallPluginAtEndOfFrames() {
    IntPtr renderEventFunc = bridge.GetRenderEventFunc();
    while (renderEventFunc != IntPtr.Zero) {
      // Wait until all frame rendering is done.
      yield return new WaitForEndOfFrame();


      if (ShouldIssuePluginEvent()) {
        // Issue a plugin event such that it is called from the Unity render thread.
        GL.IssuePluginEvent(renderEventFunc, 1 /* eventID */);
      }
    }
  }

  private int GetRenderingSystem() {
    switch (SystemInfo.graphicsDeviceType) {
      case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
      case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
      case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
        return XREnvironmentConstants.RENDERING_SYSTEM_OPENGL;
      case UnityEngine.Rendering.GraphicsDeviceType.Metal:
        return XREnvironmentConstants.RENDERING_SYSTEM_METAL;
      case UnityEngine.Rendering.GraphicsDeviceType.Direct3D11:
        return XREnvironmentConstants.RENDERING_SYSTEM_DIRECT3D11;
      default:
        return XREnvironmentConstants.RENDERING_SYSTEM_UNSPECIFIED;
    }
  }

  private string GetMobileAppKey() {
    // Prefer an app key that was explicitly set in code on this controller.
    if (!String.IsNullOrEmpty(overrideAppKey)) {
      return overrideAppKey;
    }

    // Otherwise, get the app key configured in the unity editor.
  #if UNITY_EDITOR
    return XRUnityEditorKeyLoader.MobileAppKey();
  #else
    return XRInternal.XRAutoGenerated.XRMobileAppKey.KEY;
  #endif
  }

  private static CompressedImageData.Encoding ToImageDataEncoding(
    XRDetectionImage.Encoding encoding) {
    switch (encoding) {
      case XRDetectionImage.Encoding.RGB24_INVERTED_Y:
        return CompressedImageData.Encoding.RGB24_INVERTED_Y;
      case XRDetectionImage.Encoding.RGB24:
        return CompressedImageData.Encoding.RGB24;
      default:
        return CompressedImageData.Encoding.UNSPECIFIED;
    }
  }

  private bool EnableRemote() {
    return bridge.IsStreamingSupported();
  }

  private void SetEngineMode() {
    XREngineConfiguration.SpecialExecutionMode engineMode =
      XREngineConfiguration.SpecialExecutionMode.NORMAL;
    if (remoteOnly) {
      engineMode = XREngineConfiguration.SpecialExecutionMode.REMOTE_ONLY;
    } else if (disableNativeAr) {
      engineMode = XREngineConfiguration.SpecialExecutionMode.DISABLE_NATIVE_AR_ENGINE;
    }
    var configMessageBuilder = new MessageBuilder();
    var config = configMessageBuilder.initRoot(XRConfiguration.factory);
    config.getEngineConfiguration()
      .setMode(engineMode);
    bridge.CommitConfiguration(configMessageBuilder);
    xrEnvironment = XRNativeBridge.GetXREnvironment();

    if (cam != null) {
      UpdateCameraProjectionMatrix(cam, origin, facing, scale);
    }
  }

}
