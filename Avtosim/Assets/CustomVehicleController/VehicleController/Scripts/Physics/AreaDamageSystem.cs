using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class AreaDamageSystem
    {
        public int FindCollisionAreaID(Transform transform, Vector3 collPoint, CollisionAreasDataSO collisionAreasDataSO)
        {
            int size = collisionAreasDataSO.CollisionAreas.Length;

            for (int i = 0; i < size; i++)
            {
                if (IsPointInside(transform, collisionAreasDataSO.CollisionAreas[i], collPoint, 0))
                    return i;
            }

            return size - 1;
        }

        public List<int> FindAffectedCollisionAreasID(Transform transform, Vector3 collPoint, float damageArea, CollisionAreasDataSO collisionAreasDataSO)
        {
            int size = collisionAreasDataSO.CollisionAreas.Length;

            List<int> affectedCollisionAreas = new();

            for (int i = 0; i < size; i++)
            {
                if (IsPointInside(transform, collisionAreasDataSO.CollisionAreas[i], collPoint, damageArea))
                    affectedCollisionAreas.Add(i);
            }

            return affectedCollisionAreas;
        }

        private bool IsPointInside(Transform transform, CollisionArea collArea, Vector3 collPoint, float damageArea)
        {
            // Convert the world position of the point to the local space of the transform
            Vector3 localPoint = transform.InverseTransformPoint(collPoint);

            float halfDamageArea = damageArea / 2;

            // Calculate the bounds of the collision area in its local space
            float rightBound = collArea.Center.x + collArea.Width / 2 + halfDamageArea;
            float leftBound = collArea.Center.x - collArea.Width / 2 - halfDamageArea;
            float topBound = collArea.Center.y + collArea.Height / 2 + halfDamageArea;
            float bottomBound = collArea.Center.y - collArea.Height / 2 - halfDamageArea;
            float frontBound = collArea.Center.z + collArea.Length / 2 + halfDamageArea;
            float rearBound = collArea.Center.z - collArea.Length / 2 - halfDamageArea;

            // Check if the local point is within the bounds of the collision area
            return (localPoint.x <= rightBound && localPoint.x >= leftBound) &&
                   (localPoint.y <= topBound && localPoint.y >= bottomBound) &&
                   (localPoint.z <= frontBound && localPoint.z >= rearBound);
        }
    }
}
