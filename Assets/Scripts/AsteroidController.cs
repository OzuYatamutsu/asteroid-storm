using UnityEngine;

/// <summary>
/// Controls movement and handling of Asteroid objects created
/// in the game field.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class AsteroidController : MonoBehaviour
{
    // The below are used to calculate initial size,
    // rotation speed, and movement speed on creation.
    public const float SCALE_MIN_BASE = 2.5f;
    public const float SCALE_MAX_BASE = 15.5f;

    /// <summary>
    /// How fast should the asteroid move?
    /// </summary>
    [SerializeField] public float Difficulty;

    /// <summary>
    /// Asteroids should not move if game is paused.
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
    /// Which direction should this Asteroid move?
    /// </summary>
    public Vector3 MovementVector {get; private set;}
    private Vector3 RotationVector;

    /// <summary>
    /// How fast is the Asteroid moving along its
    /// movement vector?
    /// </summary>
    public float Speed {get; private set;}

    private Renderer IsVisibleRenderer;
    private Rigidbody ChildRigidBody;

    /// <summary>
    /// Called when the Asteroid is first created.
    /// </summary>
    void Start()
    {
        // Set renderer component
        IsVisibleRenderer = gameObject.GetComponent<Renderer>();

        // As more time elapses, new AsteroidController begin to move faster (min 1)
        // and the scale of the asteroids begins to increase
        Difficulty = (
            (GameManager.Instance?.ElapsedTimeSecs ?? 0) == 0
            ? 1f
            : (GameManager.Instance.ElapsedTimeSecs * 0.5f)
        );

        // Set size of the Asteroid using difficulty to scale up accordingly
        var difficultyScaleFactor = (1 + (0.1f * Difficulty));

        transform.localScale = new Vector3(
            Random.Range(SCALE_MIN_BASE * difficultyScaleFactor, SCALE_MAX_BASE * difficultyScaleFactor),  // x
            Random.Range(SCALE_MIN_BASE * difficultyScaleFactor, SCALE_MAX_BASE * difficultyScaleFactor),  // y
            Random.Range(SCALE_MIN_BASE * difficultyScaleFactor, SCALE_MAX_BASE * difficultyScaleFactor)  // z
        );

        // Set movement vector of the Asteroid
        MovementVector = Quaternion.Euler(
            Random.Range(-45,45),  // x
            Random.Range(-45,45),  // y
            Random.Range(-45,45)  // z
        ) * new Vector3(
            transform.forward.x * Random.Range(-10.0f, 10.0f),
            transform.forward.y * Random.Range(-10.0f, 10.0f),
            transform.forward.z * Random.Range(-10.0f, 10.0f)
        ) * Difficulty;

        // Set rotation vector of the Asteroid
        RotationVector = Random.insideUnitSphere;

        // Set rigidbody
        ChildRigidBody = gameObject.GetComponent<Rigidbody>();

        // How will the asteroid rotate?
        ChildRigidBody.angularVelocity = RotationVector;

        // How will the asteroid move?
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
        // Stop asteroid from moving
        ChildRigidBody.velocity = Vector3.zero;
    }

    private void OnGameResumeOrStarted()
    {
        // Start asteroid moving again
        ChildRigidBody.velocity = MovementVector;
    }
}
