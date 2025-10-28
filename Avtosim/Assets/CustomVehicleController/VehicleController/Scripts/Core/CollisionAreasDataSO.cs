using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "CollisionAreasDataSO", menuName = "CustomVehicleController/DamageSystem/CollisionAreasData")]
    public class CollisionAreasDataSO : ScriptableObject
    {
        public CollisionArea[] CollisionAreas;
        public AreaVerticesContainer[] AreasVertices;
        public void UpdateAreaVertices(AreaVerticesContainer[] newVertices)
        {
            AreasVertices = newVertices;
        }
    }

    [Serializable]
    public class AreaVerticesContainer
    {
        public string Name;
        public AreaVerticesContainer(int[] AreaVerticesIds, string Name)
        {
            this.AreaVerticesIndexes = AreaVerticesIds;
            this.Name = Name;
        }
        public int[] AreaVerticesIndexes;
    }
}
