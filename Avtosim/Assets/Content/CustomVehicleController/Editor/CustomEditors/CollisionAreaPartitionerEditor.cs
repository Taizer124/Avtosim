using UnityEditor;
using UnityEngine;
using Assets.VehicleController;
using UnityEditor.UIElements;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CollisionAreaPartitioner))]
    public class CollisionAreaPartitionerEditor : Editor
    {
        public const string COLLISION_PARTS_FOLDER_NAME = "CollisionAreas";
        public override void OnInspectorGUI()
        {
            CollisionAreaPartitioner partitioner = (CollisionAreaPartitioner)target;
            if (partitioner == null)
                return;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionAreasSOName"));
            if (GUILayout.Button("Create New CollisionAreasSO"))
            {
                string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(COLLISION_PARTS_FOLDER_NAME);
                var result = AssetSaver.TryCreateCollisionAreasSO(folderPath, serializedObject.FindProperty("CollisionAreasSOName").stringValue, "asset");
                if(result)
                {
                    serializedObject.FindProperty("_collisionAreasDataSO").objectReferenceValue = result;
                    serializedObject.FindProperty("CollisionAreasSOName").stringValue = "";
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_meshFilter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_collisionAreasDataSO"));
            if (serializedObject.FindProperty("_collisionAreasDataSO").objectReferenceValue != null)
            {
                var areas = (serializedObject.FindProperty("_collisionAreasDataSO").objectReferenceValue as CollisionAreasDataSO);
                SerializedObject so = new SerializedObject(areas);
                EditorGUILayout.PropertyField(so.FindProperty("CollisionAreas"));
                so.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Partition the mesh into Collision Areas"))
            {
                partitioner.PartitionMeshIntoCollisionAreas();
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("DebugGizmos"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
