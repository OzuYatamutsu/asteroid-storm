using UnityEngine;

/// <summary>
/// Dictates movement of the plane controlled by the player.
/// </summary>
public class CubeShipPilot : MonoBehaviour
{
    /// <summary>
    /// How fast should the ship move forward NOW?
    /// </summary>
    [SerializeField] public float ShipSpeed = 0f;

    /// <summary>
    /// How fast should the ship move at its maximum speed?
    /// </summary>
    [SerializeField] public float ShipSpeedMax = 100f;
    [SerializeField] private const float AbsoluteSpeedMax = 250f;

    /// <summary>
    /// How fast should the ship increase in speed
    /// up to the max each tenth of a second?
    /// </summary>
    [SerializeField] public float Acceleration = 3f;

    /// <summary>
    /// How fast should the ship turn?
    /// </summary>
    [SerializeField] private float TurnSpeed = 50f;

    /// <summary>
    /// Used to adjust max speed as a function of game time
    /// </summary>
    [SerializeField] public float Difficulty;

    /// <summary>
    /// The RidgidBody assigned to this ship.
    /// </summary>
    [SerializeField] new public Rigidbody rigidbody;

    /// <summary>
    /// To what degree should we be propelled from the object we have struck?
    /// </summary>
    [SerializeField] public float CollisionForceMultiplier = -10f;

    /// <summary>
    /// How much damage can this ship take without exploding?
    /// </summary>
    [SerializeField] public float HullStrength = 100f;

    /// <summary>
    /// Controls whether this ship is immune to hull damage.
    /// </summary>
    [SerializeField] public bool IsInvincible = true;

    /// <summary>
    /// (Reference to explosion to spawn on damage)
    /// </summary>
    [SerializeField] public GameObject DamageExplosionPrefab;

    /// <summary>
    /// (Reference to explosion to spawn on death)
    /// </summary>
    [SerializeField] public GameObject DeathExplosionPrefab;
    
    private bool ShouldMove;
    private AudioSource ThrusterAudioEmitter;

    /// <summary>
    /// Register event handlers
    /// </summary>
    void OnEnable()
    {
        EventManager.onGameStart += OnGameStart;
        EventManager.onGamePaused += OnGamePaused;
        EventManager.onGameResume += OnGameResume;
        EventManager.onGameEnd += OnGameEnd;

        InvokeRepeating("Accelerate", 0.1f, 0.1f);
        ThrusterAudioEmitter = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Unregister event handlers
    /// </summary>
    void OnDisable()
    {
        EventManager.onGameStart -= OnGameStart;
        EventManager.onGamePaused -= OnGamePaused;
        EventManager.onGameResume -= OnGameResume;
        EventManager.onGameEnd -= OnGameEnd;
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        if (ShouldMove) {
            // Forward movement
            Move();

            // Rotational/angular movement
            Turn();

            // Modify max speed as a function of gametime
            Difficulty = (GameManager.Instance.ElapsedTimeSecs * 0.001f);

            // If AudioSource is attached, modify the pitch based on speed
            if (ThrusterAudioEmitter != null)
                ThrusterAudioEmitter.pitch = (ShipSpeed / AbsoluteSpeedMax) * 2.5f;
        }

        // If we're out of hull strength, end the game.
        if (HullStrength <= 0f)
            EventManager.EndGame();
    }

    /// <summary>
    /// Called when the ship hits something!
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collided with {collision.gameObject}!");
        Vector3 collidedAtPosition = collision.gameObject.transform.position;

        rigidbody.AddForceAtPosition(
            (collidedAtPosition - transform.position) * CollisionForceMultiplier,
            collidedAtPosition,
            ForceMode.Impulse
        );

        // Rotate slightly as well to reduce chance we will
        // run into the object repeatedly
        transform.Rotate(
            transform.rotation.x + 15,
            transform.rotation.y + 15,
            transform.rotation.z + 15
        );
    }

