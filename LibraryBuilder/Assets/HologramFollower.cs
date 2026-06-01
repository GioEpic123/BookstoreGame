using UnityEngine;
using UnityEngine.EventSystems;

public class HologramFollower : MonoBehaviour
{
    public Camera cam;
    public LayerMask placementMask = ~0; // default: everything
    public float fallbackDistance = 5f;
    public float heightOffset = 0.01f;

    public GameObject holoInstance; // Hologram prefab
    Transform holoTransform;
    bool locked = false;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Start()
    {
        // Doesn't workS
        //StartHologram(holoInstance);
    }

    void Update()
    {
        if (!locked || holoInstance == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementMask))
        {
            Vector3 pos = hit.point + Vector3.up * heightOffset;
            holoTransform.position = pos;
            holoTransform.rotation = Quaternion.identity; // or match hit.normal: Quaternion.LookRotation(Vector3.forward, hit.normal)
        }
        else
        {
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = fallbackDistance;
            Vector3 pos = cam.ScreenToWorldPoint(screenPos);
            holoTransform.position = pos;
        }
    }

    public void StartHologram(GameObject prefab)
    {
        if (prefab == null) return;
        if (holoInstance != null) Destroy(holoInstance);
        holoInstance = Instantiate(prefab);
        holoTransform = holoInstance.transform;
        locked = true;
    }

    public void StopHologram(bool destroy = true)
    {
        locked = false;
        if (destroy && holoInstance != null)
        {
            Destroy(holoInstance);
            holoInstance = null;
            holoTransform = null;
        }
    }

    public bool IsLocked() => locked;
}