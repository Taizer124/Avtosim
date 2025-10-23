using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class SeparatorAttribute : PropertyAttribute
    {
        public const float DEFAULT_HEIGHT = 1.5f;
        public const float DEFAULT_LEFT_PADDING = 10;
        public const float DEFAULT_VERTICAL_OFFSET = -5;
        public float Height
        {
            get;
            private set;
        }
        public float LeftPadding
        {
            get;
            private set;
        }
        public float VerticalOFfset
        {
            get;
            private set;
        }

        public Color32 Color = new Color32(128, 128, 128, 255);

        public SeparatorAttribute(float height = DEFAULT_HEIGHT, float leftPadding = DEFAULT_LEFT_PADDING, float bottomPadding = DEFAULT_VERTICAL_OFFSET)
        {
            Height = height;
            LeftPadding = leftPadding;
            VerticalOFfset = bottomPadding;
        }
    }
}

