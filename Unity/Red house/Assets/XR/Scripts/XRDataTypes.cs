using System;
using UnityEngine;

/**
 * AR capabilities available to a given device based on its native AR engine (e.g. is it using ARKit
 * vs ARCore vs. 8th Wall's Computer Vision technology).
 */
public struct XRCapabilities {
  /**
   * The type of AR position tracking used by the device.
   */
  public enum PositionTracking {
    /**
     * Unable to determine the tracking engine used by the device.
     */
    UNSPECIFIED,

    /**
     * The device has 6DoF positional and rotational camera tracking, where position is based
     * physically accurate distances (such as the position tracking offered by ARKit and ARCore).
     */
    ROTATION_AND_POSITION,

    /**
     * The device has full 6DoF positional and rotational camera tracking, where position
     * is scaled based on the distance to a horizontal surface in the visual scene.
     */
    ROTATION_AND_POSITION_NO_SCALE
  };

  /**
   * The type of AR surface estimation used by the device.
   */
  public enum SurfaceEstimation {
    /**
     *  Unable to determine the surface estimataion engine used by the device.
     */
    UNSPECIFIED,

    /**
     * The device is using 8th Wall instant surface placement (Non-ARKit/ARCore).
     */
    FIXED_SURFACES,

    /**
     * The device is using an early version of ARKit/ARCore that only supports detection of
     * horizontal surfaces.
     */
    HORIZONTAL_ONLY,

    /**
     * The device is using a newwer version of ARKit/ARCore that supports detection of both
     * horizontal and vertical surfaces.
     */
    HORIZONTAL_AND_VERTICAL
  };

  /**
   * The type of AR image-target detection used by the device.
   */
  public enum TargetImageDetection {
    /**
     *  Unable to determine the image-target detection engine used by the device.
     */
    UNSPECIFIED,

    /**
     * The device is running an AR engine that does not support detection of image-targets.
     */
    UNSUPPORTED,

    /**
     * The device is running an AR engine that supports detection of image-targets of a
     * developer-specified, predfined phyisical size in meters.
     */
    FIXED_SIZE_IMAGE_TARGET,
  };

  /**
   * The level of position tracking available to the device.
   */
  public readonly PositionTracking positionTracking;

  /**
   * The level of surface estimation available to the device.
   */
  public readonly SurfaceEstimation surfaceEstimation;

  /**
   * The level of image-target detection available to the device.
   */
  public readonly TargetImageDetection targetImageDetection;

  /**
   * Initializes a new XRCapabilities struct with a specified positionTracking, surfaceEstimation
   * and targetImageDetection.
   */
  public XRCapabilities(
    PositionTracking positionTracking,
    SurfaceEstimation surfaceEstimation,
    TargetImageDetection targetImageDetection) {
    this.positionTracking = positionTracking;
    this.surfaceEstimation = surfaceEstimation;
    this.targetImageDetection = targetImageDetection;
  }

  /**
   * This is a convience method. Equivalent to calling:
   *
   *   XRCapabilities.positionTracking == XRCapabilities.PositionTracking.ROTATION_AND_POSITION;
   *
   * If true, the device has 6DoF positional and rotational camera tracking, where position is
   * based on physically accurate distances (such as the position tracking offered by ARKit and
   * ARCore).
   */
  public bool IsPositionTrackingRotationAndPosition() {
    return positionTracking == PositionTracking.ROTATION_AND_POSITION;
  }

  /**
   * This is a convience method. Equivalent to calling:
   *
   *   return positionTracking == PositionTracking.ROTATION_AND_POSITION_NO_SCALE;
   *
   * If true, the device has full 6DoF positional and rotational camera tracking, where position
   * is scaled based on the distance to a horizontal surface in the visual scene.
   */
  public bool IsPositionTrackingRotationAndPositionNoScale() {
    return positionTracking == PositionTracking.ROTATION_AND_POSITION_NO_SCALE;
  }

  /**
   * This is a convience method. Equivalent to calling:
   *
   *   return surfaceEstimation == SurfaceEstimation.FIXED_SURFACES;
   *
   * If true, the device is using 8th Wall instant surface placement (Non-ARKit/ARCore).
   */
  public bool IsSurfaceEstimationFixedSurfaces() {
    return surfaceEstimation == SurfaceEstimation.FIXED_SURFACES;
  }

