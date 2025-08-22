using UnityEngine;

public class Player : MonoBehaviour
{
    Grid grid;

    public Pawn pawnPrefab;
    Pawn pawn1;
    Pawn pawn2;

    public void spawnPawn()
    {
        Debug.Log("grid.cells.Length: "+" "+this.grid.cells.Length);
        Cell randomCell1 = this.grid.cells[Random.Range(0, this.grid.width), Random.Range(0, this.grid.height)];
        Cell randomCell2 = this.grid.cells[Random.Range(0, this.grid.width), Random.Range(0, this.grid.height)];

        

        pawn1 = Instantiate(pawnPrefab, new Vector3(0,0,0), Quaternion.identity);
        pawn2 = Instantiate(pawnPrefab, new Vector3(0,0,0), Quaternion.identity);
        
        pawn1.move(randomCell1);
        pawn2.move(randomCell2);
        

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.grid = FindAnyObjectByType<Grid>();
        grid.CellsGenerated += OnGridCellsGenerated;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnDestroy()
    {
        if (grid != null)
        {
            grid.CellsGenerated -= OnGridCellsGenerated;
        }
    }

    private void OnGridCellsGenerated(Grid g)
    {
            spawnPawn();
    }

}
