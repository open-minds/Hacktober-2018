using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections.Generic;

public class XRSurfaceController : MonoBehaviour {

  private const float GROUND_DISTANCE_GUESS = 1.2f; // ~4ft
  private const float MIN_GROUND_DISTANCE_FROM_PHONE = 0.5f;
  //New potential ground surface must be at least 8 inches below current ground surface
  private const float MIN_GROUND_CHANGE_DISTANCE = 0.2f; // ~8in

  private const float COMPATIBILITY_SURFACE_HALF_SIZE = 100.0f;

  // If true, XRSurfaceController will update the rendered mesh and the collider mesh of the surface
  // so that it matches the detected surface. This allows for interactions like shadows that clip
  // to surface boundaries, and objects that can fall off surfaces.
  public bool deformToSurface = false;

  // If true, the game object will appear as placed in the scene prior to surface detection.
  // If false, the renderer and collider for this object are disabled, and all child objects are
  // inactivated until a surface is detected.
  public bool displayImmediately = false;

  // If true, only attach to ground surfaces. If displayImmediately is on, groundOnly only
  // adjusts the height of the game object to try to match it to the ground. If displayImmediately
  // is off, groundOnly searches for surfaces that are sufficiently lower than the camera and are
  // lower than any other detected surface.  If false, use any detected surface.
  public bool groundOnly = false;

  // If true, attach to the first detected surface and don't move it. If false, move the game object
  // to the currently active surface
  public bool lockToFirstSurface = true;

  // Invoked as soon as the surface is visible and positioned. This is invoked immediately when
  // displayImmediately is true. Otherwise it fires at the same time as onSurfaceAttach.
  public UnityEvent onSurfaceReady;

  // Invoked on first attachment to a detected surface, i.e. the first time a suitable surface is
  // detected. This is invoked immediately on devices where native AR engines are disabled or
  // unavailable.
  public UnityEvent onSurfaceAttach;

  // Invoked when switching from one surface to another after the first attachment to a surface.
  // This will never be invoked when lockToFirstSurface is true, when displayImmediately and
  // groundOnly are true, or on devices where native AR engines are disabled or unavailable.
  public UnityEvent onSurfaceSwitch;

  private XRController xr;
  private bool surfaceFound = false;
  private float groundHeightGuess;

  private MeshFilter meshFilter = null;
  private MeshCollider meshCollider = null;
  private Mesh deformMesh = null;

  private long surfaceId = Int64.MinValue;
  private Dictionary<long, Vector3> centerMap;

  private bool initialized = false;
  private Quaternion originalRotation_;

  private bool hidden = false;
  private List<GameObject> hiddenChildren = new List<GameObject>();
  private List<Collider> hiddenColliders = new List<Collider>();
  private List<Renderer> hiddenRenderers = new List<Renderer>();

  // If true, this gameObject will be rotated with the normal of the detected surface.
  private bool rotatePlaneWithDetectedNormal = true;

  void OnEnable() {
    if (onSurfaceReady == null) {
      onSurfaceReady = new UnityEvent();
    }
    if (onSurfaceAttach == null) {
      onSurfaceAttach = new UnityEvent();
    }
    if (onSurfaceSwitch == null) {
      onSurfaceSwitch = new UnityEvent();
    }
    originalRotation_ = transform.localRotation;
    centerMap = new Dictionary<long, Vector3>();
    xr = GameObject.FindWithTag("XRController").GetComponent<XRController>();
  }

  void OnDisable() {
    initialized = false;
    if (deformMesh != null) {
      Destroy(deformMesh);
      deformMesh = null;
    }
  }

  private void Initialize() {
    initialized = true;
    surfaceFound = false;

    meshFilter = gameObject.GetComponent<MeshFilter>();
    meshCollider = gameObject.GetComponent<MeshCollider>();

    if (deformToSurface) {
      if (meshFilter == null) {
        meshFilter = gameObject.AddComponent<MeshFilter>();
      }
      deformMesh = meshFilter.mesh;

      if (meshCollider != null) {
        meshCollider.sharedMesh = deformMesh;
      }
    }

    if (xr.GetCapabilities().IsSurfaceEstimationFixedSurfaces()) {
      InitializeForFixedSurfaces();
      return;
    }

    if (groundOnly) {
      // Start by moving object a reasonable guess for the ground height.
      groundHeightGuess = xr.GetCapabilities().IsPositionTrackingRotationAndPositionNoScale()
        ? 0.0f
        : Camera.main.transform.position.y - GROUND_DISTANCE_GUESS;

      SetHeight(groundHeightGuess);
    }

    // If display immediately is off, hide the object until a surface is found.
    if (!displayImmediately) {
      SetHidden(true);
    } else if(onSurfaceReady != null) {
      onSurfaceReady.Invoke();
    }
  }

