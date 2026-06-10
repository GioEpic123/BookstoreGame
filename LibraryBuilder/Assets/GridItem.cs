using System;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;

public class GridItem {

    public GameObject instance;
    public BuildOption buildOption; // What kind of object
    public Vector2Int gridPos; // Where it is
    public BuildModeDirection direction = BuildModeDirection.South;

}