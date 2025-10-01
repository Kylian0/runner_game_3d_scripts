using Unity.VisualScripting;
using System.Collections;
using UnityEngine;


/// <summary>
/// 
/// LandManager have to know all the land pieces
/// He had to know where the character is
/// He had to take the position X of a lane on the land where the character is
/// 
/// </summary>

public class LandManager : MonoBehaviour
{
    // References to other components
    public GroundSegmenter[] lands;
    public CharacterMovement character;
    private GroundSegmenter currentLand;

    private int currentLandIndex;

    public void Start()
    {
        KnowCurrentLand();
    }

    private void Update()
    {
        KnowCurrentLane();
    }


    private int GetLandIndex(Vector3 characterPosition)
    {
        return 0;

    }

    private int GetLaneIndex()
    {
        // Check if references are assigned
        if ( currentLand == null )
        {
            Debug.Log("Current land is not assigned in LandManager.");
            return -1;
        }

        // Get the character's Z position
        float characterX = character.transform.position.x;

        // Minimal distance to find the closest lane
        float distanceTolerance = 0.05f;
        // Index of the closest lane

        for (int i = 0; i < 3 ; i++)
        {
            // Get the position of the lane i
            Vector3 lanePosition = currentLand.GetLaneWorldCenter(i);

            if (Mathf.Abs(characterX - lanePosition.x) < distanceTolerance)
            {
                return i;
            }

        }
        return -1;
        
    }

    public void KnowCurrentLand() 
    {
        // Check if references are assigned
        if (character == null)
        {
            Debug.Log("Character is not assigned in LandManager.");
            return;
        };
        if (lands == null || lands.Length == 0)
        {
            Debug.Log("Lands are not assigned in LandManager.");
            return;
        };

        // Determine which land the character is currently on
        currentLandIndex = GetLandIndex(character.transform.position);
        currentLand = lands[currentLandIndex];

        int currentLaneIndex = GetLaneIndex();
    }

    private int KnowCurrentLane()
    {
        if (currentLand == null)
        {
            return -1;
        }

        if (currentLand != null)
        {
            currentLand.RebuildLaneMarker();
        }

        int laneIndex = GetLaneIndex();
        return laneIndex;
    }

}
