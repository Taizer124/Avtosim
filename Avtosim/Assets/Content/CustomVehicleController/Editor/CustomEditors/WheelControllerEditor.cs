using Assets.VehicleController;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(WheelController))]
    public class WheelControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WheelController wheel = (WheelController)target;
            base.OnInspectorGUI();
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Recalculate Wheel Radius", "If the provided Wheel Mesh Transform has MeshRenderer, the radius will be calculated automatically from the height of the mesh.")))
            {
                if(wheel.GetWheelTransform() != null)
                {
                    if (wheel.GetWheelTransform().TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                    {
                        SerializedObject so = new SerializedObject(wheel);
                        SerializedProperty wheelRadius = so.FindProperty("_wheelRadius");
                        wheelRadius.floatValue = mesh.bounds.size.y / 2;
                        so.ApplyModifiedProperties();
                        so.Update();
                    }              
                    else
                        Debug.LogError("Wheel Mesh Transform game object has no MeshRenderer");
            }
                else
                    Debug.LogError("Selected wheel has no Wheel Mesh Transform assigned");
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
