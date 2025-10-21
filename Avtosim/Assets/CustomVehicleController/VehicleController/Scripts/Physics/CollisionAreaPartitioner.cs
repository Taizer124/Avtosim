using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/vehicle-damage-system/collision-area-partitioner")]
    public class CollisionAreaPartitioner : MonoBehaviour
    {
        [SerializeField]
        private string CollisionAreasSOName;
        [SerializeField, Space, Space]
        private MeshFilter _meshFilter;
        [SerializeField]
        private CollisionAreasDataSO _collisionAreasDataSO;

        public bool DebugGizmos = true;


        public void PartitionMeshIntoCollisionAreas()
        {
            if(_meshFilter == null)
            {
                Debug.LogError("MeshFilter isn't assigned");
                return;
            }

            if(_collisionAreasDataSO == null)
            {
                Debug.LogError("CollisionAreasData SO isn't assigned");
                return;
            }
            _collisionAreasDataSO.UpdateAreaVertices(DistibuteVerticesIntoAreas());
        }

        private Dictionary<int, List<int>> FindAreasForVertices()
        {
            Vector3[] allVertices = _meshFilter.sharedMesh.vertices;
            int vertices = allVertices.Length;

            Dictionary<int, List<int>> verticesIdsPerAreaDict = new();

            for (int i = 0; i < vertices; i++)
            {
                int id = FindVerticeAreaID(allVertices[i]);
                if (!verticesIdsPerAreaDict.ContainsKey(id))
                    verticesIdsPerAreaDict.Add(id, new List<int>());

                verticesIdsPerAreaDict[id].Add(i);
            }

            return verticesIdsPerAreaDict;
        }

        private AreaVerticesContainer[] DistibuteVerticesIntoAreas()
        {
            var verticesPerAreaDict = FindAreasForVertices();

            int areasAmount = verticesPerAreaDict.Count;
            AreaVerticesContainer[] verticesPartitioned = new AreaVerticesContainer[areasAmount];

            for (int i = 0; i < areasAmount; i++)
            {
                string name = "Rest";
                if (i < areasAmount - 1)
                    name = _collisionAreasDataSO.CollisionAreas[i].Name;

                AreaVerticesContainer temp = new(verticesPerAreaDict[i].ToArray(), name);
                verticesPartitioned[i] = temp;
            }

            return verticesPartitioned;
        }

        private int FindVerticeAreaID(Vector3 vertice)
        {
            int len = _collisionAreasDataSO.CollisionAreas.Length;
            for(int i = 0; i < len; i++)
            {
                if (IsInsideCollisionArea(i, _meshFilter.transform.TransformPoint(vertice)))
                    return i;
            }

            return len;
        }

        private bool IsInsideCollisionArea(int areaID, Vector3 verticeWorldPos)
        {
            CollisionArea collArea = _collisionAreasDataSO.CollisionAreas[areaID];

            // Convert the world position of the point to the local space of the transform
            Vector3 localPoint = transform.InverseTransformPoint(verticeWorldPos);

            // Calculate the bounds of the collision area in its local space
            float leftBound = collArea.Center.x - collArea.Width / 2;
            float rightBound = collArea.Center.x + collArea.Width / 2;
            float topBound = collArea.Center.y + collArea.Height / 2;
            float bottomBound = collArea.Center.y - collArea.Height / 2;
            float frontBound = collArea.Center.z + collArea.Length / 2;
            float rearBound = collArea.Center.z - collArea.Length / 2;

            // Check if the local point is within the bounds of the collision area
            return (localPoint.x <= rightBound && localPoint.x >= leftBound) &&
                   (localPoint.y <= topBound && localPoint.y >= bottomBound) &&
                   (localPoint.z <= frontBound && localPoint.z >= rearBound);
        }

        private Vector3[] _vertices = {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1),
            };

        private int[] _triangles = {
            0, 2, 1, //face front
			0, 3, 2,
            2, 3, 4, //face top
			2, 4, 5,
            1, 2, 5, //face right
			1, 5, 6,
            0, 7, 4, //face left
			0, 4, 3,
            5, 4, 7, //face back
			5, 7, 6,
            0, 6, 7, //face bottom
			0, 1, 6
            };

        private Color[] _colors = new Color[] {
            new Color(0.33f, 0.33f, 1f, 0.8f),
            new Color(0.33f, 1f, 0.33f, 0.8f),
            new Color(1f, 0.33f, 0.33f, 0.8f),
            new Color(1f, 1f, 0.33f, 0.8f),
            new Color(0.33f, 1f, 1f, 0.8f)};



        private void OnDrawGizmos()
        {
            if (!DebugGizmos)
                return;
            Vector3 offset = -transform.right / 2 - transform.forward / 2 - transform.up / 2;

            int i = 0;
            int colorsLen = _colors.Length;

            if (_collisionAreasDataSO == null || _collisionAreasDataSO.CollisionAreas == null)
                return;

            foreach (var collArea in _collisionAreasDataSO.CollisionAreas)
            {

                Mesh mesh = new Mesh();
                mesh.vertices = _vertices;
                mesh.triangles = _triangles;
                mesh.Optimize();
                mesh.RecalculateNormals();
                Gizmos.color = _colors[i % colorsLen];
                i++;
                Vector3 size = new Vector3(collArea.Width, collArea.Height, collArea.Length);

                Vector3 position = transform.rotation * (collArea.Center - size / 2) + transform.position;

                Gizmos.DrawMesh(mesh, position, Quaternion.LookRotation(transform.forward), size);
            }
        }
    }
    [Serializable]
    public class CollisionArea
    {
        public string Name;
        public Vector3 Center;
        [Min(0.1f)]
        public float Height = 1;
        [Min(0.1f)]
        public float Width = 1;
        [Min(0.1f)]
        public float Length = 1;
    }
}
