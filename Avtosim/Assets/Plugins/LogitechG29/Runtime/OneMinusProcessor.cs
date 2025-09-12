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
    public class OneMinusProcessor : InputProcessor<float>
    {
#if UNITY_EDITOR
        static OneMinusProcessor()
        {
            Initialize();
        }
#endif
        public override float Process(float value, InputControl control)
        {
            return 1f - value;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<OneMinusProcessor>();
        }
    }
}