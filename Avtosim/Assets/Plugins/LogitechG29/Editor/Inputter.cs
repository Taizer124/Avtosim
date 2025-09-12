#region

using System.Linq;
using UnityEditor;
using UnityEditor.Build;

#endregion

namespace LogitechG29.Editor
{
    /// <summary>
    ///     Добавляет заданные символы определения в символы определения PlayerSettings.
    ///     Просто добавьте свои собственные символы определения в свойство «Символы» ниже.
    /// </summary>
    [InitializeOnLoad]
    public class Inputter : UnityEditor.Editor
    {
        /// <summary>
        ///     Добавьте символы определения, как только Unity завершит компиляцию.
        /// </summary>
        static Inputter()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var fromBuildTargetGroup = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

            var definesString = PlayerSettings.GetScriptingDefineSymbols(fromBuildTargetGroup);

            var allDefines = definesString.Split(';').ToList();

            allDefines.AddRange(Symbols.Except(allDefines));

            PlayerSettings.SetScriptingDefineSymbols(fromBuildTargetGroup, string.Join(";", allDefines.ToArray()));
        }

        /// <summary>
        ///     Символы, которые будут добавлены в редактор
        /// </summary>
        public static readonly string[] Symbols =
        {
            "INPUTTER"
        };
    }
}