using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>   
/// Divide the field into 3 segments to create 3 lanes for the player to travel on.
/// Lanes are calculated on the X-axis (classic runner layout).  
/// Can create markers visible in the Scene for each lane.
/// </summary>
[ExecuteAlways]
public class GroundSegmenter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Ground mesh transform")]
    public Transform groundField;

    [Header("Cutting parameters")]
    [Tooltip("Number of lanes (fixed at 3)")]
    public int laneNumber = 3;

    [Tooltip("Space between lanes")]
    public float laneSpace = 0f;

    [Header("Markers and Visualization")]
    [Tooltip("Create marker GameObjects positioned in the center of each lane")]
    public bool createMarker = true;

    [Tooltip("Show Lane Gizmos in Scene View")]
    public bool showGizmo = true;

    public Color gizmoFillColor = new Color(0f, 1f, 1f, 0.12f);
    public Color gizmoWireColor = new Color(0f, 1f, 1f, 0.9f);

    private const string MarkerContainerName = "LaneMarker";

    // Lane center positions cache
    private Vector3[] laneCenter;

    // Last bounds calculated
    private Bounds lastBound;

    private void Reset()
    {
        AutoAssignGround();
        RebuildLaneMarker();
    }

    private void OnValidate()
    {
        laneNumber = Mathf.Max(1, laneNumber);

#if UNITY_EDITOR
        // Delay call to avoid modifying prefab assets
        EditorApplication.delayCall += () =>
        {
            if (this != null && !EditorUtility.IsPersistent(gameObject))
            {
                RebuildLaneMarker();
            }
        };
#else
        RebuildLaneMarker();
#endif
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (laneCenter == null || laneCenter.Length != laneNumber)
            {
                RebuildLaneMarker();
            }
        }
    }

    [ContextMenu("Rebuild Lane Marker")]
    public void RebuildLaneMarker()
    {
        AutoAssignGround();
        RemoveOldMarkersContainer();

        Transform markerContainer = null;

        if (createMarker)
        {
            GameObject container = new GameObject(MarkerContainerName);
            container.transform.SetParent(transform, false);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;
            markerContainer = container.transform;
        }

        // Get bounds from the ground
        Renderer groundRenderer = groundField != null ? groundField.GetComponent<Renderer>() : null;
        Bounds bounds = new Bounds();

        if (groundRenderer != null)
        {
            bounds = groundRenderer.bounds;
        }
        else
        {
            var parentCollider = GetComponent<BoxCollider>();
            if (parentCollider != null)
            {
                bounds = parentCollider.bounds;
                Debug.LogWarning("[GroundSegmenter] : No renderer found. Using parent BoxCollider bounds.");
            }
            else
            {
                Debug.LogError("[GroundSegmenter] : Cannot determine ground size.");
                return;
            }
        }

        lastBound = bounds;

        // World dimensions
        float fieldDimensionX = bounds.size.x;
        float fieldDimensionZ = bounds.size.z;

        // Calculate lane width
        float totalSpace = laneSpace * (laneNumber - 1);
        float laneWidth = (fieldDimensionX - totalSpace) / laneNumber;

        if (laneWidth <= 0f)
        {
            Debug.LogError("[GroundSegmenter] : Calculated lane width <= 0.");
            return;
        }

        laneCenter = new Vector3[laneNumber];

        Vector3 center = bounds.center;
        Vector3 right = groundField != null ? groundField.right : transform.right;

        // Correct centering for X-axis lanes
        float startX = -((laneWidth * laneNumber) + (laneSpace * (laneNumber - 1))) / 2f + laneWidth / 2f;

        for (int i = 0; i < laneNumber; i++)
        {
            float offsetX = startX + i * (laneWidth + laneSpace);
            Vector3 laneWorld = center + right * offsetX;

            // Z position stays at center of ground
            laneWorld = new Vector3(laneWorld.x, laneWorld.y, center.z);

            laneCenter[i] = laneWorld;

            if (createMarker && markerContainer != null)
            {
                GameObject marker = new GameObject($"LaneMarker_{i}");
                marker.transform.SetParent(markerContainer, worldPositionStays: true);
                marker.transform.position = laneWorld;
                marker.transform.rotation = groundField != null ? groundField.rotation : transform.rotation;
            }
        }
    }

    private void AutoAssignGround()
    {
        if (groundField != null) return;

        Transform child = transform.Find("ground");
        if (child != null) { groundField = child; return; }

        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "ground" || t.name == "Ground")
            {
                groundField = t;
                return;
            }
        }
    }

    private void RemoveOldMarkersContainer()
    {
        Transform oldMarkers = transform.Find(MarkerContainerName);
        if (oldMarkers == null) return;

#if UNITY_EDITOR
        // Only remove from scene, not from prefab asset
        if (!EditorUtility.IsPersistent(oldMarkers.gameObject))
        {
            if (Application.isPlaying)
                Destroy(oldMarkers.gameObject);
            else
                DestroyImmediate(oldMarkers.gameObject);
        }
#else
        Destroy(oldMarkers.gameObject);
#endif
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        if (laneCenter != null && laneCenter.Length == laneNumber)
        {
            float boundsY = lastBound.size.y > 0.0001f ? lastBound.size.y : 0.2f;
            float laneWidth = (lastBound.size.x - laneSpace * (laneNumber - 1)) / laneNumber;
            float depth = lastBound.size.z > 0.0001f ? lastBound.size.z : 1f;

            for (int i = 0; i < laneNumber; i++)
            {
                Vector3 size = new Vector3(laneWidth, boundsY, depth);
                Gizmos.color = gizmoFillColor;
                Gizmos.DrawCube(laneCenter[i], size);
                Gizmos.color = gizmoWireColor;
                Gizmos.DrawCube(laneCenter[i], size);
            }
        }
    }

    public Vector3 GetLaneWorldCenter(int laneIndex)
    {
        if (laneCenter == null || laneIndex < 0 || laneIndex >= laneCenter.Length)
        {
            Debug.LogWarning("[GroundSegmenter] GetLaneWorldCenter: invalid index or lanes not calculated.");
            return transform.position;
        }
        return laneCenter[laneIndex];
    }

    public Vector3 PositionToLane(Vector3 position, int laneIndex)
    {
        Vector3 laneCenterPos = GetLaneWorldCenter(laneIndex);
        return new Vector3(laneCenterPos.x, position.y, laneCenterPos.z);
    }
}