  private void InitializeForFixedSurfaces() {
    surfaceFound = true;
    SetHidden(false);
    SetHeight(0);

    if (deformToSurface) {
      // Set mesh to a large quad
      Vector3[] vertices = new Vector3[4];
      vertices[0] = new Vector3(-COMPATIBILITY_SURFACE_HALF_SIZE, 0, -COMPATIBILITY_SURFACE_HALF_SIZE);
      vertices[1] = new Vector3(COMPATIBILITY_SURFACE_HALF_SIZE, 0, -COMPATIBILITY_SURFACE_HALF_SIZE);
      vertices[2] = new Vector3(COMPATIBILITY_SURFACE_HALF_SIZE, 0, COMPATIBILITY_SURFACE_HALF_SIZE);
      vertices[3] = new Vector3(-COMPATIBILITY_SURFACE_HALF_SIZE, 0, COMPATIBILITY_SURFACE_HALF_SIZE);

      int[] triangles = new int[6];
      triangles[0] = 0;
      triangles[1] = 2;
      triangles[2] = 1;
      triangles[3] = 0;
      triangles[4] = 3;
      triangles[5] = 2;

      Vector3[] normals = new Vector3[4];
      normals[0] = Vector3.up;
      normals[1] = Vector3.up;
      normals[2] = Vector3.up;
      normals[3] = Vector3.up;

      Vector2[] uvs = new Vector2[4];
      uvs[0] = new Vector2(vertices[0].x, vertices[0].z);
      uvs[1] = new Vector2(vertices[1].x, vertices[1].z);
      uvs[2] = new Vector2(vertices[2].x, vertices[2].z);
      uvs[3] = new Vector2(vertices[3].x, vertices[3].z);

      deformMesh.Clear();
      deformMesh.vertices = vertices;
      deformMesh.triangles = triangles;
      deformMesh.uv = uvs;
      deformMesh.normals = normals;

      if (meshCollider != null) {
        meshCollider.sharedMesh = deformMesh;
      }
    }

    if (onSurfaceReady != null) {
      onSurfaceReady.Invoke();
    }
  }

  void Update() {
    if (xr.DisabledInEditor()) {
      return;
    }

    if (!initialized) {
      Initialize();
    }

    if (xr.GetCapabilities().IsSurfaceEstimationFixedSurfaces()) {
      return;
    }

    if (groundOnly && displayImmediately) {
      UpdateForDisplayImmediatelyGroundOnly();
      return;
    }

    bool surfaceUpdated = UpdateSurface();

    if (!surfaceUpdated && surfaceFound) {
      transform.position = centerMap[surfaceId];
      if (deformToSurface) {
        DeformMesh(xr.GetSurface(surfaceId).mesh);
      }
    }
  }

  private void UpdateForDisplayImmediatelyGroundOnly() {
    long activeSurfaceId = xr.GetActiveSurfaceId();
    XRSurface activeSurface = xr.GetSurface(activeSurfaceId);

    bool activeSurfaceIsValidGround = activeSurface != XRSurface.NO_SURFACE &&
                                      activeSurface.type == XRSurface.Type.HORIZONTAL_PLANE;

    if (!activeSurfaceIsValidGround) {
      return;
    }

    Vector3 centerVertex = GetVertexCenter(activeSurface.mesh);
    if (surfaceFound) {
      if (centerVertex.y < groundHeightGuess) {
        groundHeightGuess = centerVertex.y;
        SetHeight(groundHeightGuess);
      }
    } else {
      if (centerVertex.y < Camera.main.transform.position.y - MIN_GROUND_DISTANCE_FROM_PHONE) {
        groundHeightGuess = centerVertex.y;
        SetHeight(groundHeightGuess);
        onSurfaceAttach.Invoke();
        surfaceFound = true;
      }
    }
  }

