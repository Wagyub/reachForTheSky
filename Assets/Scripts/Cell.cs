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
    public int level { get; set; }

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
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
    {
        if (hit.collider.gameObject != this.gameObject)
            return; // on a cliqué sur autre chose qu'une cellule
    }

    if (Grid == null || Grid.dallePrefab == null)
    {
        Debug.LogWarning("Grid ou prefab Dalle manquant !");
        return;
    }

    placeDalle();
    }

    private void OnMouseEnter()
    {
        outerMeshRenderer.material.color = Color.yellow;
    }

    private void OnMouseExit()
    {
        outerMeshRenderer.material.color = initalOuterColor;
    }

    public void placeDalle()
    {
        if (level >= 4)
        {
            Debug.Log("Impossible de jouer ici !");
            return;
        }
        if (Grid.dallePrefab == null) return;

        Vector3 spawnPosition = transform.position + new Vector3(0f, 0, -0.2f-0.2f*level);
        GameObject newDalle = Instantiate(Grid.dallePrefab, spawnPosition, Quaternion.identity);
        newDalle.layer = LayerMask.NameToLayer("Dalle"); 
        newDalle.AddComponent<Dalle>();

        level++;

        Debug.Log($"La cellule ({X}, {Y}) est au niveau {level}");

        Transform inner = newDalle.transform.Find("InnerCube");
        if (inner != null)
        {
            MeshRenderer rend = inner.GetComponent<MeshRenderer>();
            if (rend != null && rend.materials.Length > 0)
            {
                switch (level)
                {
                    case 1: rend.materials[0].color = Color.yellow; break;
                    case 2: rend.materials[0].color = new Color(1f, 0.5f, 0f); break; // orange
                    case 3: rend.materials[0].color = Color.red; break;
                    case 4: rend.materials[0].color = Color.green; break;
                    default: rend.materials[0].color = Color.gray; break;
                }
            }
        }

        if (level >= 4)
        {
            Debug.Log($"Le joueur {GameManager.Instance.turn.activePlayer} a gagné !");
        }
    }
}