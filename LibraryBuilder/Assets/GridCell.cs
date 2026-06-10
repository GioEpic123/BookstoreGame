using System;
using UnityEngine.EventSystems;
using UnityEngine;

public class GridCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    /// <summary>
    ///  Grid Cell - A physical space within the Buildgrid. 
    /// Knows it's position, and what object it's holding. 
    /// Can be naturally obstructed to prevent building
    /// </summary>


    /// Grid Cell
    public GameObject gridGraphics;
    public Vector2Int gridPos; // Position within the grid
    public GridCellColor currentColor;
    public GridManager gridManager;

    // Logic
    BoxCollider clickDetection;
    public bool isNaturallyObstructed = false; // prevents cell interaction
    public bool isObstructed = false; // prevents cell interaction

    // Grid Item
    public GameObject attachedObject;
    public BuildOption attachedObjectBuildOption;
    public BuildModeDirection attachedObjectDirection;


    void Awake() {
        currentColor = GridCellColor.Grey;
        clickDetection = GetComponent<BoxCollider>();
        attachedObjectBuildOption = BuildOption.None;
    }
    // When Build Mode starts, show graphics & set color
    // - Grey: Avaliable
    // - Red: Naturally obstructed
    public void BuildModeActive(Boolean active) {
        gridGraphics.SetActive(active);
        clickDetection.enabled = active && !isObstructed;
        if (isNaturallyObstructed || isObstructed) {
            ChangeColor(GridCellColor.Red);
        }
        else {
            ChangeColor(GridCellColor.Grey);

        }
    }

    public void SetObstructed(bool obstructed) {
        isObstructed = obstructed;
        //gridGraphics.SetActive(!obstructed);
        //clickDetection.enabled = !obstructed;
    }
    // Mouse Activity - forward to GridManager for operation, apply response color
    public void OnPointerClick(PointerEventData eventData) {
        log("..Clicked..");
        gridManager.CellWasClicked(this);
    }
    public void OnPointerEnter(PointerEventData eventData) {
        log("Hovering...");
        gridManager.CellWasEntered(this);
    }
    public void OnPointerExit(PointerEventData eventData) {
        log("...stopped Hovering.");
        gridManager.CellWasExited();
    }

    public void ChangeColor(GridCellColor color) {
        currentColor = color;
        gridGraphics.GetComponent<Renderer>().material = BuildModeHelpers.GetMaterialForGridCellColor(color);
    }

    void log(String message) {
        Debug.Log($"Cell ({gridPos[0]}, {gridPos[1]}): {message}");
    }
}
