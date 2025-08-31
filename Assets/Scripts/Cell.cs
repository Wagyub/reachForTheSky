using UnityEngine;

public class Cell : MonoBehaviour
{
    [Header("Placement")] public float cellSize = 1f;

    public bool placeAtCellCenter = true; // centre (x+0.5f,y+0.5f) ou coin (x,y)

    public MeshRenderer outerMeshRenderer;
    public MeshRenderer innerMeshRenderer;
    public Color initialInnerColor;
    public Color initialOuterColor;
    public Color avaiableMovingCellColor;
    public Color avaiableConstructingCellColor;
    private GameManager _gameManager;

    [Header("Runtime (lecture seule)")] public Grid Grid { get; private set; }

    public int X { get; private set; }
    public int Y { get; private set; }
    public bool isPlayerOn { get; set; }
    public bool isHoverable { get; set; }
    public bool isHovered { get; set; }
    public bool isAvaiableMovingCell { get; set; }
    public bool isAvaiableConstructingCell { get; set; }
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
                initialInnerColor = avaiableMovingCellColor;
            else initialInnerColor = new Color(0, 0.150f, 0, 255);

        else if (isAvaiableConstructingCell) initialInnerColor = avaiableConstructingCellColor;
        else
            initialInnerColor = new Color(0, 0, 0, 255);


        if (isHovered)
        {
            if (isAvaiableMovingCell && _gameManager.turn.phase == Phase.MOVING)
                outerMeshRenderer.material.color = Color.greenYellow;
            else if (isAvaiableConstructingCell)
                outerMeshRenderer.material.color = Color.cyan;
            else
                outerMeshRenderer.material.color = Color.yellow;
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
                    _gameManager.turn.chooseConstructCell(this);
                }

                break;
            case Phase.CONSTRUCTING:

                if (isAvaiableConstructingCell)
                    switch (level)
                    {
                        case 0:
                        case 1:
                        case 2:
                            var potentialLevel = level + 1;
                            var slabPrefab = Resources.Load<GameObject>("Prefabs/Slab_" + potentialLevel);
                            if (slabPrefab != null)
                            {
                                var createdSLab = Instantiate(slabPrefab, new Vector3(), new Quaternion(), transform);
                                var spawnPos = new Vector3(-0.79f, -(0.5f + level * 1.8f), -0.13f);
                                var spawnRot = Quaternion.Euler(-180f, 0f, 0f);
                                createdSLab.transform.localScale = new Vector3(0.84f, 6f, 1.3f);
                                createdSLab.transform.localPosition = spawnPos;
                                createdSLab.transform.localRotation = spawnRot;
                                level++;
                            }
                            else
                            {
                                Debug.LogError("Slab_1 prefab not found in Resources/Prefabs!");
                            }

                            break;
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
        isAvaiableConstructingCell = false;
    }
}