using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISystem : MonoBehaviour {
    public GameObject buildModeButton;
    public GameObject buildMenu;
    Main main;
    void Start() {
        main = Main.Instance;
    }

    public void OnBuildModeToggle() {
        bool buildModeActive = main.buildGrid.OnBuildModeToggle();
        Debug.Log("Build mode tapped, BuildGrid Returned active: " + buildModeActive);
        buildMenu.SetActive(buildModeActive);
        buildModeButton.SetActive(!buildModeActive);
    }

    // BUILD MENU:
    // - Logic lives in grid, notify 
    // TODO: this is way too static, we need a way to determine which objects user has access to at runtime
    public void OnPlantButtonSelect() {
        SelectBuildOption(BuildOption.Plant);
    }

    public void OnShelfButtonSelect() {
        SelectBuildOption(BuildOption.Shelf);
    }

    public void OnBigShelfButtonSelect() {
        SelectBuildOption(BuildOption.BigShelf);
    }

    public void OnTableButtonSelect() {
        SelectBuildOption(BuildOption.Table);
    }

    public void OnEraserButtonSelect() {
        main.buildGrid.EnableEraserMode();
    }

    void SelectBuildOption(BuildOption option) {
        main.buildGrid.BuildOptionSelected(option);
    }
}