    void OnCollisionExit(Collision collision)
    {
        // Set these to 0 vectors so we aren't flung off into space
        // on each collision
        rigidbody.velocity = new Vector3(0, 0, 0);
        rigidbody.angularVelocity = new Vector3(0, 0, 0);

        // Take damage!
        if (!IsInvincible)
        {
            HullStrength -= 30f;
            Instantiate(DamageExplosionPrefab, transform.position, Quaternion.identity);

            // Slow the ship down by 90%
            ShipSpeed *= 0.1f;

            // Reset max speed
            ShipSpeedMax = 100f;

            // Make the ship invincible for a small period of time
            Debug.Log($"Enabling invincibility (health at {HullStrength}%).");
            IsInvincible = true;
            Invoke("SetNotInvincible", 1);  // second
        }
    }

    /// <summary>
    /// What happens when we hit something with a trigger (e.g. a powerup?)
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.name.StartsWith("Powerup"))
            return;

        // Pick up the powerup! Increase player's health (if not already at max).
        HullStrength += 20;
        if (HullStrength > 100f)
            HullStrength = 100f;
        
        // Increase the player's score by 5%.
        GameManager.Instance.Score = (uint) Mathf.RoundToInt(GameManager.Instance.Score * 1.05f);
    }

    /// <summary>
    /// What happens after we hit something with a trigger?
    /// </summary>
    private void OnTriggerExit(Collider other)
    {       
        if (!other.gameObject.name.StartsWith("Powerup"))
            return;

        // Powerup has done its job, so remove this object
        Destroy(other.gameObject);
    }

    private void SetNotInvincible()
    {
        if (IsInvincible)
            Debug.Log("Disabling invincibility.");
        IsInvincible = false;
    }

    /// <summary>
    /// Returns a vector containing the relative distance between this ship
    /// and another object in space.
    /// </summary>
    public Vector3 GetRelativeDistance(Vector3 obj, bool absolute = false)
    {
        if (!absolute)
            return transform.position - obj;
        return new Vector3(
            Mathf.Abs(transform.position.x - obj.x),
            Mathf.Abs(transform.position.y - obj.y),
            Mathf.Abs(transform.position.z - obj.z)
        );
    }

    /// <summary>
    /// Controls forward movement. The ship always tries to move forward if it can.
    /// </summary>
    private void Move()
    {
        // Foward movement
        transform.position += (
            transform.forward * ShipSpeed * Time.deltaTime  // Base forward movement
        );
    }

    /// <summary>
    /// Controls plane rotation (pitch, yaw, roll).
    /// </summary>
    private void Turn()
    {
        var rotationalInput = Input.GetAxis("Horizontal");
        var pitchInput = Input.GetAxis("Vertical");

        // left-right
        var yaw = rotationalInput * TurnSpeed * Time.deltaTime;

        // up-down
        var pitch = pitchInput * TurnSpeed * Time.deltaTime;

        // Rotational movement
        transform.Rotate(pitch, yaw, 0);
    }

    /// <summary>
    /// Increase ShipSpeed if lesser than the max
    /// </summary>
    private void Accelerate()
    {
        if (!ShouldMove)
            return;
        if (ShipSpeedMax > AbsoluteSpeedMax)
        {
            ShipSpeedMax = AbsoluteSpeedMax;
            return;
        }
            

        ShipSpeed += Acceleration;
        ShipSpeedMax += ShipSpeedMax * Difficulty;

        if (ShipSpeed > ShipSpeedMax)
            ShipSpeed = ShipSpeedMax;
        if (ShipSpeed == ShipSpeedMax)
            return;
    }


    private void OnGameStart()
    {
        ShouldMove = true;
        IsInvincible = false;
    }

    private void OnGamePaused()
    {
        ShouldMove = false;
    }

    private void OnGameResume()
    {
        ShouldMove = true;
    }

    private void OnGameEnd()
    {
        ShouldMove = false;

        Instantiate(DeathExplosionPrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}
