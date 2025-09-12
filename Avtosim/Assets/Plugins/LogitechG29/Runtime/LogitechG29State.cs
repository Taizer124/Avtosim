#region

using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

#endregion

namespace LogitechG29.Runtime
{
    [Preserve]
    public struct LogitechG29State : IInputStateTypeInfo
    {
        public static FourCC KFormat => new('J', 'O', 'Y');

        [InputControl(name = "hatSwitch", displayName = "Hat Switch", layout = "Dpad", format = "BIT", bit = 0,
             defaultState = 8, sizeInBits = 4, offset = 1),
         InputControl(name = "hatSwitch/up", displayName = "Hat Up", layout = "DiscreteButton", format = "BIT", bit = 0,
             sizeInBits = 4, parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7"),
         InputControl(name = "hatSwitch/right", displayName = "Hat Right", layout = "DiscreteButton", format = "BIT",
             bit = 0, sizeInBits = 4, parameters = "minValue=1,maxValue=3"),
         InputControl(name = "hatSwitch/left", displayName = "Hat Left", layout = "DiscreteButton", format = "BIT",
             bit = 0, sizeInBits = 4, parameters = "minValue=5,maxValue=7"),
         InputControl(name = "hatSwitch/down", displayName = "Hat Down", layout = "DiscreteButton", format = "BIT",
             bit = 0, sizeInBits = 4, parameters = "minValue=3,maxValue=5"), InputControl(name = "southButton",
             layout = "Button", offset = 1, bit = 4, usages = new[]
             {
                 "PrimaryAction",
                 "Submit"
             }, aliases = new[]
             {
                 "a",
                 "cross"
             }, displayName = "South Button", shortDisplayName = "A"), InputControl(name = "westButton",
             layout = "Button", offset = 1, bit = 5, usage = "SecondaryAction", aliases = new[]
             {
                 "x",
                 "square"
             }, displayName = "West Button", shortDisplayName = "X"), InputControl(name = "eastButton",
             layout = "Button", offset = 1, bit = 6, usages = new[]
             {
                 "Back",
                 "Cancel"
             }, aliases = new[]
             {
                 "b",
                 "circle"
             }, displayName = "East Button", shortDisplayName = "B"), InputControl(name = "northButton",
             layout = "Button", offset = 1, bit = 7, aliases = new[]
             {
                 "y",
                 "triangle"
             }, displayName = "North Button", shortDisplayName = "Y")]
        public int MainButtons;

        [InputControl(name = "rightShiftButton", displayName = "Right Shift Button", layout = "Button",
             shortDisplayName = "", offset = 2, bit = 0),
         InputControl(name = "leftShiftButton", displayName = "Left Shift Button", layout = "Button",
             shortDisplayName = "", offset = 2, bit = 1),
         InputControl(name = "rightBumperButton", displayName = "Right Bumper Button", layout = "Button",
             shortDisplayName = "", offset = 2, bit = 2),
         InputControl(name = "leftBumperButton", displayName = "Left Bumper Button", layout = "Button",
             shortDisplayName = "", offset = 2, bit = 3),
         InputControl(name = "shareButton", displayName = "Share Button", layout = "Button", shortDisplayName = "",
             offset = 2, bit = 4),
         InputControl(name = "optionsButton", displayName = "Options Button", layout = "Button", shortDisplayName = "",
             offset = 2, bit = 5),
         InputControl(name = "rightStickButton", displayName = "Right Stick Button", layout = "Button",
             shortDisplayName = "", offset = 2, bit = 6),
         InputControl(name = "leftStickButton", displayName = "Left Stick Button", layout = "Button",
             shortDisplayName = "", offset = 2, bit = 7)]
        public bool MiscButtons;

        [InputControl(name = "shifter1", displayName = "Shifter 1", layout = "Button", offset = 3, bit = 0),
         InputControl(name = "shifter2", displayName = "Shifter 2", layout = "Button", offset = 3, bit = 1),
         InputControl(name = "shifter3", displayName = "Shifter 3", layout = "Button", offset = 3, bit = 2),
         InputControl(name = "shifter4", displayName = "Shifter 4", layout = "Button", offset = 3, bit = 3),
         InputControl(name = "shifter5", displayName = "Shifter 5", layout = "Button", offset = 3, bit = 4),
         InputControl(name = "shifter6", displayName = "Shifter 6", layout = "Button", offset = 3, bit = 5),
         InputControl(name = "shifter7", displayName = "Shifter 7", layout = "Button", offset = 3, bit = 6),
         InputControl(name = "plusButton", displayName = "Plus Button", layout = "Button", offset = 3, bit = 7)]
        public bool Plus;

        [InputControl(name = "minusButton", displayName = "Minus Button", layout = "Button", shortDisplayName = "-",
             offset = 4, bit = 0),
         InputControl(name = "rightTurnButton", displayName = "Right Turn Button", layout = "Button",
             shortDisplayName = "→", offset = 4, bit = 1),
         InputControl(name = "leftTurnButton", displayName = "Left Turn Button", layout = "Button",
             shortDisplayName = "←", offset = 4, bit = 2),
         InputControl(name = "returnButton", displayName = "Return Button", layout = "Button", shortDisplayName = "↩",
             offset = 4, bit = 3),
         InputControl(name = "homeButton", displayName = "Home Button", layout = "Button", shortDisplayName = "◈",
             offset = 4, bit = 4)]
        public bool Minus;

        //[InputControl(name = "steeringAxis", displayName = "Steering Axis", layout = "Axis", shortDisplayName = "steering", offset = 5/*, parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp,clampMin=-1,clampMax=1"*/, processors = "axisDeadzone", defaultState = short.MaxValue)]
        //public short steering;

        [InputControl(name = "stick", displayName = "Stick", layout = "Stick", format = "VEC2",
            usage = "Primary2DMotion", processors = "stickDeadzone", offset = 5, sizeInBits = 40)]
        public Vector2 Stick;

        [InputControl(name = "stick/x", format = "BIT", offset = 0, sizeInBits = 16,
            parameters = "normalize=true, normalizeMin=0, normalizeMax=1, normalizeZero=0.5",
            processors = "axisDeadzone", defaultState = short.MaxValue)]
        public short X;

        [InputControl(name = "stick/y", format = "BIT", offset = 0, sizeInBits = 16,
            parameters = "normalize=true, normalizeMin=0, normalizeMax=1, normalizeZero=0.5",
            processors = "axisDeadzone", defaultState = short.MaxValue)]
        public short Y;

        [InputControl(name = "throttleAxis", displayName = "Throttle Axis", layout = "Axis",
            shortDisplayName = "throttle", offset = 7,
            parameters = "normalize=true,normalizeMin=1,normalizeMax=0,normalizeZero=0.5")]
        public byte Throttle;

        [InputControl(name = "brakeAxis", displayName = "Brake Axis", layout = "Axis", shortDisplayName = "brake",
            offset = 8, parameters = "normalize=true,normalizeMin=1,normalizeMax=0,normalizeZero=0.5")]
        public byte Brake;

        [InputControl(name = "clutchAxis", displayName = "Clutch Axis", layout = "Axis", shortDisplayName = "clutch",
            offset = 9, parameters = "normalize=true,normalizeMin=1,normalizeMax=0,normalizeZero=0.5")]
        public byte Clutch;

        public FourCC format => KFormat;
    }
}