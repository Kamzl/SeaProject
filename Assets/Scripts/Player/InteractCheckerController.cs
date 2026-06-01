using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractCheckerController : MonoBehaviour
{
    [SerializeField] private InteractController interactController;

    private List<string> crossedInteractables = new();
    private List<string> filteredInteractables = new();

    private void Update()
    {
        if (crossedInteractables.Count > filteredInteractables.Count)
        {
            Camera camera = interactController.GetCamera();
            Ray ray = new Ray()
            {
                origin = camera.transform.position,
                direction = camera.transform.forward
            };
            RaycastHit hit;
            Physics.Raycast(ray, out hit, interactController.GetRaycastDistance(), ~LayerMask.GetMask("Player"));
            if (hit.transform && hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactable") && crossedInteractables.Contains(hit.transform.gameObject.name) && !filteredInteractables.Contains(hit.transform.gameObject.name))
            {
                filteredInteractables.Add(hit.transform.gameObject.name);
                HudController.instance.SetIsCrosshairOnInteractable(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            crossedInteractables.Add(other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        crossedInteractables.Remove(other.gameObject.name);
        filteredInteractables.Remove(other.gameObject.name);
        if(filteredInteractables.Count <= 0) HudController.instance.SetIsCrosshairOnInteractable(false);
    }
}
