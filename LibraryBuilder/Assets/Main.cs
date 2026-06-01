using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject player; // Not yet implemented
    public BuildGrid buildGrid; // This won't be passed like this, but for our tester it works
    public UISystem ui;


    static Main instance;

    public static Main Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Main>();
            }
            return instance;
        }
    }

    // Prototype: Build the grid, position it, and make it confugurable on "Build Mode" toggle.
    void Start()
    {
        // 10x10 test - make one in the middle to start
        buildGrid = Instantiate(buildGrid);
        buildGrid.MakeBuildGrid(10);
        // Block the center
        buildGrid.ObstructCell(new Vector2Int(4, 4));
        buildGrid.transform.position = new Vector3(0, 0.5f, 0); // Center the grid on the origin
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if(moveHorizontal < 0 || moveHorizontal > 0)
        {
            buildGrid.ArrowKeyHit(moveHorizontal < 0 ? BuildModeDirection.West : BuildModeDirection.East);
        }else if(moveVertical < 0 || moveVertical > 0)
        {
            buildGrid.ArrowKeyHit(moveVertical < 0 ? BuildModeDirection.South : BuildModeDirection.North);
        }
    }

    // Main TODO list:
    // --> Adding
    // --> UI to select item & select eraser
    // --> Serialize to save/load
}
