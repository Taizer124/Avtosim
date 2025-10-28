using UnityEngine;

namespace Assets.VehicleController
{
    public class RacerAIGasInput
    {
        public float GetInput(RaycastHitInfo hitInfo, float dot, AIRacerState state, float brakeInput)
        {
            if (brakeInput > 0.5f)
                return 0;

            if (state == AIRacerState.ReturningToTrack || state == AIRacerState.Reversing)
            {
                if (hitInfo.Hit)
                    return 1;
                return 0;
            }

            if (dot > 0)
                return dot;

            float velocityDifference = hitInfo.VelocityDifference;

            if (hitInfo.HitDistance == 0)
                return 1;

            if (velocityDifference > 0)
                return 1;

            if (velocityDifference < 0)
                return 1 - Mathf.Clamp01(-velocityDifference / hitInfo.HitDistance);

            return Mathf.Clamp01(1 - (velocityDifference / hitInfo.HitDistance)) * dot;
        }

        private bool NoDangersAhead(RaycastHitInfo[] hitInfo)
        {
            for (int i = 0; i < hitInfo.Length; i++)
            {
                if (hitInfo[i].DotToControllerForward < 0.8f)
                    continue;
                if (hitInfo[i].Hit)
                    return false;
            }
            return true;
        }
    }
}
