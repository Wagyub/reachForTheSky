using UnityEngine;
using UnityEngine.InputSystem; // nouveau Input System
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    Grid grid;

    public Pawn pawnPrefab;
    private List<Pawn> pawns = new List<Pawn>();
    private int activePawnIndex = 0;

    private Pawn ActivePawn => pawns[activePawnIndex];

    private SlabPlacer placer;

    public void spawnPawn()
    {
        var positionPawn1 = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.height), 0);
        var positionPawn2 = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.height), 0);

        var p1 = Instantiate(pawnPrefab, positionPawn1, Quaternion.identity);
        var p2 = Instantiate(pawnPrefab, positionPawn2, Quaternion.identity);

        pawns.Add(p1);
        pawns.Add(p2);
    }

    void Start()
    {
        this.grid = FindAnyObjectByType<Grid>();
        this.placer = FindAnyObjectByType<SlabPlacer>(); // auto-détection
        spawnPawn();
    }

    void Update()
    {
        if (placer == null || pawns.Count == 0) return;

        // clic gauche → tenter de placer slab avec le pawn actif
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ActivePawn.PlaceSlab(placer);

            // passer au pawn suivant
            activePawnIndex = (activePawnIndex + 1) % pawns.Count;
        }
    }
}
