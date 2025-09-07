using UnityEngine;


/// <summary>
/// 
/// Divide the field into 3 segments to create 3 lanes for the player to travel on.
/// Divisez le terrain en 3 segments pour créer 3 voies sur lesquelles le joueur pourra circuler.
/// 
/// </summary>

[ExecuteAlways]

public class GroundSegmenter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Ground mesh transform")]
    public Transform groundField;

    [Header("Cutting parameters")]
    [Tooltip("Number of lanes, fixed at 3")]
    public int laneNumber = 3;

    [Tooltip("Espace entre les voies")]
    public float laneSpace = 0f;

    [Header("Markers and Visualization")]
    [Tooltip("Create marker GameObjects positioned in the center of each lane")]
    public bool createMarker = true;

    [Tooltip("Show Lane Gizmos in Scene View")]
    public bool showGizmo = true;

    public Color gizmoFillColor = new Color(0f, 1f, 1f, 0.12f);
    public Color gizmoWireColor = new Color(0f, 1f, 1f, 0.9f);

    // Container name for markers
    private const string MarkerContainerName = "LaneMarker";

    // Lane Center Cache
    private Vector3[] laneCenter;

    // Last Bound calculate
    private Bounds lastBound;

    private void Reset()
    {
        // automatic assignment when adding the component
        AutoAssignGround();
        RebuildLaneMarker();
    }

    private void OnValidate()
    {
        // Force to 3 lanes
        laneNumber = Mathf.Max(1, laneNumber);

        if (laneNumber != 3)
        {
            laneNumber = 3;

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () => { RebuildLaneMarker(); };
            #else
            RebuildLaneMarkers();
            #endif

        }
        else
        {
            RebuildLaneMarker();
        }
        ;
    }

    private void Awake()
    {
        // if we haven't built the markers yet, we rebuild them.
        if (Application.isPlaying)
        {
            if (laneCenter == null || laneCenter.Length != laneNumber)
            {
                RebuildLaneMarker();
            }
            ;
        };
    }

    [ContextMenu("Rebuild Lane Marker")]
    public void RebuildLaneMarker() 
    {
        // Make sure ground is assign.
        AutoAssignGround();

        // We retrieve the bounds from the ground renderer if possible.
        Renderer groundRenderer = null;
        Bounds bounds = new Bounds();

        if ( groundField != null )
        {
            groundRenderer = groundField.GetComponent<Renderer>();
        };

        if (groundRenderer != null)
        {
            bounds = groundRenderer.bounds;
        }
        else
        {
            // Last resort : use the parent's BoxCollider.
            var parentCollider = GetComponent<BoxCollider>();

            if (parentCollider != null)
            {
                bounds = parentCollider.bounds;
                Debug.LogWarning("[GroundSegmenter] : No renderer found on 'ground'. Using parent BoxCollider bounds. Warning: these bounds may include walls.");

            }
            else
            {
                Debug.LogError("[GroundSegmenter] : Unable to determine ground size: assign 'ground' (with a MeshRenderer) or add a BoxCollider on the parent.");
                return;
            };
        };

        // Save for Gizmo.
        lastBound = bounds;

        // World Dimension
        float fieldDimensionX = bounds.size.x;
        float fieldDimensionZ = bounds.size.z;

        // Calcul every lanes.
        float totalSpace = laneSpace * (laneNumber - 1);
        float totalDimensionX = (fieldDimensionX - totalSpace) / laneNumber;

        if (totalDimensionX <= 0f)
        {
            Debug.LogError("[GroundSegmenter] : Calculated width <= 0. Checks ground size and laneGap.");
            return;
        };

        // removal of the old container if present.
        RemoveOldMarkersContainer();

        Transform markerContainer = null;

        if (createMarker)
        {
            var newGameObject = new GameObject(MarkerContainerName);
            newGameObject.transform.SetParent(transform, worldPositionStays: false);
            newGameObject.transform.localPosition = Vector3.zero;
            newGameObject.transform.localRotation = Quaternion.identity;
            newGameObject.transform.localScale = Vector3.one;
            markerContainer = newGameObject.transform;
        };

        laneCenter = new Vector3[laneNumber];


        // Landmarks to take into account the orientation of the ground in the world.

        Vector3 center = bounds.center;
        Vector3 right = (groundField != null) ? groundField.right : transform.right;
        Vector3 up = (groundField != null) ? groundField.up : transform.up;

        // Calcul position of the first lane ( at left ).
        float firstLane = -fieldDimensionX * 0.5f + totalDimensionX * 0.5f;

        for (int i = 0; i < laneNumber; i++)
        {
            float offSetX = firstLane + i * (totalDimensionX + laneSpace);
            Vector3 laneCenterWorld = center + right * offSetX;

            laneCenter[i] = laneCenterWorld;

            if (createMarker && markerContainer != null)
            {
                var newMarker = new GameObject($"LaneMarker_{i}");
                newMarker.transform.SetParent(markerContainer, worldPositionStays: true);
                newMarker.transform.position = laneCenterWorld;

                float laneMarkerChildPosZ = -fieldDimensionZ * 0.5f + + (fieldDimensionZ / laneNumber) * (i + 0.5f);
                newMarker.transform.position = new Vector3(0f, laneCenterWorld.y, center.z + laneMarkerChildPosZ);

                // Align the rotation of the world
                if (groundField != null)
                {
                    newMarker.transform.rotation = groundField.rotation;
                }
                else
                {
                    newMarker.transform.rotation = transform.rotation;
                };
            };
        };
    }

    private void AutoAssignGround()
    {
        if (groundField != null) return;

        // Search direct child.
        Transform transformFind = transform.Find("ground");

        if (transformFind != null)
        {
            groundField = transformFind;
            return;
        };

        foreach (var child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "ground" || child.name == "Ground")
            {
                groundField = child;
                return;
            };
        };
    }

    private void RemoveOldMarkersContainer()
    {
        var oldMarkers = transform.Find(MarkerContainerName);
        
        if (oldMarkers == null)
        {
            return;
        };

        #if UNITY_EDITOR

        if (Application.isPlaying)
        {
          Destroy(oldMarkers.gameObject);
        }
        else
        {
          DestroyImmediate(oldMarkers.gameObject);
        };
        #else

        Destroy(oldMarkers.gameObject);

        #endif
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        // If we have calculate center, we draw a box for all lanes.
        if (laneCenter != null && laneCenter.Length == laneNumber)
        {

            float boundsY = (lastBound.size.y > 0.0001f) ? lastBound.size.y : 0.2f;

            // Calcul width and depth with bounds.
            float totalDimensionX = (lastBound.size.x > 0.0001f) ? ((lastBound.size.x - laneSpace * (laneNumber - 1)) / laneNumber) : 0.5f;
            float depth = (lastBound.size.z > 0.0001f) ? lastBound.size.z : 1f;

            Gizmos.color = gizmoFillColor;

            for (int i = 0; i < laneNumber; i++)
            {
                Vector3 center = laneCenter[i];
                Vector3 size = new Vector3(totalDimensionX, boundsY, depth);

                Gizmos.DrawCube(center, size);

                Gizmos.color = gizmoWireColor;
                Gizmos.DrawCube(center, size);
                Gizmos.color = gizmoFillColor;
            };
        };
    }

    public Vector3 GetLaneWorldCenter(int laneIndex)
    {
        if (laneCenter == null || laneIndex < 0 || laneIndex >= laneCenter.Length )
        {
            Debug.LogWarning("[GroundSegmenter] GetLaneWorldCenter: invalid index or lanes not calculated.");
            return transform.position;
        };
        return laneCenter[laneIndex];
    }

    public Vector3 PositionToLane(Vector3 position, int laneIndex)
    {
        Vector3 laneCenterPos = GetLaneWorldCenter(laneIndex);
        return new Vector3(laneCenterPos.x, position.y, laneCenterPos.z);
    }


};
