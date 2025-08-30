using UnityEngine;

public class Cell : MonoBehaviour
{
    [Header("Placement")] public float cellSize = 1f;

    public bool placeAtCellCenter = true; // centre (x+0.5f,y+0.5f) ou coin (x,y)

    public MeshRenderer outerMeshRenderer;
    public MeshRenderer innerMeshRenderer;
    private GameManager _gameManager;
    private Color initialInnerColor;
    private Color initialOuterColor;

    [Header("Runtime (lecture seule)")] public Grid Grid { get; private set; }

    public int X { get; private set; }
    public int Y { get; private set; }
    public bool isPlayerOn { get; set; }
    public bool isHoverable { get; set; }
    public bool isHovered { get; set; }
    public bool isAvaiableMovingCell { get; set; }
    public int level { get; set; }

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }


    private void Update()
    {
        isHoverable = !isPlayerOn;
        initialOuterColor = new Color(.12f, .12f, .12f);
        if (isAvaiableMovingCell)
            if (_gameManager.turn.phase == Phase.MOVING)
                initialInnerColor = Color.darkGreen;
            else initialInnerColor = new Color(0, 0.150f, 0, 255);

        else
            initialInnerColor = new Color(0, 0, 0, 255);

        if (isHovered)
        {
            if (isAvaiableMovingCell && _gameManager.turn.phase == Phase.MOVING)
            {
                innerMeshRenderer.material.color = Color.green;
                outerMeshRenderer.material.color = Color.greenYellow;
            }
            else
            {
                outerMeshRenderer.material.color = Color.yellow;
            }
        }
        else
        {
            outerMeshRenderer.material.color = initialOuterColor;
            innerMeshRenderer.material.color = initialInnerColor;
        }
    }

    // Optionnel: interaction simple
    private void OnMouseDown()
    {
        switch (_gameManager.turn.phase)
        {
            case Phase.MOVING:
                if (isAvaiableMovingCell)
                {
                    _gameManager.turn.activePlayer.selectedPawn.move(this);
                    _gameManager.turn.EndTurn();
                }

                break;
            default:
                return;
        }
    }


    private void OnMouseEnter()
    {
        isHovered = isHoverable;
    }

    private void OnMouseExit()
    {
        isHovered = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Permet d’apercevoir les changements en Éditeur
        if (cellSize <= 0f) cellSize = 0.001f;
        if (Grid != null) PositionSelf();
    }
#endif

    // Appelée par Grid juste après la création
    public void Initialize(Grid grid, int x, int y)
    {
        Grid = grid;
        X = x;
        Y = y;
        outerMeshRenderer = transform.Find("OuterCube").GetComponent<MeshRenderer>();
        innerMeshRenderer = transform.Find("InnerCube").GetComponent<MeshRenderer>();
        initialOuterColor = outerMeshRenderer.material.color;
        initialInnerColor = innerMeshRenderer.material.color;
        PositionSelf();

        // Aucune modification d'échelle du prefab !
    }

    private void PositionSelf()
    {
        if (Grid == null) return;

        // Origine = position du Grid + offset défini dans Grid
        var origin = Grid.transform.position + (Vector3)Grid.originOffset;

        var ox = placeAtCellCenter ? X + 0.5f : X;
        var oy = placeAtCellCenter ? Y + 0.5f : Y;

        var target = origin + new Vector3(ox * cellSize, oy * cellSize, 0f);

        // On place le GameObject de la Cell et on conserve sa composante Z existante
        var p = transform.position;
        p.x = target.x;
        p.y = target.y;
        transform.position = p;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    public void resetCell()
    {
        isAvaiableMovingCell = false;
    }
}