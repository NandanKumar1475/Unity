using UnityEngine;

public class SelectAndDrag : MonoBehaviour
{
    Camera cam;
    GameObject selected;
    Vector3 offset;

    private void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectGameObject();
        }

        if (Input.GetMouseButton(0) && selected != null)
        {
            DragGameObject();
        }

        if (Input.GetMouseButtonUp(0) && selected != null)
        {
            MeshRenderer mr = selected.GetComponent<MeshRenderer>();
            if (mr != null) mr.material.color = Color.white;

            selected = null;
        }
    }

    private void DragGameObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitinfo))
        {
            selected.transform.position = hitinfo.point + offset;
        }
    }

    private void SelectGameObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            selected = hitInfo.collider.gameObject;

            offset = selected.transform.position - hitInfo.point;

            MeshRenderer mr = selected.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material.color = Color.red;
            }
        }
    }
}
