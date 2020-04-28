using UnityEngine;

/// <summary>
/// Directs a camera to track the object in front of it.
/// </summary>
public class FollowCamera : MonoBehaviour
{
    /// <summary>
    /// What should we follow?
    /// </summary>
    [SerializeField] public Transform Target;
    
    /// <summary>
    /// How far behind the target should the camera sit?
    /// </summary>
    [SerializeField] public Vector3 CameraOffset = new Vector3(0f, 2f, 5f);

    /// <summary>
    /// How fast the camera should smooth between current and target positions. 
    /// </summary>
    private float LerpDampenConstant = 0.15f;

    private Vector3 CurrentVelocity = Vector3.one;

    /// <summary>
    /// Returns true if the current position is currently visible from this camera.
    /// </summary>
    public bool IsVisibleFromFollowCam(Vector3 position)
    {
        var normalizedViewportCoords = GetComponent<Camera>().WorldToViewportPoint(position);
        return (
            normalizedViewportCoords.x >= 0
            && normalizedViewportCoords.y >= 0
            && normalizedViewportCoords.z >= 0
            && normalizedViewportCoords.x <= 1
            && normalizedViewportCoords.y <= 1
        );
    }

    /// <summary>
    /// Called after all Update() functions have run.
    /// </summary>
    private void LateUpdate()
    {
        // Update camera position over time
        transform.position = Vector3.SmoothDamp(
            transform.position,  // Camera's current position,
            Target.position + (Target.rotation * CameraOffset),  // Camera's target position
            ref CurrentVelocity,  // Camera's current speed (to be modified by SmoothDamp)
            LerpDampenConstant  // How long it should take to get there
        );

        transform.LookAt(Target, Target.up);
    }
}
