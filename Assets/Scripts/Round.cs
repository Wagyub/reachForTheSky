using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum Phase
{
    IDLE,
    MOVING,
    CONSTRUCTING
}
public class Round : MonoBehaviour
{
    public Phase phase;
    public int number = 0;
    public Player activePlayer { get; private set; }
    private List<Player> players;
    private int activePlayerIndex = 0;

    private void Start()
    {
        this.number = 0;
        this.phase = Phase.IDLE;
    }

    public void StartRound(Player startingPlayer, Player[] allPlayers)
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

    
    private System.Collections.IEnumerator ActivateNextPlayer()
    {
        // attendre la fin de frame → évite que l'input de la souris "pollue"
        yield return null;

        SetActivePlayer(players[activePlayerIndex]);
    }

    private void SetActivePlayer(Player player)
    {
        activePlayer = player;
        activePlayer.canPlay = true;

        Debug.Log("C'est au joueur " + player.name + " de jouer !");
    }

    public void nextRound()
    {
        this.number++;
    }
    
}

