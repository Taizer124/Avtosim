using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    public static class PartTypeNameValidator
    {
        public static bool CheckClassNameIsValid(string name) => IsValidClassName(name);
            
        private static bool IsValidClassName(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                Debug.LogWarning("Engine type name is empty");
                return false;
            }

            if(className.Length == 1 && className[0] == '_')
            {
                Debug.LogWarning("Class name cannot be $'_'");
                return false;
            }

            if (!char.IsLetter(className[0]) && className[0] != '_')
            {
                Debug.LogWarning("Engine type name must start with a letter or an underscore");
                return false;
            }

            foreach (char c in className)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    Debug.LogWarning("Engine type name can only include letters, digits and underscores");
                    return false;
                }
            }

            if (IsKeyword(className))
            {
                Debug.LogWarning("Engine type name must be different from the reserved c# keywords");
                return false;
            }

            if (Type.GetType(className) != null)
            {
                Debug.LogWarning("Engine type name must not have name conflicts with other classes");
                return false;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == className)
                    {
                        Debug.LogWarning("Class with the same name already exists");
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsKeyword(string word)
        {
            string[] keywords = { "abstract", "as",
                "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
                "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false",
                "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal",
                "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private",
                "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
                "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
                "ushort", "using", "virtual", "void", "volatile", "while" };
            return Array.Exists(keywords, w => w.Equals(word));
        }
    }
}
