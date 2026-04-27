using R3;
using Scripts.Tools;
using UnityEngine;

namespace Objects.Ship
{
    public class ShipController : MonoBehaviour
    {
        [SerializeField] private float torqueForce;
        [SerializeField] private float linearForce;
        [SerializeField] private RotateableObject wheel;
        [SerializeField] private RotateableObject lever;
        [SerializeField] private ForceMode rotationMode;
        [SerializeField] private ForceMode forceMode;

        private Rigidbody _rigidbody;
    
        //Goes from -1 to 1
        private float _rotation;
        private float _speed;

        private const float DefaultSteeringValue = 0.5f;

        private void Awake()
        {
            wheel.OnRotate.Subscribe(x => _rotation = (x - DefaultSteeringValue) * 2);
            lever.OnRotate.Subscribe(x => _speed = (x - DefaultSteeringValue) * 2);
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _rigidbody.AddRelativeTorque(Vector3.up * (_rotation * torqueForce), rotationMode);
            Debug.Log($"Rotation force: {_rotation * torqueForce}");
            _rigidbody.AddRelativeForce(Vector3.forward * (_speed * linearForce), forceMode);
            Debug.Log($"Linear force: {_speed * linearForce}");
        }
    }
}
