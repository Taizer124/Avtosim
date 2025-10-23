using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [Serializable]
    public class RaycastHitInfo
    {
        public bool Hit;
        public float HitDistance;
        public Vector3 Direction;
        public float RayLength;
        public float VelocityDifference;
        public Vector3 HitVelocity;
        public float DotToControllerForward;

        public RaycastHitInfo()
        {
            Hit = false;
            HitDistance = 0f;
            Direction = new Vector3(0,0,0);
            RayLength = 0;
            VelocityDifference = 0;
            HitVelocity = Vector3.zero;
            DotToControllerForward = 0;
        }
    }
}

