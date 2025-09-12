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
    public class MinProcessor : InputProcessor<float>
    {
#if UNITY_EDITOR
        static MinProcessor()
        {
            Initialize();
        }
#endif
        public float MinValue = 0;

        public override float Process(float value, InputControl control)
        {
            return Mathf.Min(MinValue, value);
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<MinProcessor>();
        }
    }
}