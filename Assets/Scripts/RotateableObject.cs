using UnityEngine;

namespace scripts.tools
{
    public class RotateableObject : MonoBehaviour, InteractableObject
    {
        [SerializeField] private Transform rotateTransform;
        private Vector3 _startPoint;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void StartMovement(Vector3 startPosition)
        {
            _startPoint = startPosition;
        }
        
        public void MoveTo(Vector3 movePosition)
        {
            
        }
    }
}
