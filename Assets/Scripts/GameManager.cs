using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enforces the rules of the game and holds game state.
/// 
/// A singleton; holds references to player objects (and other stuff).
/// </summary>
[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Is the game currently paused?
    /// </summary>
    private bool IsPaused;

    /// <summary>
    /// Has the game ended?
    /// </summary>
    private bool IsEnded;

    /// <summary>
    /// The reference to the player ship.
    /// </summary>
    [SerializeField] public CubeShipPilot PlayerShip;

    /// <summary>
    /// The reference to the asteroid spawner/despawner.
    /// </summary>
    [SerializeField] public AsteroidManager AsteroidSpawner;

    /// <summary>
    /// Reference to the main followcam script.
    /// </summary>
    [SerializeField] public FollowCamera FollowCam;

    /// <summary>
    /// The reference to the main menu.
    /// </summary>
    [SerializeField] public Canvas MainMenu;

    /// <summary>
    /// The reference to the end game menu.
    /// </summary>
    [SerializeField] public Canvas EndGameMenu;

    /// <summary>
    /// The reference to the shield UI overlay.
    /// </summary>
    [SerializeField] public Canvas ShieldUI;

    /// <summary>
    /// The reference to the pause game menu.
    /// </summary>
    [SerializeField] public Canvas PauseGameMenu;

    /// <summary>
    /// What is the player's current score?
    /// </summary>
    public uint Score;
    [SerializeField] public uint ScorePerQuarterSec = 5;

    /// <summary>
    /// How many seconds have we been playing?
    /// </summary>
    [SerializeField] public uint ElapsedTimeSecs;

    /// <summary>
    /// (Singleton instance)
    /// </summary>
    private static GameManager _instance;

    /// <summary>
    /// Reference to singleton instance of GameManger
    /// </summary>
    public static GameManager Instance {get { return _instance; }}

    /// <summary>
    /// Register event handlers
    /// </summary>
    void OnEnable()
    {
        EventManager.onGameStart += OnGameStart;
        EventManager.onGamePaused += OnGamePaused;
        EventManager.onGameResume += OnGameResume;
        EventManager.onGameEnd += OnGameEnd;

        // Set initial game state on main menu
        IsPaused = true;
        MainMenu.gameObject.SetActive(true);
        EndGameMenu.gameObject.SetActive(false);
        PauseGameMenu.gameObject.SetActive(false);
        ShieldUI.gameObject.SetActive(false);
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
    /// (Needed because Unity button OnClick() doesn't accept
    /// static methods)
    /// </summary>
    public void FireStartGameEvent()
    {
        EventManager.StartGame();
    }

    /// <summary>
    /// Restarts the entire game.
    /// </summary>
    public void ReloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        // Create the instance if it doesn't exist,
        // and destroy the previous instance if it's not ours.
        if (_instance == null)
        {
            _instance = this;
            InvokeRepeating("IncrementScorePerQuarterSec", 0.25f, 0.25f);  // invoke at t = 0.25 sec and every 0.25 sec afterwards
            InvokeRepeating("IncrementElapsedTimePerSec", 1, 1);
        }
            
        else if (_instance != this)
            Destroy(this.gameObject);
    }

    void Update()
    {
        // Should game be paused/resumed?
        if (Input.GetKeyDown(KeyCode.Escape) && !IsPaused && !IsEnded)
            EventManager.PauseGame();
        else if (Input.GetKeyDown(KeyCode.Escape) && IsPaused && !IsEnded)
            EventManager.ResumeGame();
        
    }

    /// <summary>
    /// Increments the current score each quarter of a second.
    /// 
    /// Also, modify score per quarter second based on speed of player ship.
    /// </summary>
    private void IncrementScorePerQuarterSec()
    {
        if (IsPaused || IsEnded)
            return;

        Score += ScorePerQuarterSec;
        ScorePerQuarterSec = 5 + (uint) Mathf.RoundToInt(0.1f * PlayerShip.ShipSpeed);
    }

    /// <summary>
    /// Increments the timer.
    /// </summary>
    private void IncrementElapsedTimePerSec()
    {
        if (IsPaused || IsEnded)
            return;
        
        ElapsedTimeSecs += 1;
    }

    // Event handlers below
    private void OnGameStart()
    {
        IsPaused = false;

        // And remove main menu
        MainMenu.gameObject.SetActive(false);

        // And display shield overlay
        ShieldUI.gameObject.SetActive(true);
    }

    private void OnGamePaused()
    {
        IsPaused = true;
        PauseGameMenu.gameObject.SetActive(true);
    }

    private void OnGameResume()
    {
        IsPaused = false;
        PauseGameMenu.gameObject.SetActive(false);
    }

    private void OnGameEnd()
    {
        IsPaused = true;
        IsEnded = true;

        // And hide shield overlay
        ShieldUI.gameObject.SetActive(false);

        // And display end menu
        EndGameMenu.gameObject.SetActive(true);
    }
}
