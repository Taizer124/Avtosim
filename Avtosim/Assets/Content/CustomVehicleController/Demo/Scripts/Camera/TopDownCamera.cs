using UnityEngine;

namespace Assets.VehicleController
{
    public class TopDownCamera : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        [SerializeField]
        private float _distance = 50;
        private const float MAX_DISTANCE = 100;
        [SerializeField]
        private float _distanceGainFromSpeed = 20;

        [SerializeField]
        private float _shakeAmount = 0.03f;

        private void LateUpdate()
        {
            if (_vehicleController == null)
            {
                _vehicleController = GameObject.FindGameObjectWithTag("Player").GetComponent<CustomVehicleController>();
                return;
            }
            float distance = _distance + _distanceGainFromSpeed * _vehicleController.GetCurrentCarStats().SpeedPercent;
            transform.position = new Vector3(_vehicleController.transform.position.x, distance, _vehicleController.transform.position.z);

            ChangeDistanceFromUserScrollInput();
            ShakeCamera();
        }
        private void ShakeCamera()
        {
            gameObject.transform.localPosition = transform.localPosition + Random.insideUnitSphere * _shakeAmount * _vehicleController.GetCurrentCarStats().SpeedPercent;
        }
        private void ChangeDistanceFromUserScrollInput()
        {
            _distance = Mathf.Clamp(_distance - Input.mouseScrollDelta.y, 1, MAX_DISTANCE);
        }
    }
}
