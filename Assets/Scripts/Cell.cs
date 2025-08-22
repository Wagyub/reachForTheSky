using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [Header("Placement")]
    public float cellSize = 1f;
    public bool placeAtCellCenter = true; // centre (x+0.5f,y+0.5f) ou coin (x,y)

    [Header("Runtime (lecture seule)")]
    public Grid Grid { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    
    public bool isPlayerOn { get; set; }
    
    public MeshRenderer outerMeshRenderer;
    private Color initalOuterColor;
    public int level {get; set;}

    // Appelée par Grid juste après la création
    public void Initialize(Grid grid, int x, int y)
    {
        Grid = grid;
        X = x;
        Y = y;
        var t = 
        outerMeshRenderer = transform.Find("OuterCube").GetComponent<MeshRenderer>();
        initalOuterColor = outerMeshRenderer.material.color;
        PositionSelf();
        // Aucune modification d'échelle du prefab !
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Permet d’apercevoir les changements en Éditeur
        if (cellSize <= 0f) cellSize = 0.001f;
        if (Grid != null)
        {
            PositionSelf();
        }
    }
#endif

    private void PositionSelf()
    {
        if (Grid == null) return;

        // Origine = position du Grid + offset défini dans Grid
        Vector3 origin = Grid.transform.position + (Vector3)Grid.originOffset;

        float ox = placeAtCellCenter ? (X + 0.5f) : X;
        float oy = placeAtCellCenter ? (Y + 0.5f) : Y;

        Vector3 target = origin + new Vector3(ox * cellSize, oy * cellSize, 0f);

        // On place le GameObject de la Cell et on conserve sa composante Z existante
        Vector3 p = transform.position;
        p.x = target.x;
        p.y = target.y;
        transform.position = p;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // Optionnel: interaction simple
    private void OnMouseDown()
    {
        Debug.Log($"Cell clicked: ({X},{Y})", this);
    }

    private void OnMouseEnter()
    {
        outerMeshRenderer.material.color = Color.yellow;
    }
    
    private void OnMouseExit()
    {
        outerMeshRenderer.material.color = initalOuterColor;   
    }
}