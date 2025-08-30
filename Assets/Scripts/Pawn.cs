using UnityEngine;

public class Pawn : MonoBehaviour
{
    [Header("Outline")] [SerializeField] private Color hoverOutlineColor = Color.yellow;

    [SerializeField] private Color selectableOutlineColor = Color.saddleBrown;
    [SerializeField] private Color selectedOutlineColor = Color.lightGreen;
    private GameObject outline;
    private Turn turn;
    public bool canPlace { get; set; }

    public bool isHoverable { get; set; }

    public bool isSelectable { get; set; }
    public bool isSelected { get; set; }

    public bool isHovered { get; set; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        reset();
        outline = transform.Find("default/outline").gameObject;
        turn = FindObjectOfType<Turn>();
    }

    // Update is called once per frame
    private void Update()
    {
        outline.SetActive(isHoverable);
        if (isSelected)
        {
            outline.GetComponent<MeshRenderer>().material.color = selectedOutlineColor;
        }
        else
        {
            if (isHovered)
                outline.GetComponent<MeshRenderer>().material.color = hoverOutlineColor;
            else
                outline.GetComponent<MeshRenderer>().material.color = selectableOutlineColor;
        }
    }

    private void OnMouseDown()
    {
        switch (turn.phase)
        {
            case Phase.SELECTING:
            case Phase.MOVING:
                if (isSelectable) turn.SelectPawnPhase(this);
                break;
        }
    }

    private void OnMouseEnter()
    {
        isHovered = true;
    }

    private void OnMouseExit()
    {
        isHovered = false;
    }

    private void reset()
    {
        canPlace = false;
        isHoverable = false;
        isSelectable = false;
        isSelected = false;
    }


    public void move(Cell cell)
    {
        transform.position = cell.transform.position + new Vector3(0, 0f, -(0.7f + cell.level));
    }
}