using UnityEngine;

/// <summary>
/// Contains event handlers other scripts can reference
/// to trigger an action on a certain event.
/// </summary>
public class EventManager : MonoBehaviour
{
    // Events here
    public delegate void GameStart();
    public delegate void GamePause();
    public delegate void GameResume();
    public delegate void GameEnd();

    // Event publishers here

    /// <summary>
    /// What should happen when the game is initially started?
    /// </summary>
    public static GameStart onGameStart;

    /// <summary>
    /// What should happen when the game is paused while in progress?
    /// </summary>
    public static GamePause onGamePaused;

    /// <summary>
    /// What should happen when the game is resumed after being paused?
    /// </summary>
    public static GameResume onGameResume;

    /// <summary>
    /// What should happen when the game has ended?
    /// </summary>
    public static GameEnd onGameEnd;

    /// <summary>
    /// Fires the GameStart event.
    /// </summary>
    public static void StartGame()
    {
        Debug.Log("Starting game.");

        if (onGameStart != null)
            onGameStart();
    }

    /// <summary>
    /// Fires the GamePause event.
    /// </summary>
    public static void PauseGame()
    {
        Debug.Log("Pausing game.");

        if (onGamePaused != null)
            onGamePaused();
    }

    /// <summary>
    /// Fires the GameResume event.
    /// </summary>
    public static void ResumeGame()
    {
        Debug.Log("Resuming game.");

        if (onGameResume != null)
            onGameResume();
    }

    /// <summary>
    /// Fires the GameEnd event.
    /// </summary>
    public static void EndGame()
    {
        Debug.Log("Ending game.");

        if (onGameEnd != null)
            onGameEnd();
    }
}
