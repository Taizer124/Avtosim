using UnityEngine;

namespace Assets.VehicleController
{
    public class RacerAIBreakInput
    {
        public float GetInput(RaycastHitInfo followDirHit, float dotToTrack, RaycastHitInfo dangerHit, float dotToDanger, float risk, AIRacerState state)
        {
            if (state == AIRacerState.ReturningToTrack || state == AIRacerState.Reversing)
            {
                if (followDirHit.Hit)
                    return 0;
                return 1;
            }


            if (dangerHit != null && dotToDanger >= risk)
            {
                if (dangerHit.HitVelocity != Vector3.zero)
                {
                    if (dangerHit.VelocityDifference >= 0)
                        return 0;

                    return (-dangerHit.VelocityDifference / 4 / dangerHit.HitDistance);
                }
                return (dangerHit.VelocityDifference / 4 / dangerHit.HitDistance);
            }
            return 0;
        }
    }
}