  /**
   * This is a convience method. Equivalent to calling:
   *
   *   return surfaceEstimation == SurfaceEstimation.HORIZONTAL_ONLY;
   *
   * If true, the device is using an early version of ARKit/ARCore that only supports detection of
   * horizontal surfaces.
   */
  public bool IsSurfaceEstimationHorizontalOnly() {
    return surfaceEstimation == SurfaceEstimation.HORIZONTAL_ONLY;
  }

  /**
   * This is a convience method. Equivalent to calling:
   *
   *   return surfaceEstimation == SurfaceEstimation.HORIZONTAL_AND_VERTICAL;
   *
   * If true, the device is using a newwer version of ARKit/ARCore that supports detection of
   * both horizontal and vertical surfaces.
   */
  public bool IsSurfaceEstimationHorizontalAndVertical() {
    return surfaceEstimation == SurfaceEstimation.HORIZONTAL_AND_VERTICAL;
  }

  /**
   * This is a convience method. Equivalent to calling:
   *
   *   return targetImageDetection == TargetImageDetection.UNSUPPORTED;
   *
   * If true, the device is running an AR engine that does not support detection of image-targets.
   */
  public bool IsTargetImageDetectionUnsupported() {
    return targetImageDetection == TargetImageDetection.UNSUPPORTED;
  }

  /**
   * This is a convience method. Equivalent to calling:
   *
   *   return targetImageDetection == TargetImageDetection.FIXED_SIZE_IMAGE_TARGET;
   *
   * If true, the device is running an AR engine that supports detection of image-targets of a
   * developer-specified, predfined phyisical size in meters.
   */
  public bool IsTargetImageDetectionFixedSizeImageTarget() {
    return targetImageDetection == TargetImageDetection.FIXED_SIZE_IMAGE_TARGET;
  }
}

/**
 * Defines the amount that a camera feed texture should be rotated to appear upright in a given
 * app's UI based on the app's orientation (e.g. portrait or landscape right) on the current device.
 */
public enum XRTextureRotation {
  /**
   * Unable to determine the camera feed texture rotation.
   */
  UNSPECIFIED,

  /**
   * The camera feed texture does not need to be rotated.
   */
  R0,

  /**
   * The camera feed texture should be rotated by 90 degrees.
   */
  R90,

  /**
   * The camera feed texture should be rotated by 180 degrees.
   */
  R180,

  /**
   * The camera feed texture should be rotated by 270 degrees.
   */
  R270
};

/**
 * Indicates the current quality of a device's tracking in the user's current environment.
 */
public struct XRTrackingState {
  /**
   * Indicates the current quality level of tracking.
   */
  public enum Status {
    /**
     * Unable to determine tracking quality.
     */
    UNSPECIFIED,

    /**
     * Tracking is not currently enabled.
     */
    NOT_AVAILABLE,

    /**
     * Tracking is enabled but its quality is currently low.
     */
    LIMITED,

    /**
     * Tracking is enabled and operating as expected.
     */
    NORMAL
  };


  /**
   * Indicates why tracking is currently limited. Only specified when tracking status is LIMITED.
   */
  public enum Reason {
    /**
     * Tracking status is not currently LIMITED, or unable to determine why tracking is LIMITED.
     */
    UNSPECIFIED,

    /**
     * Tracking is limited because the tracking engine is still starting up.
     */
    INITIALIZING,

    /**
     * Tracking is limited because the tracking engine is unable to determine the device's current
     * location.
     */
    RELOCALIZING,

    /**
     * Tracking is limited because the device is moving too much.
     */
    TOO_MUCH_MOTION,

    /**
     * Tracking is limited because the current camera feed does not contain enough visual
     * information to determine how the device is moving.
     */
    NOT_ENOUGH_TEXTURE
  };

  /**
   * The current quality level of tracking.
   */
  public readonly Status status;

  /**
   * Indicates why tracking is currently limited, if it is currently limited.
   */
  public readonly Reason reason;

  /**
   * Initializes a new XRTrackingState struct with a specified status and reason.
   */
  public XRTrackingState(Status status, Reason reason) {
    this.status = status;
    this.reason = reason;
  }
}

/**
 * A surface detected by an AR surface detection engine.
 */
public struct XRSurface {
  /**
   * Indicates the type of surfaces that was detected.
   */
  public enum Type {
    /**
     * Unable to determine the type of surface that was detected.
     */
    UNSPECIFIED,

    /**
     * A flat surface parallel to the ground, e.g. a table or the ground.
     */
    HORIZONTAL_PLANE,

