using UnityEngine;

namespace Assets.VehicleController
{
    public abstract class PositionEvaluator : MonoBehaviour
    {
        public abstract Vector3 GetFollowTrackDirection(Vector3 position, float speedABS);
        public abstract void CalculateProgress(Vector3 position);
        public abstract float GetProgress();
    }
}
