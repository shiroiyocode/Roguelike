using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        if (target == null)
        {
            // Try to find player if target not assigned
            GameObject player =
                GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
            return;
        }

        Vector3 desiredPosition =
            target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed == 0 ? 1f : smoothSpeed
        );
    }
}