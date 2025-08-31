using System.Collections;
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
    public Turn turn;
    public Player[] players;
    public Player startingPlayer;
    public Grid grid;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // reste entre les scènes
        turn = FindAnyObjectByType<Turn>();
        grid = FindAnyObjectByType<Grid>();
        grid.CellsGenerated += OnGridCellsGenerated;
        setState(GameState.Playing);
    }

    private async void OnGridCellsGenerated(Grid g)
    {
        players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        StartCoroutine(SetupAndStart());
    }

    private IEnumerator SetupAndStart()
    {
        foreach (var player in players)
            player.spawnPawn();


        // Attendre la fin de la frame pour laisser s’exécuter les Start() des pions
        yield return null; // ou: yield return new WaitForEndOfFrame();

        startingPlayer = players[0];
        turn.StartTurn(startingPlayer, players);
    }


    public void setState(GameState newState)
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