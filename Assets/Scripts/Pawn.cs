using UnityEngine;

public class Pawn : MonoBehaviour
{
    private GameObject outline;
    public bool canPlace { get; set; }

    public bool isHoverable { get; set; }

    public bool isSelectable { get; set; }
    public bool isSelected { get; set; }
    
    public bool isHovered { get; set; }
    
    [Header("Outline")]
    [SerializeField] private Color hoverOutlineColor = Color.yellow;
    [SerializeField] private Color selectableOutlineColor = Color.saddleBrown;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        reset();
        outline = transform.Find("default/outline").gameObject;
    }

    // Update is called once per frame
    private void Update()
    {
        outline.SetActive(isHoverable);
        if (!isHovered && isHoverable)
        {
            this.outline.GetComponent<MeshRenderer>().material.color = selectableOutlineColor;
        }
    }

    private void OnMouseEnter()
    {
        this.outline.GetComponent<MeshRenderer>().material.color = hoverOutlineColor;
        isHovered=true;
    }

    private void OnMouseExit()
    {
        isHovered=false;
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