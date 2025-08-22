using UnityEngine;

public class Pawn : MonoBehaviour
{
    public bool canPlay { get; private set; }
    public void move(Cell cell)
    {
        transform.position = cell.transform.position;
    }



    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
