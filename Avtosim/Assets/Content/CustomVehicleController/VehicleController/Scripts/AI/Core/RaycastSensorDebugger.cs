using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    [ExecuteInEditMode]
    public class RaycastSensorDebugger : MonoBehaviour
    {
        [SerializeField]
        private RaycastSensor _raycastSensor;
        private RaycastHitInfo[] _hitInfoArray;
        private float[] _rayAngleArray;

        public bool Debug = true;

        private void OnDrawGizmos()
        {
            if (!Debug)
                return;

            if (_raycastSensor == null)
                return;

            InitializeHitInfoArray();
            Vector3 fwd = transform.forward;
            Vector3 pos = transform.position;   
            for (int i = 0; i < _hitInfoArray.Length; i++)
            {
                _hitInfoArray[i].Direction = Quaternion.Euler(0, _rayAngleArray[i], 0) * fwd;
                UnityEngine.Debug.DrawRay(pos,
                            _hitInfoArray[i].Direction * _hitInfoArray[i].RayLength, Color.black);
            }
            UnityEngine.Debug.DrawRay(pos,
                            -fwd * _raycastSensor.RearRayLength, Color.blue);
        }

        private void InitializeHitInfoArray()
        {
            _hitInfoArray = new RaycastHitInfo[_raycastSensor.RaycastAmount];
            _rayAngleArray = new float[_raycastSensor.RaycastAmount];

            Vector3 forward = transform.forward;

            for (int i = 0; i < _hitInfoArray.Length; i++)
            {
                _hitInfoArray[i] = new RaycastHitInfo();
                if (i <= _raycastSensor.RaycastAmount / 2)
                {
                    _rayAngleArray[i] = i * _raycastSensor.MaxAngleBetweenRays;
                }
                else
                {
                    _rayAngleArray[i] = (i - _raycastSensor.RaycastAmount / 2) * -_raycastSensor.MaxAngleBetweenRays;
                }

                float cLen = _raycastSensor.RayLength;
                float aLen = cLen * Mathf.Sin(Mathf.Abs(_rayAngleArray[i]) * Mathf.Deg2Rad);
                if (aLen > _raycastSensor.MaxHorizontalDistance)
                {
                    aLen = _raycastSensor.MaxHorizontalDistance;
                    cLen = aLen / Mathf.Sin(Mathf.Abs(_rayAngleArray[i]) * Mathf.Deg2Rad);
                }

                _hitInfoArray[i].RayLength = cLen;
                _hitInfoArray[i].DotToControllerForward = Vector3.Dot(forward, Quaternion.Euler(0, _rayAngleArray[i], 0) * forward);
            }
        }
    }
}
