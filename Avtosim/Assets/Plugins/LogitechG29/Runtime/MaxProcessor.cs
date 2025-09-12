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
    public class MaxProcessor : InputProcessor<float>
    {
#if UNITY_EDITOR
        static MaxProcessor()
        {
            Initialize();
        }
#endif
        public float MaxValue = 0;

        public override float Process(float value, InputControl control)
        {
            return Mathf.Max(MaxValue, value);
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<MaxProcessor>();
        }
    }
}