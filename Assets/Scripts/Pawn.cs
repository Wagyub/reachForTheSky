using TMPro;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [Header("Text")] [SerializeField] private Vector3 offsetText = new(0, .2f, 0);

    [SerializeField] private Color initialTextColor;
    [Header("Outline")] [SerializeField] private Color hoverOutlineColor = Color.yellow;

    [SerializeField] private Color selectableOutlineColor = Color.saddleBrown;
    [SerializeField] private Color selectedOutlineColor = Color.lightGreen;


    [SerializeField] private float baseThickness = 1f;
    [SerializeField] private float pulseAmount = 0.5f;
    [SerializeField] private float pulseSpeed = 2f;
    private GameObject outline;
    private Turn turn;
    public string pawnName { get; set; }
    public TextMeshProUGUI nameTMP { get; set; }
    public bool canPlace { get; set; }

    public bool isHoverable { get; set; }

    public bool isSelectable { get; set; }
    public bool isSelected { get; set; }

    public bool isHovered { get; set; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        reset();
        turn = FindObjectOfType<Turn>();
        outline = transform.Find("default/outline").gameObject;
        nameTMP = transform.Find("Canvas/name").GetComponent<TextMeshProUGUI>();
        nameTMP.text = pawnName;
    }


    // Update is called once per frame
    private void Update()
    {
        outline.SetActive(isHoverable);
        if (outline.activeSelf)
        {
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

            var pulse = baseThickness + Mathf.PingPong(Time.time * pulseSpeed, pulseAmount);
            outline.GetComponent<MeshRenderer>().material.SetFloat("_Thickness", pulse);
        }
    }


    private void LateUpdate()
    {
        var mainCam = FindAnyObjectByType<Camera>();
        if (nameTMP == null || mainCam == null) return;

        // World position (pawn’s head + offset)
        var worldPos = transform.position + offsetText;

        // Convert to screen space
        var screenPos = mainCam.WorldToScreenPoint(worldPos);

        // If pawn is in front of camera
        if (screenPos.z > 0)
        {
            nameTMP.transform.position = screenPos;
            nameTMP.gameObject.SetActive(true);
            if (isSelectable)
            {
                nameTMP.outlineWidth = 0.37f;
                nameTMP.outlineColor = Color.black;
                nameTMP.color = new Color(initialTextColor.r, initialTextColor.b, initialTextColor.g, 255);
            }
            else
            {
                nameTMP.outlineColor = Color.clear;
                nameTMP.outlineWidth = 0;
                nameTMP.color = initialTextColor;
            }
        }
        else
        {
            // Behind camera → hide
            nameTMP.gameObject.SetActive(false);
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