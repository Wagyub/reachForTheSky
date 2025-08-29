using UnityEngine;

public class Pawn : MonoBehaviour
{
    public bool canPlace { get; private set; }
    public bool isHoverable { get; private set; }
    public bool isSelectable { get; private set; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void move(Cell cell)
    {
        transform.position = cell.transform.position;
    }
}