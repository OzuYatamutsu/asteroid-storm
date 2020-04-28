using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages how AsteroidManager objects are spawned in.
/// </summary>
public class AsteroidManager : MonoBehaviour
{
    /// <summary>
    /// What should be spawned in?
    /// </summary>
    [SerializeField] public AsteroidController SpawnTarget;

    /// <summary>
    /// How many Asteroids should we start with?
    /// </summary>
    [SerializeField] public const uint InitialAsteroidCount = 250;

    /// <summary>
    /// How many Asteroids should be on the field now?
    /// </summary>
    [SerializeField] public uint MaxAsteroidCount;

    /// <summary>
    /// Holds references to the Asteroids currently on the game field.
    /// </summary>
    private List<AsteroidController> ActiveAsteroids;

    private static System.Random _rand = new System.Random();

    // The following are parameters that are used to determine
    // where the Asteroid objects should be spawned in, and
    // when the Asteroid objects should be despawned.
    private const float DISTANCE_X_MIN = 100f;
    private const float DISTANCE_X_MAX = 500f;
    private const float DISTANCE_Y_MIN = DISTANCE_X_MIN;
    private const float DISTANCE_Y_MAX = DISTANCE_X_MAX;
    private const float DISTANCE_Z_MIN = DISTANCE_X_MIN;
    private const float DISTANCE_Z_MAX = DISTANCE_X_MAX;
    private const float SPAWN_DISTANCE_MAX = 750f;
    private const float DESPAWN_DISTANCE_MAX = 750f;  // (Behind the player.)

    /// <summary>
    /// Are we allowed to change the state of asteroids in the scene?
    /// </summary>
    private bool SpawningEnabled;

    private void OnEnable()
    {
        EventManager.onGameStart += OnGameStart;
        EventManager.onGamePaused += OnGamePausedOrEnded;
        EventManager.onGameResume += OnGameResume;
    }

    private void OnDisable()
    {
        EventManager.onGameStart -= OnGameStart;
        EventManager.onGamePaused -= OnGamePausedOrEnded;
        EventManager.onGameResume -= OnGameResume;
    }

    /// <summary>
    /// Called when the Asteroid is first created.
    /// </summary>
    private void Start()
    {
        MaxAsteroidCount = InitialAsteroidCount;
        ActiveAsteroids = new List<AsteroidController>();
        SpawnAsteroids(true);
    }

    private void LateUpdate()
    {
        if (SpawningEnabled)
        {
            // Despawn asteroids if necessary
            foreach (AsteroidController asteroid in GetAsteroidsToDespawn())
                DespawnAsteroid(asteroid);

            // Spawn asteroids if necessary
            SpawnAsteroids();

            // Finally, add one more asteroid to limit per sec of game time
            // (i.e., game gets harder over time)
            // MaxAsteroidCount = InitialAsteroidCount + GameManager.Instance.ElapsedTimeSecs;
        }
    }

    /// <summary>
    /// If the current number of Asteroid does not match AsteroidCount,
    /// spawn/despawn Asteroid to match this number.
    /// 
    /// Spawns in a distance around the player.
    /// </summary>
    private void SpawnAsteroids(bool initial = false)
    {
        if (ActiveAsteroids.Count == MaxAsteroidCount)
            return;

        // Spawn in asteroids as necessary
        if (ActiveAsteroids.Count < MaxAsteroidCount)
        {
            for (var i = ActiveAsteroids.Count; i != MaxAsteroidCount; i++)
                SpawnAsteroid(initial);
            return;
        }
    }

    /// <summary>
    /// Spawns a new Asteroid somewhere on the game field.
    /// </summary>
    private void SpawnAsteroid(bool initial)
    {
        // Where should the Asteroid be spawned relative to the player?
        var playerTransform = GameManager.Instance.PlayerShip.transform;
        var spawnLocation = new Vector3();

        if (initial)
        {
            // Treat {x,y,z}-min/max as a radius, so we subtract a random number as well
            // to give us a chance that we will spawn in behind the player
            var spawnOffset = new Vector3(
                Random.Range(DISTANCE_X_MIN, DISTANCE_X_MAX) - Random.Range(DISTANCE_X_MIN, DISTANCE_X_MAX),
                Random.Range(DISTANCE_Y_MIN, DISTANCE_Y_MAX) - Random.Range(DISTANCE_Y_MIN, DISTANCE_Y_MAX),
                Random.Range(DISTANCE_Z_MIN, DISTANCE_Z_MAX) - Random.Range(DISTANCE_Z_MIN, DISTANCE_Z_MAX)
            );

            spawnLocation = playerTransform.position + spawnOffset;
        }
        else
        {
            // Only spawn at max distance in front of player.
            spawnLocation = playerTransform.position + (playerTransform.forward * SPAWN_DISTANCE_MAX);

            // Add a random angle to spawn location so we don't spawn directly in front of player
            spawnLocation = Quaternion.Euler(
                Random.Range(-45,45),  // x
                Random.Range(-45,45),  // y
                Random.Range(-45,45)  // z
            ) * spawnLocation;

            // Abort if we would spawn out of view of the player
            if (!GameManager.Instance.FollowCam.IsVisibleFromFollowCam(spawnLocation))
                return;
        }

        
        ActiveAsteroids.Add(
            Instantiate(SpawnTarget, spawnLocation, Quaternion.identity, transform)
        );
    }

    /// <summary>
    /// Returns an array of Asteroid objects that are outside of maximum distance
    /// to the player, and are candidates for despawn.
    /// </summary>
    private AsteroidController[] GetAsteroidsToDespawn()
    {
        return ActiveAsteroids.Where(asteroid => AsteroidIsTooFar(asteroid))
                              .Where(asteroid => !GameManager.Instance.FollowCam.IsVisibleFromFollowCam(
                                  asteroid.gameObject.transform.position
                              ))
                              .ToArray();
    }

    /// <summary>
    /// Returns true if the asteroid is beyond DESPAWN_DISTANCE_MAX in any direction.
    /// </summary>
    private bool AsteroidIsTooFar(AsteroidController asteroid)
    {
        var relativeDistanceToPlayer = GameManager.Instance.PlayerShip.GetRelativeDistance(
            asteroid.transform.position, true
        );

        return (
            relativeDistanceToPlayer.x >= DESPAWN_DISTANCE_MAX
            || relativeDistanceToPlayer.y >= DESPAWN_DISTANCE_MAX
            || relativeDistanceToPlayer.z >= DESPAWN_DISTANCE_MAX
        );
    }

    /// <summary>
    /// Despawns a specific Asteroid somewhere on the game field.
    /// </summary>
    private void DespawnAsteroid(AsteroidController asteroid)
    {
        Destroy(asteroid);
        ActiveAsteroids.Remove(asteroid);
    }

    private void OnGameStart()
    {
        SpawningEnabled = true;
        Debug.Log("Spawning enabled!");
    }

    private void OnGamePausedOrEnded()
    {
        SpawningEnabled = false;
    }

    private void OnGameResume()
    {
        SpawningEnabled = true;
    }
}
