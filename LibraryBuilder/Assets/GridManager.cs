using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class GridManager {

    /// Manage cell interactions, highlighting, and anything related to user interaction

    // Logic
    bool inBuildMode = false;
    public bool isEraserMode = false;
    public BuildGrid parentGrid;
    BuildOption currentBuildMode = BuildOption.None; // Null in eraser mode
    // TODO: Scroll to rotate!
    BuildModeDirection currentBuildDirection = BuildModeDirection.South; // All items face the camera by default
    List<GridCell> currentSelection = new List<GridCell>();
    GridCell currentActiveCell;

    Hologram hologram;


    public GridManager(BuildGrid mama) {
        parentGrid = mama; //awe

        // Make holo object, attach to buildGrid
        var gameObject = new GameObject();
        gameObject.AddComponent<Hologram>();
        gameObject.transform.parent = parentGrid.transform;
        hologram = gameObject.GetComponent<Hologram>();
    }

    //
    // Build Mode Config
    //

    public bool ToggleBuildMode() {
        inBuildMode = !inBuildMode;
        currentBuildMode = BuildOption.None;
        return inBuildMode;
    }

    public void SetBuildOption(BuildOption option) {
        log("Selected build option: " + option);
        currentBuildMode = option;
        isEraserMode = false;
        if (option != BuildOption.None) {
            UpdateHologram(option);
        }
        else {
            hologram.SetVisible(false);
        }
    }

    public void EnableEraserMode() {
        log("Entering eraser mode...");
        if (hologram != null) {
            hologram.SetVisible(false);
        }
        currentBuildMode = BuildOption.None;
        isEraserMode = true;
    }

    public void BuildDirectionChanged(BuildModeDirection direction) {
        log("Build Direction now " + direction);
        currentBuildDirection = direction;
        if (currentActiveCell) {
            GridCellColor currentSelectionColor = currentActiveCell.currentColor;
            ClearSelection();
            UpdateSelection(currentActiveCell);
            HighlightSelection(currentSelectionColor);
        }
    }


    //
    // MOUSE ACTIVITY
    // - Can assume all build actions are taken in build mode, as events won't register otherwise

    public void CellWasEntered(GridCell cell) {
        UpdateSelection(cell);

        if (isEraserMode) {
            bool canErase = cell.isObstructed;
            HighlightSelection(canErase ? GridCellColor.Green : GridCellColor.Red);
        }
        else if (currentBuildMode != BuildOption.None) {
            HighlightSelection(CanAddItemToCell(cell) ? GridCellColor.Green : GridCellColor.Red);
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

            if (TryAddItemToCell(cell)) {
                // TODO: Consider success color? 
                HighlightSelection(GridCellColor.Green);
            }
            else {
                log("!! Error !! Can't build here!!");
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
    // Selections
    //

    public void UpdateSelection(GridCell cell) {
        currentActiveCell = cell;
        currentSelection = GetSelectionForCellAndOption(cell, currentBuildMode, currentBuildDirection);
        MoveHolo();
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

    // Main 'Grid Algorithm'
    // Given that we're pointing at this cell, and using the current config (option, direction)
    // Return the cells that would make up the 'selection' for it
    public List<GridCell> GetSelectionForCellAndOption(GridCell cell, BuildOption buildMode, BuildModeDirection direction) {
        // Build direction starts out South. We grow N and E

        // Examples: c is cursor, x is part of selection

        // [x] [x] [ ]
        // [x] [c] [ ]     2x2 object, oriented South
        // [ ] [ ] [ ] 

        // It rotates around the selected cell:

        // [ ] [ ] [ ]
        // [x] [c] [ ]      East
        // [x] [x] [ ]   

        // [ ] [ ] [ ] 
        // [ ] [c] [x]      North
        // [ ] [x] [x]   

        // [ ] [x] [x]
        // [ ] [c] [x]      West
        // [ ] [ ] [ ]   

        Vector2Int cellPos = cell.gridPos;
        Vector2Int dimensions = buildMode.Dimensions();
        int verticalSize = -1;
        int horizontalSize = -1;
        int verticalSearch = 0;
        int horizontalSearch = 0;

        // Set search directions
        switch (direction) {
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
        List<GridCell> selection = new List<GridCell>();

        for (int vertical = 0; vertical < verticalSize; vertical++) {
            for (int horizontal = 0; horizontal < horizontalSize; horizontal++) {
                // We always grow up and to the left of the cursor cell
                int x = cellPos[0] + (horizontal * horizontalSearch);
                int y = cellPos[1] + (vertical * verticalSearch);

                if (x < 0 || x >= grid.Length || y < 0 || y >= grid[x].Length) {
                    // out of bounds
                    continue;
                }

                selection.Add(grid[y][x]);
            }
        }
        return selection;
    }

    //
    // User Write/Removal of Grid Items
    //

    // See if adding at current selection is possible
    public bool CanAddItemToCell(GridCell cell) {

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

    public bool TryAddItemToCell(GridCell cell) {
        if (!CanAddItemToCell(cell)) {
            return false;
        }
        // Steal the hologram, it's already in position & well oriented 
        hologram.SetColor(GridCellColor.White);
        hologram.SetHittable(true);
        GameObject instance = hologram.instance;
        hologram.instance = null;

        // Child the instance to the cell
        instance.transform.parent = cell.transform;

        // Only target cell attaches, and has it as a child
        cell.attachedObject = instance;
        cell.attachedObjectBuildOption = currentBuildMode;
        cell.attachedObjectDirection = currentBuildDirection;
        // All selection cells get obstructed
        foreach (GridCell selectionCell in currentSelection) {
            selectionCell.SetObstructed(true);
        }
        currentBuildMode = BuildOption.None; // Clear held item
        return true;
    }

    public void EraseItemInCell(GridCell cell) {

        //First, find what our selection is here
        BuildOption option = cell.attachedObjectBuildOption;
        var selectionAtCell = GetSelectionForCellAndOption(cell, cell.attachedObjectBuildOption, cell.attachedObjectDirection);



        //UpdateSelection(cell);
        parentGrid.DestroyObject(cell.attachedObject);
        Debug.Log("attc " + cell.attachedObject);
        foreach (GridCell eraserCell in selectionAtCell) {
            Debug.Log("er attc " + eraserCell.attachedObject);
            eraserCell.attachedObject = null;
            eraserCell.SetObstructed(false);
        }
        // currentSelection = new List<GridCell> {
        //     cell
        // };
    }

    public void RotateInstanceToMatchBuildDirection(GameObject instance) {
        switch (currentBuildDirection) {
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

    //
    // Hologram
    //

    public void UpdateHologram(BuildOption option) {
        hologram.MakeInstance(option);
    }

    public void MoveHolo() {
        if (!hologram.instance) return;
        if (currentActiveCell == null) {
            hologram.SetVisible(false);
            return;
        }
        hologram.SetVisible(true);
        RotateInstanceToMatchBuildDirection(hologram.instance);
        hologram.instance.transform.position = currentActiveCell.transform.position;
    }

    void log(String message) {
        Debug.Log($"[GM]: {message}");
    }

}