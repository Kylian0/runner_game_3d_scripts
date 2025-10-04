using UnityEngine;

public class CharacterCamera : MonoBehaviour
{
    // Player reference
    [SerializeField] private Transform player;
    // Camera offset from the player
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);

    // Update is called once per frame
    private void CameraUpdate()
    {
        if (player != null)
        {
            // Update camera position and rotation
            transform.position = player.position + offset;
            transform.LookAt(player);
        };
    }
}
