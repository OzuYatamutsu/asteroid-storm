using UnityEngine;

/// <summary>
/// Controls movement and handling of Powerup objects created
/// in the game field.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class PowerupController : MonoBehaviour
{
    /// <summary>
    /// Powerups should not move if game is paused.
    /// </summary>
    private bool ShouldMove;

    private void OnEnable()
    {
        EventManager.onGameStart += OnGameResumeOrStarted;
        EventManager.onGamePaused += OnGamePaused;
        EventManager.onGameResume += OnGameResumeOrStarted;
    }

    private void OnDisable()
    {
        EventManager.onGameStart -= OnGameResumeOrStarted;
        EventManager.onGamePaused -= OnGamePaused;
        EventManager.onGameResume -= OnGameResumeOrStarted;
    }

    /// <summary>
    /// Which direction should this Powerup move?
    /// </summary>
    public Vector3 MovementVector {get; private set;}
    private Vector3 RotationVector;

    /// <summary>
    /// How fast is the Powerup moving along its
    /// movement vector?
    /// </summary>
    public float Speed {get; private set;}

    private Renderer IsVisibleRenderer;
    private Rigidbody ChildRigidBody;

    /// <summary>
    /// Called when the Powerup is first created.
    /// </summary>
    void Start()
    {
        // Set renderer component
        IsVisibleRenderer = gameObject.GetComponent<Renderer>();

        // Set movement vector of the Powerup
        MovementVector = Quaternion.Euler(
            Random.Range(-45,45),  // x
            Random.Range(-45,45),  // y
            Random.Range(-45,45)  // z
        ) * new Vector3(
            transform.forward.x * Random.Range(-10.0f, 10.0f),
            transform.forward.y * Random.Range(-10.0f, 10.0f),
            transform.forward.z * Random.Range(-10.0f, 10.0f)
        );

        // Set rotation vector of the Powerup
        RotationVector = Random.insideUnitSphere;

        // Set rigidbody
        ChildRigidBody = gameObject.GetComponent<Rigidbody>();

        // How will the powerup rotate?
        ChildRigidBody.angularVelocity = RotationVector;

        // How will the powerup move?
        ChildRigidBody.velocity = MovementVector;
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update() {}

    void OnDestroy()
    {
        Destroy(gameObject);
    }

    private void OnGamePaused()
    {
        // Stop powerup from moving
        ChildRigidBody.velocity = Vector3.zero;
    }

    private void OnGameResumeOrStarted()
    {
        // Start powerup moving again
        ChildRigidBody.velocity = MovementVector;
    }
}