    /**
     * A flat surface perpendicular to the ground, e.g. a wall.
     */
    VERTICAL_PLANE
  }

  /**
   * A unique identifier for this surface that persists across updates.
   */
  public readonly Int64 id;

  /**
   * The type of the surface, e.g. horizontal or vertical.
   */
  public readonly Type type;

  /**
   * The orientation of this surface. Applying this rotation to GameObjects will rotate them to lie
   * flat on the surface.
   */
  public readonly Quaternion rotation;

  /**
   * A mesh that covers the surface.
   */
  public readonly Mesh mesh;

  /**
   * Initializes a new XRSurface struct with a specified id, type, rotation and mesh.
   */
  public XRSurface(Int64 id, Type type, Quaternion rotation, Mesh mesh) {
    this.id = id;
    this.type = type;
    this.rotation = rotation;
    this.mesh = mesh;
  }

  /**
   * A special surface that is returned by surface query APIs when no surface matches the query.
   * Surfaces returend by these APIs can be checked against NO_SURFACE using the == and !=
   * operators.
   */
  public static XRSurface NO_SURFACE = new XRSurface(
    0,
    Type.UNSPECIFIED,
    new Quaternion(0.0f, 0.0f, 0.0f, 1.0f),
    null);

  public override bool Equals(object o) {
    if (!(o is XRSurface)) {
      return false;
    }
    XRSurface s = (XRSurface)o;
    return id == s.id && type == s.type && rotation == s.rotation && mesh == s.mesh;

  }

  public static bool operator ==(XRSurface a, XRSurface b) {
    return a.Equals(b);
  }

  public static bool operator !=(XRSurface a, XRSurface b) {
     return !a.Equals(b);
  }

  public override int GetHashCode(){
    return id.GetHashCode()
      ^ type.GetHashCode()
      ^ rotation.GetHashCode()
      ^ (mesh == null ? 0 : mesh.GetHashCode());
  }
}

/**
 * The result of a hit test query to estimate the 3D position of a point shown on the device's
 * camera feed.
 */
public struct XRHitTestResult {
  /**
   * The type of data that was used to generate a hit test result.
   */
  public enum Type {
    /**
     * Unable to determine how a hit test result was generated.
     */
    UNSPECIFIED,

    /**
     * The location of a hit test result was estimated from nearby feature points.
     */
    FEATURE_POINT,

    /**
     * The location of a hit test result was inferred from the location of a known surfaces, but
     * the AR engine has not yet confirmed that that location is actually a part of a surface.
     */
    ESTIMATED_SURFACE,

    /**
     * The location of a hit test result is within the bounds of a confirmed detected surface.
     */
    DETECTED_SURFACE
  };

  /**
   * The type of data that was used to generate the hit test result.
   */
  public readonly Type type;

  /**
   * The estimated 3d position in the unity scene of the queried point in the unity scene based on
   * the camera feed.
   */
  public readonly Vector3 position;

  /**
   * The estimated 3d rotation of the queried point on the camera feed.
   */
  public readonly Quaternion rotation;

  /**
   * The estimated distance from the device of the queried point on the camera feed.
   */
  public readonly float distance;

  /**
   * Initializes a new XRHitTestResult struct with a specified type, position, rotation and
   * distance.
   */
  public XRHitTestResult(Type type_, Vector3 position_, Quaternion rotation_, float distance_) {
    type = type_;
    position = position_;
    rotation = rotation_;
    distance = distance_;
  }
}

/**
 * A point in the world detected by an AR engine.
 */
public struct XRWorldPoint {
  /**
   * A unique identifier of this point that persists across frames.
   */
  public readonly Int64 id;

  /**
   * The 3d position of the point in the unity scene.
   */
  public readonly Vector3 position;

  /**
   * Indicates how confident the AR engine is in the location of this point.
   */
  public readonly float confidence;

  /**
   * Initializes a new XRWorldPoint struct with a specified id, position, and confidence.
   */
  public XRWorldPoint(Int64 id, Vector3 position, float confidence) {
    this.id = id;
    this.position = position;
    this.confidence = confidence;
  }
}

/**
 * A unity Texture2D that can be used as a source for image-target detection.
 */
