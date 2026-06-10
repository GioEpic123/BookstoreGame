using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class BuildGrid : MonoBehaviour {
    // Player - Interactable grid
    public GridCell[][] mainGrid;
    int gridSize = 5; // LxW. Default to 5, changes on make    
    GameObject cellPrefab;

    // For all user-interaction and build logic
    public GridManager gridManager;

    // TODO: 'Constructor' needs to go
    // Find a better way to indicate where to start (either a blank save, or a load from file)
    public void MakeBuildGrid(int size) {
        gridSize = size;
        cellPrefab = Resources.Load<GameObject>("GridCell");
        gridManager = new GridManager(this);
        BuildGridFloor();
    }

    // TODO: configure these on load (or on create)
    public void ObstructCell(Vector2Int gridPos) {
        mainGrid[gridPos.x][gridPos.y].isNaturallyObstructed = true;
        mainGrid[gridPos.x][gridPos.y].isObstructed = true;
    }

    void SetGridCellVisibility(Boolean active) {
        print("Toggling grid " + active);
        foreach (GridCell[] row in mainGrid) {
            foreach (GridCell cell in row) {
                if (cell == null) continue;
                cell.BuildModeActive(active);
            }
        }
    }

    void BuildGridFloor() {
        log("Building the floor...");
        // Make a grid that's gridSize x gridSize
        mainGrid = new GridCell[gridSize][];
        for (int row = 0; row < gridSize; row++) {
            mainGrid[row] = new GridCell[gridSize];
            for (int col = 0; col < gridSize; col++) {
                mainGrid[row][col] = MakeGridCell(new Vector2Int(col, row)); ;
            }
        }
    }

    // Make cell at position (row,col) in respect to this gameobject's positon
    GridCell MakeGridCell(Vector2Int gridPos) {
        GameObject cell = Instantiate(cellPrefab);
        cell.transform.SetParent(transform, worldPositionStays: false);

        float half = gridSize / 2f;

        // Center the grid around the parent
        cell.transform.localPosition = new Vector3(
            gridPos.x - half + 0.5f,
            0f,
            gridPos.y - half + 0.5f
        );

        GridCell cellScript = cell.GetComponent<GridCell>();
        cellScript.gridPos = gridPos;
        cellScript.gridManager = gridManager;
        return cellScript;
    }

    //
    // User Interaction - send it all over to our gridManager
    //
    public bool OnBuildModeToggle() {
        if (gridSize != -1) {
            bool isBuildModeEnabled = gridManager.ToggleBuildMode();
            SetGridCellVisibility(isBuildModeEnabled);
            return isBuildModeEnabled;
        }
        return false;
    }

    // TODO: find a better way to tramp these over
    public void ArrowKeyHit(BuildModeDirection direction) {
        gridManager.BuildDirectionChanged(direction);
    }

    public void BuildOptionSelected(BuildOption option) {
        gridManager.SetBuildOption(option);
    }

    public void EnableEraserMode() {
        gridManager.EnableEraserMode();
    }

    // Monobehavior wrappers for child classes
    public void DestroyObject(GameObject obj) {
        Destroy(obj);
    }

    //
    // DEBUG
    //

    // Visualize the grid in the editor
    void OnDrawGizmos() {
        Gizmos.color = UnityEngine.Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize, 1f, gridSize));
    }


    // TODO: figure out stored logs for future debugging
    void log(String message) {
        Debug.Log("[BG] " + message);
    }
}
