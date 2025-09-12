#region

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

#endregion

namespace LogitechG29.Runtime
{
#if UNITY_EDITOR
    [InitializeOnLoad, Preserve]
#endif
    public class ExtensionProcessor : InputProcessor<float>
    {
#if UNITY_EDITOR
        static ExtensionProcessor()
        {
            Initialize();
        }
#endif

        private float _previousValue;

        [Tooltip("Gravity Speed")] public float GravitySpeed = 0;
        [Tooltip("Sensitivity Speed")] public float SensitivitySpeed = 0;

        public override float Process(float value, InputControl control)
        {
            if (value == 0)
            {
                _previousValue = Mathf.MoveTowards(_previousValue, 0f, GravitySpeed * Time.unscaledDeltaTime);
            }

            _previousValue = Mathf.MoveTowards(_previousValue, value, SensitivitySpeed * Time.unscaledDeltaTime);
            return _previousValue;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<ExtensionProcessor>();
        }
    }
}