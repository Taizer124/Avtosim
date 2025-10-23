using UnityEngine;

namespace Assets.VehicleController
{
    public class HoodCamera : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;
        [SerializeField]
        private Vector3 _offset;
        [SerializeField]
        private float _shakeAmount = 0.03f;
        [SerializeField]
        private float _defaultFOV = 45;
        [SerializeField]
        private float _fovFromSpeed = 15;
        [SerializeField]
        private float _maxSpeed = 100;

        private Camera _camera;
        private void Start()
        {
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (_vehicleController == null)
            {
                _vehicleController = GameObject.FindGameObjectWithTag("Player").GetComponent<CustomVehicleController>();
                return;
            }
            transform.position = _vehicleController.transform.position + _vehicleController.transform.root.TransformDirection(_offset);
            transform.forward = _vehicleController.transform.forward;

            ShakeCamera();
            ChangeFoV();
        }
        private void ShakeCamera()
        {
            gameObject.transform.localPosition = transform.localPosition + Random.insideUnitSphere * _shakeAmount * _vehicleController.GetCurrentCarStats().SpeedPercent;
            gameObject.transform.localRotation = _vehicleController.transform.localRotation;
        }
        private void ChangeFoV()
        {
            _camera.fieldOfView = _defaultFOV + _fovFromSpeed * (_vehicleController.GetCurrentCarStats().SpeedInMsPerS / _maxSpeed);
        }
    }
}
