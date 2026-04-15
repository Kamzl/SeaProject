using System;
using UnityEditor;
using UnityEngine;

namespace scripts.tools
{
    public class RotateableObject : MonoBehaviour, InteractableObject
    {
        [SerializeField] private Transform rotateTransform;
        [SerializeField] private Transform movingPoint;
        [SerializeField] private RotationAxis rotationAxis;
        [SerializeField] private float rotationMagnitude;
        [SerializeField] private float rotationMax;

        private float _totalRotation;
        private float _lastRotation;

        private Vector3 _rotationVector;
        

        enum RotationAxis
        {
            X,
            Y,
            Z
        }

        private void OnEnable()
        {
            switch (rotationAxis)
            {
                case RotationAxis.X:
                    _rotationVector = Vector3.right;
                    break;
                case RotationAxis.Y:
                    _rotationVector = Vector3.up;
                    break;
                case RotationAxis.Z:
                    _rotationVector = Vector3.forward;
                    break;
            }
        }

        public void StartMovement(Vector3 startPosition)
        {
            movingPoint.position = startPosition;
            HudController.instance.PutDragOriginPoint(movingPoint);
        }
        
        public void MoveTo(Vector3 movePosition)
        {
            Plane projectionPlane = new Plane(rotateTransform.up, rotateTransform.position);
            movePosition = projectionPlane.ClosestPointOnPlane(movePosition);
            var movingPosition = projectionPlane.ClosestPointOnPlane(movingPoint.position);
            var centerPosition = projectionPlane.ClosestPointOnPlane(rotateTransform.position);

            //Calculate rotating power
            Ray ray = new Ray(centerPosition, movingPosition - centerPosition);
            float distance = Vector3.Cross(ray.direction, movePosition - ray.origin).magnitude;
            float angleRotation = rotationMagnitude * distance * Time.deltaTime;
            
            //Check for overshooting
            float maxAngleRotation = Vector3.Angle((movePosition - centerPosition), (movingPosition - centerPosition));
            if (angleRotation > maxAngleRotation) angleRotation = maxAngleRotation;
            
            //Check for clockwise or counter-clockwise
            Plane sidePlane = new Plane(movingPosition, centerPosition, PlayerInfo.instance.transform.position);
            angleRotation = sidePlane.GetSide(movePosition) ? angleRotation : -angleRotation;
            
            //Check for max rotation
            if (Mathf.Abs(_totalRotation + angleRotation) > rotationMax)
            {
                if (_totalRotation > 0)
                {
                    angleRotation = rotationMax - Mathf.Abs(_totalRotation);
                }
                else
                {
                    angleRotation = -(rotationMax - Mathf.Abs(_totalRotation));
                }
            }
            _totalRotation += angleRotation;
            rotateTransform.Rotate(_rotationVector, angleRotation);
            _lastRotation = angleRotation;
        }
    }
}