[Serializable] public struct XRDetectionTexture {
  /**
   * The unity texture containing the image data for detection. Textures must have the
   * "Read/Write Enabled" setting checked, and must have the "Non Power Of 2" setting set to "None".
   */
  public Texture2D tex;

  /**
   * The expected physical width of the image-target, in meters.
   */
  public float widthInMeters;

  /**
   * Initializes a new XRDetectionTexture struct with a specified tex and widthInMeters.
   */
  public XRDetectionTexture(Texture2D tex, float widthInMeters) {
    this.tex = tex;
    this.widthInMeters = widthInMeters;
  }
}

/**
 * Source image data for a image-target to detect. This can either be constructed manually, or
 * from a Unity Texture2d.
 */
public struct XRDetectionImage {
  /**
   * The width of the source binary image-target, in pixels.
   */
  public readonly int widthInPixels;

  /**
   * The height of the source binary image-target, in pixels.
   */
  public readonly int heightInPixels;

  /**
   * The expected physical width of the image-target, in meters.
   */
  public readonly float targetWidthInMeters;

  /**
   * The encoding of the binary image data.
   */
  public readonly Encoding encoding;

  /**
   * The binary data containing the image-target to detect.
   */
  public readonly byte[] imageData;

  /**
   * Indicates the binary encoding format of a image-target to detect.
   */
  public enum Encoding {
    /**
     * Unable to determine the image-target binary encoding.
     */
    UNSPECIFIED,

    /**
     * Pixels are stored in 3-byte RGB values, values ranging from 0-255, ordered by row. The
     * length of imageData should be 3 * widthInPixels * heightInPixels.
     */
    RGB24,

    /**
     * Pixels are stored in 3-byte RGB values, values ranging from 0-255, ordered by row in reverse
     * order (from bottom to top). The length of imageData should be
     * 3 * widthInPixels * heightInPixels.
     */
    RGB24_INVERTED_Y
  }

  /**
   * Initializes a new XRDetectionImage struct with a specified widthInPixels, heightInPixels,
   * targetWidthInMeters, encoding, and imageData.
   */
  public XRDetectionImage(
    int widthInPixels,
    int heightInPixels,
    float targetWidthInMeters,
    Encoding encoding,
    byte[] imageData) {
    this.widthInPixels = widthInPixels;
    this.heightInPixels = heightInPixels;
    this.targetWidthInMeters = targetWidthInMeters;
    this.encoding = encoding;
    this.imageData = imageData;
  }

  /**
   * Initializes a new XRDetectionImage from a unity Texture2D and a specified targetWidthInMeters.
   * The texture must have the "Read/Write Enabled" setting checked, and must have the
   * "Non Power Of 2" setting set to "None".
   */
  static public XRDetectionImage FromDetectionTexture(XRDetectionTexture texture){
    byte[] byteData;
    if (texture.tex.format == TextureFormat.RGB24) {
      byteData = texture.tex.GetRawTextureData();
    } else {
      Texture2D newTexture2DInRGB24 = new Texture2D(texture.tex.width, texture.tex.height,
        TextureFormat.RGB24, false);
      newTexture2DInRGB24.SetPixels(texture.tex.GetPixels());
      newTexture2DInRGB24.Apply();
      byteData = newTexture2DInRGB24.GetRawTextureData();
    }

    return new XRDetectionImage(
      texture.tex.width,
      texture.tex.height,
      texture.widthInMeters,
      Encoding.RGB24_INVERTED_Y,
      byteData);
  }
}

/**
 * An image-target that was detected by an AR Engine.
 */
public struct XRDetectedImageTarget {
  /**
   * A unique identifier for this detected image-target that is consistent across updates.
   */
  public readonly Int64 id;

  /**
   * The name of the image-target that was provided by the developer on a call to
   * XRController.SetDetectionImages.
   */
  public readonly String name;

  /**
   * The position of the center of the image in unity coordinates.
   */
  public readonly Vector3 position;

  /**
   * The orientation of the detected image. The detected image lies in the x/z plane of this
   * rotation.
   */
  public readonly Quaternion rotation;

  /**
   * Width of the detected image-target, in unity units.
   */
  public readonly float width;

  /**
   * Height of the detected image-target, in unity units.
   */
  public readonly float height;

  /**
   * Initializes a XRDetectedImageTarget from a specified id, name, position, rotation, width, and
   * height.
   */
  public XRDetectedImageTarget(
    Int64 id,
    String name,
    Vector3 position,
    Quaternion rotation,
    float width,
    float height) {
    this.id = id;
    this.name = name;
    this.position = position;
    this.rotation = rotation;
    this.width = width;
    this.height = height;
  }
}
