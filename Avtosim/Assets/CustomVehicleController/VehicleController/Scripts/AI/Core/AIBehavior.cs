using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public abstract class AIBehavior
    {
        protected float[] _dangerArray;
        protected float[] _interestArray;
        protected float[] _dotArray;

        public AIBehavior(float[] dangerArray, float[] interestArray, float[] dotArray)
        {
            _dangerArray = dangerArray;
            _interestArray = interestArray;
            _dotArray = dotArray;
        }

        public abstract void ProcessData(RaycastHitInfo[] hitInfo, Vector3 trackForwardDirection);
        protected void TryAssignDanger(int i, float danger)
        {
            if(danger > _dangerArray[i])
                _dangerArray[i] = danger;
        }
    }
}
