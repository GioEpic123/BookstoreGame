using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuildOption {
    None,
    Shelf,
    BigShelf,
    Table,
    Plant
}

public enum BuildModeDirection {
    North,
    East,
    South,
    West,
}

// Highlighting
public enum GridCellColor {
    Grey,
    Red,
    Green,
    White
}

public static class BuildModeHelpers {
    public static GameObject GetPrefabForOption(BuildOption option) {
        switch (option) {
            case BuildOption.Shelf:
                return Resources.Load<GameObject>("Shelf");
            case BuildOption.BigShelf:
                return Resources.Load<GameObject>("BigShelf");
            case BuildOption.Table:
                return Resources.Load<GameObject>("Table");
            case BuildOption.Plant:
                return Resources.Load<GameObject>("PotPlant");
            // TODO
            default:
                Debug.Log("Option not implemented: " + option);
                return null;
        }
    }

    public static Material GetMaterialForGridCellColor(GridCellColor color) {
        switch (color) {
            case GridCellColor.Grey:
                return Main.Instance.grey;
            case GridCellColor.Red:
                return Main.Instance.red;
            case GridCellColor.Green:
                return Main.Instance.green;
            case GridCellColor.White:
                return Main.Instance.white;
        }
        Debug.Log("[Helpers] Tried to obtain mat for unknown color! " + color);
        return null;
    }
}

// TODO: this is where we define the sizes of objects, I kinda hate it
// Need some kinda data store to make this cleaner

// Stored as W x L, and w >= l
public static class BuildOptionExtensions {
    private static readonly Dictionary<BuildOption, Vector2Int> dimensionMap = new Dictionary<BuildOption, Vector2Int>
    {
        { BuildOption.None, new Vector2Int(1, 1) }, // Still need a selection size, even for nothing
        { BuildOption.Shelf, new Vector2Int(1, 1) },
        { BuildOption.BigShelf, new Vector2Int(2, 1) },
        { BuildOption.Table, new Vector2Int(2, 2) },
        { BuildOption.Plant, new Vector2Int(1, 1) }
    };

    public static int Size(this BuildOption option) {
        var dimens = option.Dimensions();
        return dimens[0] * dimens[1];
    }

    public static Vector2Int Dimensions(this BuildOption option) {
        if (dimensionMap.TryGetValue(option, out var dimensions)) {
            return dimensions;
        }
        else {
            throw new ArgumentException("Unknown build option");
        }
    }
}