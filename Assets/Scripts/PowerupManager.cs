using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages how Powerup objects are spawned in.
/// </summary>
public class PowerupManager : MonoBehaviour
{
    /// <summary>
    /// What should be spawned in?
    /// </summary>
    [SerializeField] public PowerupController SpawnTarget;

    /// <summary>
    /// Holds references to the Powerups currently on the game field.
    /// </summary>
    private List<PowerupController> ActivePowerups;

    private static System.Random _rand = new System.Random();

    // The following are parameters that are used to determine
    // where the Powerup objects should be spawned in.
    private const float SPAWN_DISTANCE_MAX = 750f;
    private const float DESPAWN_DISTANCE_MAX = 750f;  // (Behind the player.)

    /// <summary>
    /// Are we allowed to change the state of Powerups in the scene?
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
    /// Called when the PowerupManager is first created.
    /// </summary>
    private void Start()
    {
        ActivePowerups = new List<PowerupController>();
    }

    private void LateUpdate()
    {
        if (SpawningEnabled)
        {
            // Despawn powerups if necessary
            ActivePowerups.RemoveAll(powerup => powerup == null);
            foreach (PowerupController powerup in GetPowerupsToDespawn())
                DespawnPowerup(powerup);

            // Spawn powerups if necessary
            SpawnPowerups();
        }
        else
        {
            // Start spawning powerups after 10 sec of elapsed game time
            SpawningEnabled = GameManager.Instance.ElapsedTimeSecs > 10;
        }
    }

    /// <summary>
    /// Spawns in a distance around the player.
    /// </summary>
    private void SpawnPowerups()
    {
        // 1% chance to spawn powerup
        if (_rand.Next(1, 100) < 99)
            return;

        SpawnPowerup();
    }

    /// <summary>
    /// Spawns a new Powerup somewhere on the game field.
    /// </summary>
    private void SpawnPowerup()
    {
        // Where should the Powerup be spawned relative to the player?
        var playerTransform = GameManager.Instance.PlayerShip.transform;
        var spawnLocation = new Vector3();

        while (!GameManager.Instance.FollowCam.IsVisibleFromFollowCam(spawnLocation)) {
            // Only spawn at max distance in front of player.
            spawnLocation = playerTransform.position + (playerTransform.forward * SPAWN_DISTANCE_MAX);

            // Add a random angle to spawn location so we don't spawn directly in front of player
            spawnLocation = Quaternion.Euler(
                Random.Range(-45,45),  // x
                Random.Range(-45,45),  // y
                Random.Range(-45,45)  // z
            ) * spawnLocation;
        }
        
        ActivePowerups.Add(
            Instantiate(SpawnTarget, spawnLocation, Quaternion.identity, transform)
        );
    }

    /// <summary>
    /// Returns an array of Powerup objects that are outside of maximum distance
    /// to the player, and are candidates for despawn.
    /// </summary>
    private PowerupController[] GetPowerupsToDespawn()
    {
        return ActivePowerups.Where(powerup => PowerupIsTooFar(powerup))
                             .Where(powerup => !GameManager.Instance.FollowCam.IsVisibleFromFollowCam(
                                  powerup.gameObject.transform.position
                              ))
                             .ToArray();
    }

    /// <summary>
    /// Returns true if the powerup is beyond DESPAWN_DISTANCE_MAX in any direction.
    /// </summary>
    private bool PowerupIsTooFar(PowerupController powerup)
    {
        var relativeDistanceToPlayer = GameManager.Instance.PlayerShip.GetRelativeDistance(
            powerup.transform.position, true
        );

        return (
            relativeDistanceToPlayer.x >= DESPAWN_DISTANCE_MAX
            || relativeDistanceToPlayer.y >= DESPAWN_DISTANCE_MAX
            || relativeDistanceToPlayer.z >= DESPAWN_DISTANCE_MAX
        );
    }

    /// <summary>
    /// Despawns a specific Powerup somewhere on the game field.
    /// </summary>
    private void DespawnPowerup(PowerupController powerup)
    {
        Destroy(powerup);
        ActivePowerups.Remove(powerup);
    }

    private void OnGameStart()
    {
        // Don't spawn immediately when the game is started.
        SpawningEnabled = false;
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
