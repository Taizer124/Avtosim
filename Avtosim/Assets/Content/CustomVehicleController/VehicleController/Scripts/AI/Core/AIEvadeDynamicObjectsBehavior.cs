using UnityEngine;

namespace Assets.VehicleController
{
    public class AIEvadeDynamicObjectsBehavior : AIBehavior
    {
        public AIEvadeDynamicObjectsBehavior(float[] dangerArray, float[] interestArray, float[] dotArray) : base(dangerArray, interestArray, dotArray){}

        public override void ProcessData(RaycastHitInfo[] hitInfo, Vector3 trackForwardDirection)
        {
            for (int i = 0; i < hitInfo.Length; i++)
                EvaluateDanger(hitInfo[i], i);
        }

        private void EvaluateDanger(RaycastHitInfo hitInfo, int i)
        {
            if (!hitInfo.Hit || hitInfo.HitDistance == 0)
                TryAssignDanger(i, 0);
            else if (hitInfo.HitVelocity != Vector3.zero)
                TryAssignDanger(i, EvadeDynamicObstacleBehaviour(hitInfo, _dotArray[i]));
        }


        private float EvadeDynamicObstacleBehaviour(RaycastHitInfo hitInfo, float dot)
        {
            if (hitInfo.HitDistance < 5)
            {
                if (hitInfo.DotToControllerForward > 0.7f)
                    return hitInfo.DotToControllerForward;
                return Mathf.Clamp01(2 * (1 - hitInfo.DotToControllerForward));
            }

            if (hitInfo.VelocityDifference > -5)
                return 0;

            return dot * Mathf.Clamp01(-hitInfo.VelocityDifference / hitInfo.HitDistance);
        }
    }
}
