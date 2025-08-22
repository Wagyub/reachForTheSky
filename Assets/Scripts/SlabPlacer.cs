using UnityEngine;
using System.Collections.Generic;

public class SlabPlacer : MonoBehaviour
{
    public GameObject slabPrefab;
    public GameObject previewPrefab;

    private GameObject previewInstance;
    private Cell hoveredCell;
    private HashSet<Cell> occupiedCells = new HashSet<Cell>();

    private Pawn currentPawn; // le pawn actif fourni par Player

    void Start()
    {
        // créer le preview une seule fois
        previewInstance = Instantiate(previewPrefab, Vector3.zero, Quaternion.identity);
        previewInstance.SetActive(false);
    }

    void Update()
    {
        if (currentPawn == null) return;
        UpdatePreview();
    }

    void UpdatePreview()
    {
        // Raycast souris
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Cell cell = hit.collider.GetComponent<Cell>();
            if (cell != null && IsAdjacentToPawn(cell))
            {
                hoveredCell = cell;
                previewInstance.SetActive(true);
                previewInstance.transform.position = cell.transform.position;

                // Couleur
                bool occupied = occupiedCells.Contains(cell);
                var renderer = previewInstance.GetComponent<Renderer>();
                renderer.material.color = occupied ? Color.red : new Color(0, 1, 0, 0.5f);
            }
            else
            {
                hoveredCell = null;
                previewInstance.SetActive(false);
            }
        }
        else
        {
            hoveredCell = null;
            previewInstance.SetActive(false);
        }
    }

    public void TryPlaceSlab()
    {
        if (hoveredCell == null) return;
        if (occupiedCells.Contains(hoveredCell)) return;

        Instantiate(slabPrefab, hoveredCell.transform.position, Quaternion.identity);
        occupiedCells.Add(hoveredCell);
    }

    public void SetActivePawn(Pawn pawn)
    {
        currentPawn = pawn;
    }

    private bool IsAdjacentToPawn(Cell cell)
    {
        // trouver la cellule où est le pawn actif
        Cell pawnCell = FindClosestCellToPawn(currentPawn);
        if (pawnCell == null) return false;

        int dx = Mathf.Abs(pawnCell.X - cell.X);
        int dy = Mathf.Abs(pawnCell.Y - cell.Y);

        // autorise orthogonal uniquement
        return (dx + dy == 1);
    }

    private Cell FindClosestCellToPawn(Pawn pawn)
    {
        // recherche brute : le plus proche Cell dans la grille
        Cell[] allCells = FindObjectsOfType<Cell>();
        Cell closest = null;
        float minDist = float.MaxValue;
        foreach (var c in allCells)
        {
            float dist = Vector3.Distance(c.transform.position, pawn.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = c;
            }
        }
        return closest;
    }
}
