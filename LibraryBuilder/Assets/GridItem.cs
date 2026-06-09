using System;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;

public class GridItem : MonoBehaviour {

    public GameObject instance;
    public BuildOption buildOption; // What kind of object
    public Vector2Int gridPos; // Where it is
    public BuildModeDirection direction = BuildModeDirection.South;

    bool visible;
    private List<MeshRenderer> renderableObjects;

    // TODO: Build, place, & rotate from save

    public void MakeInstance(BuildOption option) {
        if (instance) {
            Debug.Log("Error! Should only try to make grid item once!!!");
        }

        buildOption = option;
        instance = Instantiate(BuildModeHelpers.GetPrefabForOption(option));
        SetVisible(false);
        SetHittable(false);

        // Collect renderable objects
        renderableObjects = new List<MeshRenderer>();
        CollectRenderables(instance, renderableObjects);
    }

    public void Cleanup() {
        Debug.Log("Cleanup!");
        if (instance != null) {
            Destroy(instance);
            instance = null;
        }
        Destroy(this); // Destroy the script component
    }

    public void SetVisible(bool vis) {
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
    public void SetHoloColor() {
        foreach (MeshRenderer renderer in renderableObjects) {
            // Uncomment the following line to set the material color
            // renderer.material.color = Color.grey; // Example: Set to grey
        }
    }

}