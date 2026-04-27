using System;
using DG.Tweening;
using R3;
using UnityEditor;
using UnityEngine;

namespace Scripts.Tools
{
    public class RotateableObject : InteractableObject
    {
        [SerializeField] private Transform rotateTransform;
        [SerializeField] private Transform movingPoint;
        [SerializeField] private Vector3 rotationVector;
        [SerializeField] private float startingRotation;
        [SerializeField] private float rotationMagnitude;
        [SerializeField] private float rotationMax;

        public readonly Subject<float> OnRotate = new();

        private float _totalRotation;
        private Quaternion _startingRotation;

        private void Awake()
        {
            _totalRotation = startingRotation * (rotationMax * 2) - rotationMax;
            _startingRotation = rotateTransform.localRotation;
            isInteractable = true;
        }

        public override void StartMovement(Vector3 startPosition)
        {
            if(!isInteractable) return;
            movingPoint.position = startPosition;
            HudController.instance.PutDragOriginPoint(movingPoint);
        }
        
        public override void MoveTo(Vector3 movePosition)
        {
            if(!isInteractable) return;
            // Plane projectionPlane = new Plane(rotateTransform.up, rotateTransform.position);
            // Debug.Log(rotateTransform.up);
            Vector3 projectionPlaneNormal = new Vector3();
            projectionPlaneNormal = rotateTransform.right * rotationVector.x + rotateTransform.up * rotationVector.y +
                            rotateTransform.forward * rotationVector.z;
            Plane projectionPlane = new Plane(projectionPlaneNormal, rotateTransform.position);
            movePosition = projectionPlane.ClosestPointOnPlane(movePosition);
            var movingPosition = projectionPlane.ClosestPointOnPlane(movingPoint.position);
            var centerPosition = projectionPlane.ClosestPointOnPlane(rotateTransform.position);
            Debug.DrawLine(PlayerInfo.instance.transform.position, movePosition, Color.red);
            Debug.DrawLine(movePosition, centerPosition);
            Debug.DrawLine(movePosition, movingPosition, Color.green);
            Debug.DrawLine(centerPosition, movingPosition, Color.yellow);

            //Calculate rotating power
            Ray ray = new Ray(centerPosition, movingPosition - centerPosition);
            float distance = Vector3.Cross(ray.direction, movePosition - ray.origin).magnitude;
            float angleRotation = rotationMagnitude * distance * Time.deltaTime;
            
            //Check for overshooting
            float maxAngleRotation = Vector3.Angle((movePosition - centerPosition), (movingPosition - centerPosition));
            if (angleRotation > maxAngleRotation) angleRotation = maxAngleRotation;
            
            //Check for clockwise or counter-clockwise
            var PlayerPos = PlayerInfo.instance.transform.position;
            Plane sidePlane = new Plane(movingPosition, centerPosition, PlayerPos);
            angleRotation = sidePlane.GetSide(movePosition) == projectionPlane.GetSide(PlayerPos) ? angleRotation : -angleRotation;
            
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
            rotateTransform.Rotate(rotationVector, angleRotation);
            OnRotate.OnNext(Mathf.InverseLerp(-rotationMax, rotationMax, _totalRotation));
        }

        public void ResetRotation(float duration)
        {
            isInteractable = false;
            float animationProgress = 0;
            Quaternion startRotation = transform.localRotation;
            DOTween.To(() => animationProgress, x =>
            {
                transform.localRotation =
                    Quaternion.Lerp(startRotation, _startingRotation, x);
            }, 1, duration).OnComplete(() =>
            {
                isInteractable = true;
                _totalRotation = startingRotation * (rotationMax * 2) - rotationMax;
            });
        }
    }
}
