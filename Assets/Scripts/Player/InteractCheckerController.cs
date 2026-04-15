using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractCheckerController : MonoBehaviour
{
    [SerializeField] private InteractController interactController;

    private List<string> crossedInteractables = new();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            crossedInteractables.Add(other.gameObject.name);
            HudController.instance.SetIsCrosshairOnInteractable(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        crossedInteractables.Remove(other.gameObject.name);
        if(crossedInteractables.Count <= 0) HudController.instance.SetIsCrosshairOnInteractable(false);
    }
}
