using UnityEngine;

namespace Assets.VehicleController
{
    public class AIDangerProcessor
    {
        public float CorrectSteeringFromFutureCollision(float dotToDanger, float steer)
        {
            return steer < 0 ? -1 : 1;
        }

        public int FindMovingDirectionID(Vector3 velocity, float[] dangerArray, float lowestDanger, RaycastHitInfo[] hitInfos)
        {
            int size = dangerArray.Length;

            float closestDot = 0;
            int id = -1;

            for(int i = 0; i < size; i++)
            {
                if (dangerArray[i] <= lowestDanger)
                    continue;

                float dot = Vector3.Dot(velocity, hitInfos[i].Direction);
                if(dot > closestDot)
                {
                    id = i;
                    closestDot = dot;
                }
            }
            return id;
        }

        public bool MovingInDangerDirection(Vector3 moveDir, Vector3 dangerDir, float risk, RaycastHitInfo hitInfo) => (hitInfo.VelocityDifference * risk > hitInfo.HitDistance) && Vector3.Dot(moveDir, dangerDir) > risk; 
    }
}