  // Returns true if surface was updated
  private bool UpdateSurface() {
    long activeSurfaceId = xr.GetActiveSurfaceId();
    XRSurface activeSurface = xr.GetSurface(activeSurfaceId);

    if (activeSurface.mesh == null) {
      return false;
    }

    // Update center map height if already in map
    if (centerMap.ContainsKey(activeSurfaceId)) {
      Vector3 centerVertex = GetVertexCenter(activeSurface.mesh);
      Vector3 previousCenter = centerMap[activeSurfaceId];
      Vector3 normal = Vector3.Cross(
        activeSurface.mesh.vertices[0] - activeSurface.mesh.vertices[1],
        activeSurface.mesh.vertices[0] - activeSurface.mesh.vertices[2]
      );
      normal.Normalize();
      Vector3 newCenter = previousCenter + normal * Vector3.Dot(centerVertex - previousCenter, normal);
      centerMap[activeSurfaceId] = newCenter;
    }

    if(surfaceId == activeSurfaceId) {
      return false;
    }

    if (groundOnly && !IsValidGroundSurface(activeSurface)) {
      return false;
    }

    if (surfaceFound && lockToFirstSurface) {
      return false;
    }

    if (!centerMap.ContainsKey(activeSurfaceId)) {
      centerMap.Add(activeSurfaceId, GetVertexCenter(activeSurface.mesh));
    }

    bool triggerReady = !displayImmediately && !surfaceFound;
    bool triggerAttach = !surfaceFound;

    SetHidden(false);
    transform.position = centerMap[activeSurfaceId];
    surfaceId = activeSurfaceId;
    surfaceFound = true;
    if (rotatePlaneWithDetectedNormal) {
      transform.localRotation = originalRotation_;
      transform.Rotate(activeSurface.rotation.eulerAngles);
    }

    if (deformToSurface) {
      DeformMesh(activeSurface.mesh);
    }

    if(triggerReady){
      if(onSurfaceReady!=null){
        onSurfaceReady.Invoke();
      }
    }
    if (triggerAttach) {
      if (onSurfaceAttach != null){
        onSurfaceAttach.Invoke();
      }
    } else {
      if (onSurfaceSwitch != null) {
        onSurfaceSwitch.Invoke();
      }
    }

    return true;
  }

  private void DeformMesh(Mesh mesh) {
    //Translate world positioned surface vertices to local positioned mesh vertices
    Vector3[] relVertices = new Vector3[mesh.vertices.Length];
    for (int i = 0; i < mesh.vertices.Length; ++i) {
      relVertices[i] = transform.InverseTransformPoint(mesh.vertices[i]);
    }

    Vector2[] relUvs = new Vector2[mesh.uv.Length];
    for (int i = 0; i < mesh.vertices.Length; ++i) {
      relUvs[i] = new Vector2(relVertices[i].x, relVertices[i].z);
    }

    // The number of vertices change so we need to create a new mesh
    deformMesh.Clear();
    deformMesh.vertices = relVertices;
    deformMesh.normals = mesh.normals;
    deformMesh.uv = relUvs;
    deformMesh.triangles = mesh.triangles;

    if (meshCollider != null) {
      meshCollider.sharedMesh = deformMesh;
    }
  }

  private void SetHidden(bool hide) {
    if (hidden == hide) {
      return;
    }

    if (hide) {
      foreach (Transform child in transform) {
        if (child.gameObject.activeSelf) {
          child.gameObject.SetActive(false);
          hiddenChildren.Add(child.gameObject);
        }
      }
      foreach (var collider in GetComponents<Collider>()) {
        if (collider.enabled) {
          collider.enabled = false;
          hiddenColliders.Add(collider);
        }
      }

      foreach (var renderer in GetComponents<Renderer>()) {
        if (renderer.enabled) {
          renderer.enabled = false;
          hiddenRenderers.Add(renderer);
        }
      }
    } else {
      foreach (var child in hiddenChildren) {
        child.SetActive(true);
      }
      foreach (var collider in hiddenColliders) {
        collider.enabled = true;
      }
      foreach (var renderer in hiddenRenderers) {
        renderer.enabled = true;
      }

      hiddenChildren.Clear();
      hiddenRenderers.Clear();
      hiddenColliders.Clear();
    }

    hidden = hide;
  }

  private void SetHeight(float h) {
    transform.position = new Vector3 (transform.position.x, h, transform.position.z);
  }

  private Vector3 GetVertexCenter(Mesh mesh) {
    double x = 0.0;
    double y = 0.0;
    double z = 0.0;

    foreach (Vector3 vertex in mesh.vertices) {
      x += vertex.x;
      y += vertex.y;
      z += vertex.z;
    }
    double il = 1.0 / mesh.vertices.Length;

    return new Vector3((float)(x * il), (float)(y * il), (float)(z * il));
  }

  private bool IsValidGroundSurface(XRSurface surface) {
    if (surface.type != XRSurface.Type.HORIZONTAL_PLANE) {
        return false;
    }

    Vector3 centerVertex = GetVertexCenter(surface.mesh);
    // New ground surface must be:
    //   -below a certain cutoff
    //   -lowest surface seen so far (and at least MIN_GROUND_CHANGE_DISTANCE lower)
    bool surfaceIsGroundHeight = centerVertex.y < Camera.main.transform.position.y - MIN_GROUND_DISTANCE_FROM_PHONE;
    bool surfaceIsLowestSurface = !surfaceFound || centerMap[surfaceId].y - centerVertex.y > MIN_GROUND_CHANGE_DISTANCE;

    return surfaceIsGroundHeight && surfaceIsLowestSurface;
  }
}
