using UnityEngine;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/additional-settings/car-visuals-essentials"), AddComponentMenu("CustomVehicleController/Visuals/Car Visuals Essentials")]
    public class CarVisualsEssentials : MonoBehaviour
    {
        private Rigidbody _rigidBody;
        private CurrentCarStats _currentCarStats;

        #region Wheel Meshes
        [SerializeField, Tooltip("Array of all the vehicle's axles")]
        private VehicleAxle[] _axleArray;

        [SerializeField, Tooltip("Array of the axles with steerable wheels")]
        private VehicleAxle[] _steerAxleArray;
        #endregion

        [Space, Separator, SerializeField, Tooltip("Doesn't allow the wheel position to go beyond suspension top point and tries to keep the wheel above the ground (for example, when landing after a jump)." +
            "If the suspension length is too low, wheel position might be to updated correctly when grounded.")]
        private bool _restrainWheelPosition = true;
        [SerializeField, Tooltip("When there is no user input, steer wheels will rotate to face the vehicle's velocity.")]
        private bool _alignSteerWheelsToVelocity = true;
        [SerializeField, Tooltip("If the abs is turned off and wheels lock during braking, wheels game objects won't rotate.")]
        private bool _wheelsLockVisually = true;
        // metres per second
        private const float ALIGN_MIN_SPEED = 5;

        private float _steerWheelsAngle = 0;

        private float _smDempVelocity;

        private bool[] _isOrientationCorrectArray;

        public void Initialize(Rigidbody rb, CurrentCarStats currentCarStats)
        {
            _rigidBody = rb;
            _currentCarStats = currentCarStats;

            //if the wheel has wrong orientation
            //(for example it has to be rotated 180 degrees around the Y-axis to have the same forward vector as the car)
            //then this find those wheels and adds 180 degrees to their rotation in update loop.
            _isOrientationCorrectArray = new bool[_axleArray.Length * 2];

            int wheelId = 0;

            for (int i = 0; i < _axleArray.Length; i++)
            {
                _isOrientationCorrectArray[wheelId] = IsOrientationCorrect(_axleArray[i].LeftHalfShaft.WheelVisualTransform.localEulerAngles.y);
                wheelId++;

                _isOrientationCorrectArray[wheelId] = IsOrientationCorrect(_axleArray[i].RightHalfShaft.WheelVisualTransform.localEulerAngles.y);
                wheelId++;
            }
        }

        private bool IsOrientationCorrect(float angle)
        {
            if (angle >= 180)
                angle -= 360;
            if (angle <= -180)
                angle += 360;

            return angle < 90 && angle > -90;
        }

        public void HandleWheelVisuals(float input, float currentWheelAngle, float maxSteerAngle, float steerSpeed)
        {
            SpinWheels();
            SteerWheels(input, currentWheelAngle, maxSteerAngle, steerSpeed);
            UpdateWheelsPosition();
        }

        private void SpinWheels()
        {
            float lockEffect = 0;
            if(_wheelsLockVisually)
            {
                lockEffect = _axleArray[0].LeftHalfShaft.WheelController.LockedUp ? 1 : 0;
                if (UnityEngine.Random.Range(0, 1f) > 0.9f)
                    lockEffect = 0.97f;
            }

            int wheelId = 0;

            for (int i = 0; i < _axleArray.Length; i++)
            {
                float rotationX = _axleArray[i].LeftHalfShaft.WheelController.VisualRPM * (1 - lockEffect);

                if (!_isOrientationCorrectArray[wheelId])
                    _axleArray[i].LeftHalfShaft.WheelVisualTransform.localRotation *= Quaternion.Euler(new Vector3(-rotationX, 0, 0));
                else
                    _axleArray[i].LeftHalfShaft.WheelVisualTransform.localRotation *= Quaternion.Euler(new Vector3(rotationX, 0, 0));

                wheelId++;


                if (!_isOrientationCorrectArray[wheelId])
                    _axleArray[i].RightHalfShaft.WheelVisualTransform.localRotation *= Quaternion.Euler(new Vector3(-rotationX, 0, 0));
                else
                    _axleArray[i].RightHalfShaft.WheelVisualTransform.localRotation *= Quaternion.Euler(new Vector3(rotationX, 0, 0));

                wheelId++;
            }
        }

        private void SteerWheels(float input, float currentWheelAngle, float maxSteerAngle, float steerSpeed)
        {
            if (_alignSteerWheelsToVelocity)
            {
                _steerWheelsAngle = Mathf.SmoothDampAngle(_steerWheelsAngle, AlignWheelsWithVelocityVelocity(input, maxSteerAngle), ref _smDempVelocity, steerSpeed);
            }
            else
                _steerWheelsAngle = Mathf.SmoothDampAngle(_steerWheelsAngle, currentWheelAngle, ref _smDempVelocity, steerSpeed);


            for (int i = 0; i < _steerAxleArray.Length; i++)
            {
                if (_steerAxleArray[i].LeftHalfShaft.SteerParentTransform == null ||
                    _steerAxleArray[i].RightHalfShaft.SteerParentTransform == null)
                    continue;

                _steerAxleArray[i].LeftHalfShaft.SteerParentTransform.localRotation = Quaternion.Euler(_steerAxleArray[i].LeftHalfShaft.SteerParentTransform.localRotation.x,
                    Mathf.Clamp(_steerWheelsAngle, -maxSteerAngle, maxSteerAngle),
                    _steerAxleArray[i].LeftHalfShaft.SteerParentTransform.localRotation.z);

                _steerAxleArray[i].RightHalfShaft.SteerParentTransform.localRotation = Quaternion.Euler(_steerAxleArray[i].RightHalfShaft.SteerParentTransform.localRotation.x,
                    Mathf.Clamp(_steerWheelsAngle, -maxSteerAngle, maxSteerAngle),
                    _steerAxleArray[i].RightHalfShaft.SteerParentTransform.localRotation.z);
            }
        }

        private float AlignWheelsWithVelocityVelocity(float input, float maxSteerAngle)
        {
            float angle = 0;

            if (input == 0)
            {
                if (!_currentCarStats.InAir && Mathf.Abs(_currentCarStats.SpeedInMsPerS) > ALIGN_MIN_SPEED)
                    angle = Vector3.SignedAngle(transform.forward * Mathf.Sign(_currentCarStats.SpeedInMsPerS), _rigidBody.linearVelocity, transform.up);
            }
            else
                angle = maxSteerAngle * input;

            angle = Mathf.Clamp(angle, -maxSteerAngle, maxSteerAngle);

            return angle;
        }

        private void UpdateWheelsPosition()
        {
            for (int i = 0; i < _axleArray.Length; i++)
            {
                _axleArray[i].LeftHalfShaft.WheelController.UpdateWheelPosition(_restrainWheelPosition);
                _axleArray[i].LeftHalfShaft.WheelVisualTransform.transform.localPosition = _axleArray[i].LeftHalfShaft.WheelController.WheelPosition;

                _axleArray[i].RightHalfShaft.WheelController.UpdateWheelPosition(_restrainWheelPosition);
                _axleArray[i].RightHalfShaft.WheelVisualTransform.transform.localPosition = _axleArray[i].RightHalfShaft.WheelController.WheelPosition;
            }
        }
        public VehicleAxle[] GetAxleArray() => _axleArray;

        public CurrentCarStats GetCurrentCarStats()
        {
            if (_currentCarStats == null)
                _currentCarStats = GetComponent<CustomVehicleController>().GetCurrentCarStats();

            return _currentCarStats;
        }
        public Rigidbody GetRigidbody() => GetComponent<CustomVehicleController>().GetRigidbody();
    }
}
