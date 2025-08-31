using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Phase
{
    IDLE,
    SELECTING,
    MOVING,
    CONSTRUCTING
}

public class Turn : MonoBehaviour
{
    public Phase phase;
    public int number;
    public Grid grid;
    private int activePlayerIndex;
    private List<Player> players;
    public Player activePlayer { get; private set; }

    private void Start()
    {
        number = 0;
        phase = Phase.IDLE;
        grid = FindAnyObjectByType<Grid>();
    }

    public void StartTurn(Player startingPlayer, Player[] allPlayers)
    {
        phase = Phase.SELECTING;
        players = new List<Player>(allPlayers);
        activePlayerIndex = players.IndexOf(startingPlayer);
        SetActivePlayer(startingPlayer);
    }

    public void CheckIfMovingPhase(Pawn selectedPawn = null)
    {
        // Réinitialiser l'état des cellules
        foreach (var c in grid.cells) c.resetCell();

        if (phase != Phase.MOVING || activePlayer == null) return;

        // Déterminer quels pions traiter
        IEnumerable<Pawn> pawnsToProcess;
        if (selectedPawn != null)
        {
            // Ne traite le pion sélectionné que s'il appartient bien au joueur actif
            var belongsToActive = activePlayer.pawns != null && Array.IndexOf(activePlayer.pawns, selectedPawn) >= 0;
            if (!belongsToActive) return; // pion invalide → ne rien faire
            pawnsToProcess = new[] { selectedPawn };
        }
        else
        {
            pawnsToProcess = activePlayer.pawns;
        }

        // Pour chaque pion ciblé, trouver sa cellule, puis marquer les cellules adjacentes
        foreach (var activePlayerPawn in pawnsToProcess)
        {
            Cell pawnCell = null;

            // Trouver la cellule sur laquelle se trouve le pion (comparaison 2D: x/y)
            foreach (var cell in grid.cells)
                if (getIfPawnInCellFromPawnPosition(activePlayerPawn.transform.position, cell.transform.position))
                {
                    pawnCell = cell;
                    break;
                }

            if (pawnCell == null) continue;

            // Marquer les 8 cellules voisines (rayon 1) comme disponibles si non occupées
            for (var dy = -1; dy <= 1; dy++)
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue; // ignorer la cellule du pion
                var nx = pawnCell.X + dx;
                var ny = pawnCell.Y + dy;
                if (nx < 0 || ny < 0 || nx >= grid.width || ny >= grid.height) continue;

                var neighbor = grid.cells[nx, ny];

                // Vérifie si un pion (allié ou adverse) occupe la cellule neighbor
                var occupied = false;
                foreach (var player in players)
                {
                    foreach (var pawn in player.pawns)
                        if (getIfPawnInCellFromPawnPosition(pawn.transform.position, neighbor.transform.position))
                        {
                            occupied = true;
                            break;
                        }

                    if (occupied) break;
                }

                // Indisponible si niveau > (level pawnCell + 2)
                if (!occupied && neighbor.level <= pawnCell.level + 2)
                    neighbor.isAvaiableMovingCell = true;
            }
        }
    }

    public void chooseConstructCell(Cell selectedCell)
    {
        var otherPawn = activePlayer.pawns.First(p => p != activePlayer.selectedPawn);
        otherPawn.isSelected = false;
        otherPawn.canPlace = false;
        otherPawn.isHoverable = false;
        otherPawn.isSelectable = false;
        otherPawn.isHovered = false;
        phase = Phase.CONSTRUCTING;
        CheckIfConstructingPhase(selectedCell);
    }

    public void CheckIfConstructingPhase(Cell selectedCell)
    {
        // Reset all cells
        foreach (var c in grid.cells) c.resetCell();

        if (phase != Phase.CONSTRUCTING || activePlayer == null || selectedCell == null) return;

        // Check all 8 neighbors
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
        {
            if (dx == 0 && dy == 0) continue; // skip the selected cell itself

            var nx = selectedCell.X + dx;
            var ny = selectedCell.Y + dy;

            // Skip out-of-bounds
            if (nx < 0 || ny < 0 || nx >= grid.width || ny >= grid.height) continue;

            var neighbor = grid.cells[nx, ny];

            // Skip if level >= 3
            if (neighbor.level >= 3) continue;

            // Must have same level as selected cell
            if (neighbor.level != selectedCell.level) continue;

            // Check if any pawn is occupying the neighbor
            var occupied = false;
            foreach (var player in players)
            {
                foreach (var pawn in player.pawns)
                    if (getIfPawnInCellFromPawnPosition(pawn.transform.position, neighbor.transform.position))
                    {
                        occupied = true;
                        break;
                    }

                if (occupied) break;
            }

            if (!occupied) neighbor.isAvaiableConstructingCell = true;
        }
    }


    public bool getIfPawnInCellFromPawnPosition(Vector3 pawnPosition, Vector3 cellPosition)
    {
        // Comparaison sur X/Y uniquement, car Z est décalé pour les pions
        var p2 = new Vector2(pawnPosition.x, pawnPosition.y);
        var c2 = new Vector2(cellPosition.x, cellPosition.y);
        return Vector2.Distance(p2, c2) < 0.51f;
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
        phase = Phase.SELECTING;
        activePlayer = player;
        activePlayer.canPlay = true;
        var otherPlayer = players[(activePlayerIndex + 1) % players.Count];
        foreach (var p in activePlayer.pawns)
        {
            p.isSelectable = true;
            p.isHoverable = true;
            p.canPlace = false;
        }

        foreach (var p in otherPlayer.pawns)
        {
            p.isSelectable = false;
            p.isHoverable = false;
            p.canPlace = false;
            p.isSelected = false;
        }

        Debug.Log("C'est au joueur " + activePlayer.name + " de jouer !");
        CheckIfMovingPhase();
    }

    public void SelectPawnPhase(Pawn pawn)
    {
        phase = Phase.MOVING;
        var otherPawn = activePlayer.pawns.First(p => p != pawn);
        otherPawn.isSelected = false;
        activePlayer.selectedPawn = pawn;
        CheckIfMovingPhase(pawn);
    }

    public void nextTurn()
    {
        number++;
    }
}