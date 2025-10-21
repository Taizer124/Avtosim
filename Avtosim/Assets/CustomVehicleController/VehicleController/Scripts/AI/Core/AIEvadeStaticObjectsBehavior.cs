using UnityEngine;

namespace Assets.VehicleController
{
    public class AIEvadeStaticObjectsBehavior : AIBehavior
    {
        public AIEvadeStaticObjectsBehavior(float[] dangerArray, float[] interestArray, float[] dotArray) : base(dangerArray, interestArray, dotArray){}

        public override void ProcessData(RaycastHitInfo[] hitInfo, Vector3 trackForwardDirection)
        {
            int size = hitInfo.Length;

            for (int i = 0; i < size; i++)
                EvaluateDanger(hitInfo[i], i);
        }

        private void EvaluateDanger(RaycastHitInfo hitInfo, int i)
        {
            if (!hitInfo.Hit || hitInfo.HitDistance == 0)
                TryAssignDanger(i, 0);
            else if (hitInfo.HitVelocity == Vector3.zero)
                TryAssignDanger(i, EvadeStaticObstacleBehaviour(hitInfo, _dotArray[i]));
        }
        

        private float EvadeStaticObstacleBehaviour(RaycastHitInfo hitInfo, float dot)
        {
            return dot * Mathf.Max(hitInfo.VelocityDifference / hitInfo.HitDistance, 1 - hitInfo.HitDistance / hitInfo.RayLength);
        }
    }
}
