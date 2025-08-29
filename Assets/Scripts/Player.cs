using UnityEngine;

public class Player : MonoBehaviour
{
    public Pawn pawnPrefab;
    private Grid grid;
    private Pawn pawn1;
    private Pawn pawn2;

    public bool canPlay { get; set; }
    public Pawn selectedPawn { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        grid = FindAnyObjectByType<Grid>();
        grid.CellsGenerated += OnGridCellsGenerated;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!canPlay || GameManager.Instance.turn.activePlayer != this) return;

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log(name + "termine son tour !");
            GameManager.Instance.turn.EndTurn();
        }
    }

    private void OnDestroy()
    {
        if (grid != null) grid.CellsGenerated -= OnGridCellsGenerated;
    }

    public void spawnPawn()
    {
        Debug.Log("grid.cells.Length: " + " " + grid.cells.Length);
        var randomCell1 = grid.cells[Random.Range(0, grid.width), Random.Range(0, grid.height)];
        var randomCell2 = grid.cells[Random.Range(0, grid.width), Random.Range(0, grid.height)];


        pawn1 = Instantiate(pawnPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pawn2 = Instantiate(pawnPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        pawn1.move(randomCell1);
        pawn2.move(randomCell2);
    }

    private void OnGridCellsGenerated(Grid g)
    {
        spawnPawn();
    }
}