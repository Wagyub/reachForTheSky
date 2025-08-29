using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Phase
{
    IDLE,
    MOVING,
    CONSTRUCTING
}

public class Turn : MonoBehaviour
{
    public Phase phase;
    public int number;
    private int activePlayerIndex;
    private List<Player> players;
    public Player activePlayer { get; private set; }

    private void Start()
    {
        number = 0;
        phase = Phase.IDLE;
    }

    public void StartTurn(Player startingPlayer, Player[] allPlayers)
    {
        players = new List<Player>(allPlayers);
        activePlayerIndex = players.IndexOf(startingPlayer);
        SetActivePlayer(startingPlayer);
    }

    public void EndTurn()
    {
        number++;

        activePlayer.canPlay = false;

        activePlayerIndex = (activePlayerIndex + 1) % players.Count;
        StartCoroutine(ActivateNextPlayer());
    }


    private IEnumerator ActivateNextPlayer()
    {
        // attendre la fin de frame → évite que l'input de la souris "pollue"
        yield return null;

        SetActivePlayer(players[activePlayerIndex]);
    }

    private void SetActivePlayer(Player player)
    {
        activePlayer = player;
        activePlayer.canPlay = true;
        var otherPlayer = players[(activePlayerIndex + 1) % players.Count];
        foreach (var p in activePlayer.pawns) p.isHoverable = true;
        foreach (var p in otherPlayer.pawns) p.isHoverable = false;
        Debug.Log("C'est au joueur " + activePlayer.name + " de jouer !");
    }

    public void nextTurn()
    {
        number++;
    }
}