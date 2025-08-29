using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Grid : MonoBehaviour
{
    [Header("Dimensions")]
    [Min(1)] public int width = 10;
    [Min(1)] public int height = 10;

    [Header("Organisation")]
    public string containerName = "Cells";
    public bool regenerateOnStart = true;

    [Header("Origine de la grille")]
    public Vector2 originOffset = Vector2.zero;

    [Header("Prefab Cellule")]
    public Cell cellPrefab; // Assigne ici ton prefab "Cell"

    [Header("Caméra")]
    public Camera targetCamera;                 // Assigne la caméra dans l’inspector
    public Vector3 cameraOffset = new Vector3(0f, 0f, -8f); // Décalage en Z pour être devant la scène
    
    [Range(-45f, 45f)] public float xTilt = 10f;             // Inclinaison légère autour de Z
    public bool positionCameraOnStart = true;
    public event Action<Grid> CellsGenerated;

    public Cell[,] cells { get; private set; }

    [Header("Prefab Dalle")]
    public GameObject dallePrefab;



    // ... existing code ...
    void Start()
    {
        if (regenerateOnStart)
        {
            GenerateCells();
            CellsGenerated?.Invoke(this);
        }

        if (positionCameraOnStart)
        {
            PositionCamera();
        }

        var raycaster = targetCamera.GetComponent<PhysicsRaycaster>();
        if (raycaster != null)
        {
            int dalleLayer = LayerMask.NameToLayer("Dalle");
            if (dalleLayer != -1)
                raycaster.eventMask &= ~(1 << dalleLayer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    // Place la caméra au centre de la grille avec une légère inclinaison en Z
    public void PositionCamera()
    {
        if (targetCamera == null) return;

        // Centre monde de la grille (basé sur width/height et l’originOffset)
        Vector3 origin = transform.position + (Vector3)originOffset;
        Vector3 center = origin + new Vector3(width * 0.5f, height * 0.5f, 0f);

        // Position et rotation (roll en Z)
        targetCamera.transform.position = center + cameraOffset;
        targetCamera.transform.rotation = Quaternion.Euler(-xTilt, 0f, 0f);
    }

    // Crée (ou recrée) toutes les cellules comme enfants du conteneur
     public void GenerateCells()
    {
        if (width <= 0 || height <= 0) return;
        if (cellPrefab == null)
        {
            Debug.LogWarning("Grid: aucun 'cellPrefab' assigné. Assigne un prefab de type 'Cell'.", this);
            return;
        }

        // (Re)crée le conteneur
        Transform container = transform.Find(containerName);
        if (container != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                while (container.childCount > 0) DestroyImmediate(container.GetChild(0).gameObject);
            else
                while (container.childCount > 0) Destroy(container.GetChild(0).gameObject);
#else
            while (container.childCount > 0) Destroy(container.GetChild(0).gameObject);
#endif
        }
        else
        {
            GameObject c = new GameObject(containerName);
            c.transform.SetParent(transform);
            c.transform.localPosition = Vector3.zero;
            c.transform.localRotation = Quaternion.identity;
            c.transform.localScale = Vector3.one;
            container = c.transform;
        }

        // Base d'origine: position du GameObject + offset personnalisable
        Vector3 origin = transform.position + (Vector3)originOffset;
        cells = new Cell[width, height];


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Instancie le prefab Cell comme enfant du conteneur
                Cell cellInstance;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    var go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(cellPrefab.gameObject, container);
                    cellInstance = go.GetComponent<Cell>();
                }
                else
                {
                    cellInstance = Instantiate(cellPrefab, container);
                }
#else
                cellInstance = Instantiate(cellPrefab, container);
#endif
                cellInstance.name = $"Cell {x},{y}";

                // Initialise la cellule; elle gère sa position/visuel/échelle
                cellInstance.Initialize(this, x, y);
                cells[x, y] = cellInstance;
            }
        }
    }
}