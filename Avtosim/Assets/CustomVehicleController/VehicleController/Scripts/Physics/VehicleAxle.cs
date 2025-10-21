using System;
using System.Linq;
using UnityEngine;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(SuspensionController)), AddComponentMenu("CustomVehicleController/Physics/Vehicle Axle")]
    public class VehicleAxle : MonoBehaviour
    {
        [SerializeField]
        private HalfShaft _leftHalfShaft;
        public HalfShaft LeftHalfShaft { get => _leftHalfShaft; }
        [SerializeField, Space]
        private HalfShaft _rightHalfShaft;
        public HalfShaft RightHalfShaft { get => _rightHalfShaft; }

        private VehiclePartsSetWrapper _partsPresetWrapper;
        private Rigidbody _rigidbody;

        private bool _front = true;

        public void InitializeAxle(VehiclePartsSetWrapper partsPresetWrapper, Rigidbody rb, float wheelBaseLen, float axelLen, bool front)
        {
            _partsPresetWrapper = partsPresetWrapper;
            _rigidbody = rb;
            _front = front;

            InitializeHalfShaft(_leftHalfShaft, partsPresetWrapper, rb, wheelBaseLen, axelLen, front);
            InitializeHalfShaft(_rightHalfShaft, partsPresetWrapper, rb, wheelBaseLen, axelLen, front);
        }

        private void InitializeHalfShaft(HalfShaft halfShaft, VehiclePartsSetWrapper partsPreset, Rigidbody rb, float wheelBaseLen, float axelLen, bool front)
        {
            halfShaft.WheelController.Initialize(halfShaft.Suspension, halfShaft.WheelVisualTransform, halfShaft.SteerParentTransform, halfShaft.ChildrenToNotRotateParentTransform, partsPreset, rb, wheelBaseLen, axelLen, front);
            halfShaft.Suspension.Initialize(partsPreset, front, _leftHalfShaft.WheelController.WheelRadius);
        }

        public void HandleVehicleAxle(float speed, float speedPercent, float acceleration, float distanceToGround, int suspensionSimulationPrecision, LayerMask ignoreLayers, bool TCS)
        {
            _leftHalfShaft.WheelController.ControlWheel(speed, speedPercent, acceleration, distanceToGround, TCS);
            _rightHalfShaft.WheelController.ControlWheel(speed, speedPercent, acceleration, distanceToGround, TCS);
            HandleSuspension(suspensionSimulationPrecision, ignoreLayers);
        }

        private void HandleSuspension(int suspensionSimulationPrecision, LayerMask ignoreLayer)
        {
            float leftTravel = 1;
            float rightTravel = 1;

            _leftHalfShaft.Suspension.CalculateSpringForceAndHitPoint(suspensionSimulationPrecision, ignoreLayer);
            if (_leftHalfShaft.Suspension.HitInfo.Hit)
            {
                leftTravel = _leftHalfShaft.Suspension.HitInfo.Distance / _leftHalfShaft.Suspension.SpringRestLength;
                ApplySuspension(_leftHalfShaft.Suspension.GetSuspForce(), _leftHalfShaft.Suspension.HitInfo.HitNormal, _leftHalfShaft.Suspension.HitInfo.Position);
            }

            _rightHalfShaft.Suspension.CalculateSpringForceAndHitPoint(suspensionSimulationPrecision, ignoreLayer);
            if (_rightHalfShaft.Suspension.HitInfo.Hit)
            {
                rightTravel = _rightHalfShaft.Suspension.HitInfo.Distance / _rightHalfShaft.Suspension.SpringRestLength;
                ApplySuspension(_rightHalfShaft.Suspension.GetSuspForce(), _rightHalfShaft.Suspension.HitInfo.HitNormal, _rightHalfShaft.Suspension.HitInfo.Position);
            }

            if (_leftHalfShaft.Suspension.HitInfo.Hit && _rightHalfShaft.Suspension.HitInfo.Hit)
            {
                float antiRoll = _front ? _partsPresetWrapper.FrontSuspension.AntiRollForce : _partsPresetWrapper.RearSuspension.AntiRollForce;
                float antiRollForce = (leftTravel - rightTravel) * antiRoll;

                ApplySuspension(-antiRollForce, _leftHalfShaft.Suspension.HitInfo.HitNormal, _leftHalfShaft.Suspension.HitInfo.Position);
                ApplySuspension(+antiRollForce, _rightHalfShaft.Suspension.HitInfo.HitNormal, _rightHalfShaft.Suspension.HitInfo.Position);
            }
        }

        private void ApplySuspension(float force, Vector3 normal, Vector3 pos)
        {
            if (force == 0)
                return;

            _rigidbody.AddForceAtPosition(force * normal, pos);
        }

        public void ApplySteering(float angle)
        {
            _leftHalfShaft.WheelController.SteerAngle = angle;
            _rightHalfShaft.WheelController.SteerAngle = angle;
        }

        public void ApplyTorque(float torque)
        {
            _leftHalfShaft.WheelController.Torque = torque;
            _rightHalfShaft.WheelController.Torque = torque;
        }

        public void ApplyBraking(float brakeForce)
        {
            _leftHalfShaft.WheelController.BrakeForce = brakeForce;
            _rightHalfShaft.WheelController.BrakeForce = brakeForce;
        }

        public void ApplyHandbrake(float force, float gasInput, float traction)
        {
            _leftHalfShaft.WheelController.ApplyHandbrake(force, gasInput, traction);
            _rightHalfShaft.WheelController.ApplyHandbrake(force, gasInput, traction);
        }

        public WheelController[] ExtractWheelControllerArray()
        {
            WheelController[] arr = new WheelController[2];
            arr[0] = _leftHalfShaft.WheelController;
            arr[1] = _rightHalfShaft.WheelController;
            return arr;
        }

        public Transform[] ExtractWheelVisualTransformArray()
        {
            Transform[] arr = new Transform[2];
            arr[0] = _leftHalfShaft.WheelVisualTransform;
            arr[1] = _rightHalfShaft.WheelVisualTransform;
            return arr;
        }

        public Transform[] ExtractSteerWheelParentArray()
        {
            Transform[] arr = new Transform[2];
            arr[0] = _leftHalfShaft.SteerParentTransform;
            arr[1] = _rightHalfShaft.SteerParentTransform;
            return arr;
        }

        public SuspensionController[] ExtractSuspensionArray()
        {
            SuspensionController[] arr = new SuspensionController[2];
            arr[0] = _leftHalfShaft.Suspension;
            arr[1] = _rightHalfShaft.Suspension;
            return arr;
        }

        public static WheelController[] ExtractVehicleWheelControllerArray(VehicleAxle[] axleArray)
        {
            WheelController[] wheelControllers = new WheelController[0];
            for (int i = 0; i < axleArray.Length; i++)
            {
                wheelControllers = wheelControllers.Concat(axleArray[i].ExtractWheelControllerArray()).ToArray();
            }
            return wheelControllers;
        }

        public static Transform[] ExtractVehicleWheelVisualTransformArray(VehicleAxle[] axleArray)
        {
            Transform[] wheelTransforms = new Transform[0];
            for (int i = 0; i < axleArray.Length; i++)
            {
                wheelTransforms = wheelTransforms.Concat(axleArray[i].ExtractWheelVisualTransformArray()).ToArray();
            }
            return wheelTransforms;
        }

        public static Transform[] ExtractVehicleSuspensionArray(VehicleAxle[] axleArray)
        {
            Transform[] parents = new Transform[0];
            for (int i = 0; i < axleArray.Length; i++)
            {
                parents = parents.Concat(axleArray[i].ExtractSteerWheelParentArray()).ToArray();
            }
            return parents;
        }

        public static SuspensionController[] ExtractVehicleSteerWheelParentArray(VehicleAxle[] axleArray)
        {
            SuspensionController[] suspensionArray = new SuspensionController[0];
            for (int i = 0; i < axleArray.Length; i++)
            {
                suspensionArray = suspensionArray.Concat(axleArray[i].ExtractSuspensionArray()).ToArray();
            }
            return suspensionArray;
        }

        public static VehicleAxle[] CombineFrontAndRearAxles(VehicleAxle[] frontAxles, VehicleAxle[] rearAxles)
        {
            return frontAxles.Concat(rearAxles).ToArray();
        }

        public void SetLeftHalfShaft(WheelController wheelController, Transform wheelVisualTransform, Transform steerParentTransform, Transform childrenToNotRotateParentTransform)
        {
            _leftHalfShaft.SetFields(wheelController, wheelVisualTransform, steerParentTransform, childrenToNotRotateParentTransform);
        }

        public void SetRightHalfShaft(WheelController wheelController, Transform wheelVisualTransform, Transform steerParentTransform, Transform childrenToNotRotateParentTransform)
        {
            _rightHalfShaft.SetFields(wheelController, wheelVisualTransform, steerParentTransform, childrenToNotRotateParentTransform);
        }
    }

    [Serializable]
    public class HalfShaft
    {
        [SerializeField]
        private WheelController _wheelController;
        public WheelController WheelController { get => _wheelController; }

        [SerializeField]
        private SuspensionController _suspension;
        public SuspensionController Suspension { get => _suspension; }

        [SerializeField]
        private Transform _wheelVisualTransform;
        public Transform WheelVisualTransform { get => _wheelVisualTransform; }

        [SerializeField]
        private Transform _steerParentTransform;
        public Transform SteerParentTransform { get => _steerParentTransform; }

        [SerializeField]
        private Transform _childrenToNotRotateParentTransform;
        public Transform ChildrenToNotRotateParentTransform { get => _childrenToNotRotateParentTransform; }

        public void SetFields(WheelController wheelController, Transform wheelVisualTransform, Transform steerParentTransform, Transform childrenToNotRotateParentTransform)
        {
            _wheelController = wheelController;
            _wheelVisualTransform = wheelVisualTransform;
            _steerParentTransform = steerParentTransform;
            _suspension = _wheelController.GetComponent<SuspensionController>();
            _childrenToNotRotateParentTransform = childrenToNotRotateParentTransform;
        }
    }
}