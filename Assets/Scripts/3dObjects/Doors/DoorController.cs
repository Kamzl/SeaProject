using DG.Tweening;
using R3;
using Scripts.Tools;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Objects.Doors
{
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private RotateableObject handle;
        [SerializeField] private Vector3 rotationEndValue;
        [SerializeField] private AnimationCurve rotationCurve;
        [SerializeField] private float animationDuration;

        public UnityAction OnDoorOpen;

        private Quaternion _rotationStartValue;
        private Status _status;
        private Collider _collider;

        [Inject] private InteractableService _interactableService;
        private float _doorAutoCloseDistance;

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
            _doorAutoCloseDistance = _interactableService.InteractableSettings.doorAutoCloseDistance;
            _collider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (_status == Status.Open && Time.frameCount % 9 <= 0 && Vector3.Distance(PlayerInfo.instance.transform.position, transform.position) > _doorAutoCloseDistance)
           {
                CloseDoor();
           }
        }

        private void CheckDoor(float value)
        {
            if(_status == Status.Closed && value >= _interactableService.rotateableInfo[id].Threshold) OpenDoor();
        }
        public void OpenDoor()
        {
            if(_status != Status.Closed) return;
            _status = Status.Opening;
            transform.DORotate(rotationEndValue, animationDuration).SetEase(rotationCurve).OnComplete(() =>
            {
                _status = Status.Open;
                OnDoorOpen.Invoke();
            });
        }
        
        public void CloseDoor()
        {
            if(_status != Status.Open) return;
            _status = Status.Closing;
            float animationProgress = rotationCurve.keys[^1].time;
            Quaternion startRotation = transform.localRotation;
            handle.ResetRotation(animationDuration);
            transform.DORotateQuaternion(_rotationStartValue, animationDuration).SetEase(rotationCurve).OnComplete(() => {_status = Status.Closed;});
            // DOTween.To(() => animationProgress, x =>
            // {
            //     transform.localRotation =
            //         Quaternion.Lerp(_rotationStartValue, startRotation, rotationCurve.Evaluate(x));
            // }, rotationCurve.keys[0].time, animationDuration).OnComplete(() => {_status = Status.Closed;});
        }
    }
}
