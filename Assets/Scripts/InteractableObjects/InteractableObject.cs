using UnityEngine;

namespace Scripts.Tools
{
    public abstract class InteractableObject : MonoBehaviour
    {
        [SerializeField] protected bool isInteractable;

        public virtual void StartMovement(Vector3 startPosition)
        {
            
        }

        public virtual void MoveTo(Vector3 movePosition)
        {
        
        }
    }
}