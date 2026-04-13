using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;

public class InteractController : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private Transform interactObject;
    [SerializeField] private float raycastDistance;
    [SerializeField] private InputActionAsset inputAsset;

    private bool isInteracts;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputAction action = inputAsset.FindAction("Interact", false);
        action.canceled += context =>
        {
            isInteracts = false;
        };
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray()
        {
            origin = camera.transform.position,
            direction = camera.transform.forward
        };
        RaycastHit hit;
        if (isInteracts && Physics.Raycast(ray, out hit, raycastDistance, LayerMask.GetMask("Interactable")))
        {
            Debug.DrawLine(ray.origin, hit.point);
        }
    }

    private void OnInteract()
    {
        Ray ray = new Ray()
        {
            origin = camera.transform.position,
            direction = camera.transform.forward
        };
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            isInteracts = true;
            interactObject.position = hit.point;
            Debug.Log(hit.transform.gameObject.layer);
        }
        
        if (Physics.Raycast(ray, out hit, raycastDistance, LayerMask.GetMask("Interactable")))
        {
            Debug.Log("Hit!");
        }
        
    }
}
