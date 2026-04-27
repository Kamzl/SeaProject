using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Tools
{
    public abstract class InteractableObject : MonoBehaviour
    {
        [HideInInspector] public bool isInteractable;

        public virtual void StartMovement(Vector3 startPosition)
        {
            
        }

        public virtual void MoveTo(Vector3 movePosition)
        {
        
        }
    }
}