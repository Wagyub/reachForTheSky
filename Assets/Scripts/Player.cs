using UnityEngine;

public class Player : MonoBehaviour
{
    Grid grid;

    public Pawn pawnPrefab;
    Pawn pawn1;
    Pawn pawn2;

    public void spawnPawn()
    {
        var positionPawn1 = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.height), 0);
        var positionPawn2 = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.height), 0);
        
        pawn1 = Instantiate(pawnPrefab, positionPawn1, Quaternion.identity);
        pawn2 = Instantiate(pawnPrefab, positionPawn2, Quaternion.identity);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.grid = FindAnyObjectByType<Grid>();
        spawnPawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
