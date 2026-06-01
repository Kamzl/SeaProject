using Scripts.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

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
        Physics.Raycast(ray, out hit, raycastDistance, ~LayerMask.GetMask("Player"));
        if (hit.transform && hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            isInteracts = true;
            interactObject.position = hit.point;
            interactableObject = hit.collider.transform.GetComponentInParent<InteractableObject>();
            interactableObject.StartMovement(interactObject.position);
            
            HudController.instance.SetIsInteractableDragged(true);
        }
    }

    public Camera GetCamera()
    {
        return camera;
    }
    
    public float GetRaycastDistance()
    {
        return raycastDistance;
    }
}
