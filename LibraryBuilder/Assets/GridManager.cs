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
    BuildOption currentBuildMode = BuildOption.None; // Null in eraser mode
    // TODO: Scroll to rotate!
    BuildModeDirection buildDirection = BuildModeDirection.South; // All items face the camera by default
    List<GridCell> currentSelection = new List<GridCell>();
    GridCell currentActiveCell;

    GridItem hologram;


    public GridManager(BuildGrid mama) {
        parentGrid = mama; //awe
    }

    public bool ToggleBuildMode() {
        inBuildMode = !inBuildMode;
        currentBuildMode = BuildOption.None;
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
        if (hologram != null) {
            hologram.Cleanup();
            hologram = null;
        }
        currentBuildMode = option;
        isEraserMode = false;
        if (option != BuildOption.None) {
            MakeGridItem(option);
        }
    }

    public void EnableEraserMode() {
        log("Entering eraser mode...");
        if (hologram != null) {
            hologram.Cleanup();
            hologram = null;
        }
        currentBuildMode = BuildOption.None;
        isEraserMode = true;
    }

    public void MakeGridItem(BuildOption option) {
        var gameObject = new GameObject();
        gameObject.AddComponent<GridItem>();
        hologram = gameObject.GetComponent<GridItem>();
        hologram.MakeInstance(option);
    }

    public void MoveHolo() {
        Debug.Log("Moving holo!");
        if (currentActiveCell == null) {
            hologram.SetVisible(false);
            return;
        }
        hologram.SetVisible(true);
        RotateInstanceToMatchBuildDirection(hologram.instance);
        hologram.instance.transform.position = currentActiveCell.transform.position;
    }

    // MOUSE ACTIVITY
    // - Can assume all build actions are taken in build mode, as clicks won't register otherwise
    // - Can also assume cell isn't naturallyObstructed for the same reason

    public void CellWasEntered(GridCell cell) {
        UpdateSelection(cell.gridPos);
        currentActiveCell = cell;

        if (isEraserMode) {
            bool canErase = cell.isObstructed;
            HighlightSelection(canErase ? GridCellColor.Green : GridCellColor.Red);
        }
        else if (currentBuildMode != BuildOption.None) {
            MoveHolo();

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
            if (!cell.isObstructed) {
                HighlightSelection(GridCellColor.Red);
            }
            else {
                EraseItemInCell(cell);
                // TODO: Consider erase success color?
                HighlightSelection(GridCellColor.Grey);
            }
        }
        else if (currentBuildMode != BuildOption.None) {

            if (cell.isObstructed) {
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
        MoveHolo();
    }

    //
    // CRUD (Add, Remove)
    //

    public void EraseItemInCell(GridCell cell) {

        //First, find what our selection is here
        BuildOption option = cell.attachedObjectBuildOption;
        currentBuildMode = option;
        UpdateSelection(cell.gridPos);
        Destroy(cell.attachedObject);
        foreach (GridCell selCell in currentSelection) {
            selCell.attachedObject = null;
            selCell.SetObstructed(false);
        }
        currentBuildMode = BuildOption.None;
        currentSelection = new List<GridCell> {
            cell
        };
    }

    // See if adding at current selection is possible
    public Boolean CanAddItemToCell(GridCell cell) {
        if (cell.isObstructed) {
            Debug.Log("cell has an item");
            return false;
        }

        // some cells out of bounds
        if (currentSelection.Count < currentBuildMode.Size()) {
            Debug.Log("selection too small");
            return false;
        }

        foreach (GridCell auxCell in currentSelection) {
            if (auxCell.isObstructed) {
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


        // TODO: URGENT! This doesn't work with holo code, need to refactor
        GameObject instance = Instantiate(BuildModeHelpers.GetPrefabForOption(currentBuildMode), cell.transform);

        // All selection cells get obstructed
        foreach (GridCell selectionCell in currentSelection) {
            selectionCell.SetObstructed(true);
        }
        // Only target cell attaches, and has it as a child
        RotateInstanceToMatchBuildDirection(instance);
        cell.attachedObject = instance;
        cell.attachedObject.transform.localPosition = Vector3.zero;
        cell.attachedObjectBuildOption = currentBuildMode;
        currentBuildMode = BuildOption.None; // Clear held item
        return true;
    }

    // Use the 'grid algo' to get the currently selected cells.
    // Add them to currentSelection
    public void UpdateSelection(Vector2Int cellPos) {
        // Build direction starts out South

        // [x] [x]
        // [x] [c]     // 2x2 object, S

        // [x] [c]
        // [x] [x]     // 2x2 object, E

        // [c] [x]
        // [x] [x]     // 2x2 object, N

        // [x] [x]
        // [c] [x]     // 2x2 object, W

        Vector2Int dimensions = currentBuildMode.Dimensions();
        int verticalSize = -1;
        int horizontalSize = -1;
        int verticalSearch = 0;
        int horizontalSearch = 0;

        // Set search directions
        switch (buildDirection) {
            case BuildModeDirection.North:
                horizontalSize = dimensions[0];
                verticalSize = dimensions[1];
                verticalSearch = -1;
                horizontalSearch = 1;
                break;
            case BuildModeDirection.South:
                horizontalSize = dimensions[0];
                verticalSize = dimensions[1];
                verticalSearch = 1;
                horizontalSearch = -1;
                break;
            case BuildModeDirection.East:
                verticalSize = dimensions[0];
                horizontalSize = dimensions[1];
                verticalSearch = -1;
                horizontalSearch = -1;
                break;
            case BuildModeDirection.West:
                verticalSize = dimensions[0];
                horizontalSize = dimensions[1];
                verticalSearch = 1;
                horizontalSearch = 1;
                break;
        }

        GridCell[][] grid = parentGrid.mainGrid;

        for (int vertical = 0; vertical < verticalSize; vertical++) {
            for (int horizontal = 0; horizontal < horizontalSize; horizontal++) {
                // We always grow up and to the left of the cursor cell
                int x = cellPos[0] + (horizontal * horizontalSearch);
                int y = cellPos[1] + (vertical * verticalSearch);

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
        // TODO:
        hologram.SetHoloColor(); // NO OP! Implement
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