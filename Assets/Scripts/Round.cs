using System;
using System.Collections.Generic;
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
    public Player activePlayer { get; set; }

    private void Start()
    {
        this.number = 0;
        this.phase = Phase.IDLE;
    }
    

    public void nextRound()
    {
        this.number++;  
    }
    
}

