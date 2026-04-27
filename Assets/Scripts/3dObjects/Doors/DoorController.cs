using System;
using DG.Tweening;
using R3;
using Scripts.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Objects.Doors
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private RotateableObject handle;
        [SerializeField] private Vector3 rotationEndValue;
        [SerializeField] private AnimationCurve rotationCurve;
        [SerializeField] private float animationDuration;
        [SerializeField] private float doorAutoCloseDistance;

        private Quaternion _rotationStartValue;
        private Status _status;

        enum Status
        {
            Closed,
            Opening,
            Open,
            Closing,
            
            Default
        }
        private void Awake()
        {
            handle.OnRotate.Subscribe(CheckDoor);
            _rotationStartValue = transform.localRotation;
            _status = Status.Closed;
        }

        private void Update()
        {
            if (_status == Status.Open && Time.frameCount % 2 >= 1 && Vector3.Distance(PlayerInfo.instance.transform.position, transform.position) > doorAutoCloseDistance)
           {
                CloseDoor();
           }
        }

        private void CheckDoor(float value)
        {
            if(_status == Status.Closed && value >= 0.95f) OpenDoor();
        }
        private void OpenDoor()
        {
            _status = Status.Opening;
            float animationProgress = rotationCurve.keys[0].time;
            Quaternion startRotation = transform.localRotation;
            DOTween.To(() => animationProgress, x =>
            {
                transform.localRotation = Quaternion.Lerp(startRotation, Quaternion.Euler(rotationEndValue),
                    rotationCurve.Evaluate(x));
            }, rotationCurve.keys[^1].time, animationDuration).OnComplete(() => {_status = Status.Open;});
        }
        
        private void CloseDoor()
        {
            _status = Status.Closing;
            float animationProgress = rotationCurve.keys[^1].time;
            Quaternion startRotation = transform.localRotation;
            handle.ResetRotation(animationDuration);
            DOTween.To(() => animationProgress, x =>
            {
                transform.localRotation =
                    Quaternion.Lerp(_rotationStartValue, startRotation, rotationCurve.Evaluate(x));
            }, rotationCurve.keys[0].time, animationDuration).OnComplete(() => {_status = Status.Closed;});
        }
    }
}
