using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;  
    public GameState currentState;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // reste entre les scènes
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                // Afficher le menu principal
                break;
            case GameState.Playing:
                // Lancer le gameplay
                break;
            case GameState.Paused:
                Time.timeScale = 0; // stoppe le temps
                break;
            case GameState.GameOver:
                // Afficher l'écran de fin
                break;
        }
    }
}
