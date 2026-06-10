using System;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;

public class Hologram : MonoBehaviour {

    public GameObject instance;
    public BuildOption buildOption; // What kind of object

    bool visible;
    private List<MeshRenderer> renderableObjects;

    // TODO: Build, place, & rotate from save

    public void MakeInstance(BuildOption option) {
        buildOption = option;
        instance = Instantiate(BuildModeHelpers.GetPrefabForOption(option), transform);
        SetVisible(false);
        SetHittable(false);

        // Collect renderable objects, hack for color change
        renderableObjects = new List<MeshRenderer>();
        CollectRenderables(instance, renderableObjects);
        SetColor(GridCellColor.Grey);
    }

    public void SetVisible(bool vis) {
        if (!instance) {
            Debug.Log("tried to trigger visibility on nonexistant instance!!!");
            return;
        }
        instance.SetActive(vis);
        visible = vis;
    }

    public void SetHittable(bool hit) {
        var collider = instance.GetComponent<Collider>();
        if (collider != null) {
            collider.enabled = hit;

        }
    }

    // TODO: This is really bad, but lets us support all of our item's sub-materials for now.
    // Later, this will be about setting the graphics material (only one object)

    private void CollectRenderables(GameObject parent, List<MeshRenderer> renderables) {
        foreach (Transform child in parent.transform) {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer != null) {
                renderables.Add(renderer);
            }
            CollectRenderables(child.gameObject, renderables); // Recursively check children
        }
    }
    public void SetColor(GridCellColor color) {
        foreach (MeshRenderer renderer in renderableObjects) {
            renderer.material = BuildModeHelpers.GetMaterialForGridCellColor(color);
        }
    }

}