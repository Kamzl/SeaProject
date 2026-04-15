using scripts.tools;
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
    private InteractableObject interactableObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputAction action = inputAsset.FindAction("Interact", false);
        action.canceled += context =>
        {
            isInteracts = false;
            HudController.instance.SetIsInteractableDragged(false);
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
        if (isInteracts)
        {
            interactableObject.MoveTo(interactObject.position);
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
        
        if (Physics.Raycast(ray, out hit, raycastDistance, LayerMask.GetMask("Interactable")))
        {
            isInteracts = true;
            interactObject.position = hit.point;
            Debug.Log(hit.transform.gameObject.layer);
            Debug.Log("Hit!");
            interactableObject = hit.transform.GetComponentInParent<InteractableObject>();
            interactableObject.StartMovement(interactObject.position);
            
            HudController.instance.SetIsInteractableDragged(true);
        }
    }
}
