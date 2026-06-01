using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class GridManager : MonoBehaviour {

    /// Manage cell interactions, highlighting, and anything related to user interaction

    // Logic
    bool inBuildMode = false;
    public bool isEraserMode = false;
    public BuildGrid parentGrid;
    BuildOption currentItem = BuildOption.None; // Null in eraser mode
    // TODO: Scroll to rotate!
    BuildModeDirection buildDirection = BuildModeDirection.South; // All items face the camera by default
    List<GridCell> currentSelection = new List<GridCell>();
    GridCell currentActiveCell;


    public GridManager(BuildGrid mama) {
        parentGrid = mama; //awe
    }

    public Boolean ToggleBuildMode() {
        inBuildMode = !inBuildMode;
        currentItem = BuildOption.None;
        return inBuildMode;
    }

    public void BuildDirectionChanged(BuildModeDirection direction) {
        Debug.Log("Build Direction now " + direction);
        buildDirection = direction;
        if (currentActiveCell) {
            GridCellColor currentSelectionColor = currentActiveCell.currentColor;
            ClearSelection();
            UpdateSelection(currentActiveCell.gridPos);
            HighlightSelection(currentSelectionColor);
        }
    }

    public void SetBuildOption(BuildOption option) {
        log("Selected build option: " + option);
        currentItem = option;
        isEraserMode = false;
    }

    public void EnableEraserMode() {
        log("Entering eraser mode...");
        currentItem = BuildOption.None;
        isEraserMode = true;
    }

    // MOUSE ACTIVITY
    // - Can assume all build actions are taken in build mode, as clicks won't register otherwise
    // - Can also assume cell isn't naturallyObstructed for the same reason

    public void CellWasEntered(GridCell cell) {
        UpdateSelection(cell.gridPos);
        currentActiveCell = cell;

        if (isEraserMode) {
            bool canErase = cell.attachedObject != null;
            HighlightSelection(canErase ? GridCellColor.Green : GridCellColor.Red);
        }
        else if (currentItem != BuildOption.None) {
            if (cell.attachedObject) {
                HighlightSelection(GridCellColor.Red);
            }
            else if (CanAddItemToCell(cell)) {
                HighlightSelection(GridCellColor.Green);
            }
            else {
                HighlightSelection(GridCellColor.Red);
            }
        }
        else {
            HighlightSelection(GridCellColor.White);
        }
    }


    // Returns color cell should display as a result of operation
    public void CellWasClicked(GridCell cell) {
        if (isEraserMode) {
            if (cell.attachedObject == null) {
                HighlightSelection(GridCellColor.Red);
            }
            else {
                EraseItemInCell(cell);
                // TODO: Consider erase success color?
                HighlightSelection(GridCellColor.Grey);
            }
        }
        else if (currentItem != BuildOption.None) {

            if (cell.attachedObject) {
                Debug.Log("Already has an Object Attached!");
                HighlightSelection(GridCellColor.Red);
            }
            if (TryAddItemToCell(cell)) {
                // TODO: Consider success color? 
                HighlightSelection(GridCellColor.Green);
            }
            else {
                Debug.Log("Can't build here!!");
                HighlightSelection(GridCellColor.Red);
            }
        }
    }


    public void CellWasExited() {
        currentActiveCell = null;
        HighlightSelection(GridCellColor.Grey);
        ClearSelection();
    }

    //
    // CRUD (Add, Remove)
    //

    public void EraseItemInCell(GridCell cell) {
        Destroy(cell.attachedObject);
        // Figure out how to remove item from the other cells it's connected to
        cell.attachedObject = null;
    }

    // See if adding at current selection is possible
    public Boolean CanAddItemToCell(GridCell cell) {
        if (cell.attachedObject != null) {
            Debug.Log("cell has an item");
            return false;
        }

        // some cells out of bounds
        if (currentSelection.Count < currentItem.Size()) {
            Debug.Log("selection too small");
            return false;
        }

        foreach (GridCell auxCell in currentSelection) {
            if (auxCell.attachedObject != null) {
                return false;
            }
        }

        return true;
    }

    public Boolean TryAddItemToCell(GridCell cell) {
        if (!CanAddItemToCell(cell)) {
            return false;
        }

        // Note about item creation, re: saving:

        // -> Items will only need to know where they're placed at, and the direction they were built in
        // Won't need to know all of the cells that they're in (as i prev thought)
        // ! Cells will STILL NEED to know that they're obstructed (maybe home cell will know it's got an attached object?)


        GameObject instance = Instantiate(BuildModeHelpers.GetPrefabForOption(currentItem), cell.transform);

        // All selection cells get obstructed
        foreach (GridCell selectionCell in currentSelection) {
            selectionCell.SetObstructed(true);
        }
        // Only target cell attaches, and has it as a child
        RotateInstanceToMatchBuildDirection(instance);
        cell.attachedObject = instance;
        cell.attachedObject.transform.localPosition = Vector3.zero;
        currentItem = BuildOption.None; // Clear held item
        return true;
    }

    // Use the 'grid algo' to get the currently selected cells.
    // Add them to currentSelection
    public void UpdateSelection(Vector2Int cellPos) {
        // Selection always grows to the left and up from the origin cell
        // Build direction starts out South

        //     [c]    // 1x1 object, S or N

        // [x] [c]    // 2x1 object, S or N

        // [x]
        // [c]        // 2x1 object, E or W

        // [x] [x]
        // [x] [c]     // 2x2 object, NSE or W

        Vector2Int dimensions = currentItem.Dimensions();
        int verticalSize = -1;
        int horizontalSize = -1;

        // Set search directions
        switch (buildDirection) {
            case BuildModeDirection.North:
            case BuildModeDirection.South:
                horizontalSize = dimensions[0];
                verticalSize = dimensions[1];
                break;
            case BuildModeDirection.East:
            case BuildModeDirection.West:
                verticalSize = dimensions[0];
                horizontalSize = dimensions[1];
                break;
        }

        GridCell[][] grid = parentGrid.mainGrid;

        for (int vertical = 0; vertical < verticalSize; vertical++) {
            for (int horizontal = 0; horizontal < horizontalSize; horizontal++) {
                // We always grow up and to the left of the cursor cell
                int x = cellPos[0] + (horizontal * -1);
                int y = cellPos[1] + vertical;

                if (x < 0 || x >= grid.Length || y < 0 || y >= grid[x].Length) {
                    // out of bounds
                    continue;
                }

                currentSelection.Add(grid[y][x]);
            }
        }
        return;
    }

    public void HighlightSelection(GridCellColor color) {
        foreach (GridCell cell in currentSelection) {
            cell.ChangeColor(color);
        }
    }

    public void ClearSelection() {
        if (currentSelection.Count > 0) {
            HighlightSelection(GridCellColor.Grey);
            currentSelection.Clear();
        }
    }

    public void RotateInstanceToMatchBuildDirection(GameObject instance) {
        switch (buildDirection) {
            case BuildModeDirection.North:
                instance.transform.rotation = Quaternion.identity;
                break;
            case BuildModeDirection.East:
                instance.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case BuildModeDirection.South:
                instance.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case BuildModeDirection.West:
                instance.transform.rotation = Quaternion.Euler(0, 270, 0);
                break;
        }
    }

    void log(String message) {
        Debug.Log($"[GM]: {message}");
    }

}