using UnityEngine;
using UnityEngine.InputSystem;

public class VRDeviceChecker : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("CheckVRDevices", 1f, 5f);
    }

    void CheckVRDevices()
    {
        int vrDeviceCount = 0;

        foreach (var device in InputSystem.devices)
        {
            if (device.description.interfaceName == "XRInput" ||
                device.name.Contains("XR") ||
                device.name.Contains("Oculus") ||
                device.name.Contains("OpenXR"))
            {
                vrDeviceCount++;
                Debug.Log($"VR Device: {device.name}");

                // ПРАВИЛЬНЫЙ СПОСОБ: используем GetChildControl
                try
                {
                    var triggerControl = device.GetChildControl("trigger");
                    if (triggerControl != null)
                    {
                        float triggerValue = triggerControl.ReadValueAsObject() is float value ? value : 0f;
                        Debug.Log($"  Trigger value: {triggerValue}");
                    }

                    // Проверяем другие элементы управления
                    var gripControl = device.GetChildControl("grip");
                    if (gripControl != null)
                    {
                        float gripValue = gripControl.ReadValueAsObject() is float value ? value : 0f;
                        Debug.Log($"  Grip value: {gripValue}");
                    }

                    // Проверяем кнопки
                    var primaryButton = device.GetChildControl("primaryButton");
                    if (primaryButton != null)
                    {
                        bool primaryPressed = primaryButton.ReadValueAsObject() is bool pressed ? pressed : false;
                        Debug.Log($"  Primary button: {primaryPressed}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error reading controls from {device.name}: {e.Message}");
                }
            }
        }

        if (vrDeviceCount == 0)
        {
            Debug.LogWarning("No VR input devices detected!");
        }
        else
        {
            Debug.Log($"Found {vrDeviceCount} VR input devices");
        }
    }

    // Альтернативный метод с перечислением всех контролов
    [ContextMenu("List All Controls")]
    private void ListAllControls()
    {
        Debug.Log("=== ALL INPUT CONTROLS ===");
        foreach (var device in InputSystem.devices)
        {
            Debug.Log($"Device: {device.name} ({device.layout})");

            foreach (var control in device.allControls)
            {
                string controlType = control.GetType().Name;
                string value = "N/A";

                try
                {
                    var controlValue = control.ReadValueAsObject();
                    value = controlValue?.ToString() ?? "null";
                }
                catch
                {
                    value = "read error";
                }

                Debug.Log($"  {control.name} ({controlType}): {value} [Path: {control.path}]");
            }
        }
    }

    // Упрощенная версия для быстрой проверки
    [ContextMenu("Quick VR Check")]
    private void QuickVRCheck()
    {
        Debug.Log("=== QUICK VR CHECK ===");

        // Проверяем конкретные устройства по известным путям
        string[] devicePaths = {
            "<XRController>{LeftHand}",
            "<XRController>{RightHand}",
            "<OculusTouchController>{LeftHand}",
            "<OculusTouchController>{RightHand}",
            "<OpenXRController>{LeftHand}",
            "<OpenXRController>{RightHand}"
        };

        foreach (string path in devicePaths)
        {
            try
            {
                var device = InputSystem.GetDevice(path);
                if (device != null)
                {
                    Debug.Log($"Found device: {device.name}");

                    // Проверяем основные контролы
                    CheckControl(device, "trigger");
                    CheckControl(device, "grip");
                    CheckControl(device, "primaryButton");
                    CheckControl(device, "secondaryButton");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"Device {path} not found: {e.Message}");
            }
        }
    }

    private void CheckControl(InputDevice device, string controlName)
    {
        try
        {
            var control = device.GetChildControl(controlName);
            if (control != null)
            {
                object value = control.ReadValueAsObject();
                Debug.Log($"  {controlName}: {value}");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"  {controlName}: error - {e.Message}");
        }
    }
}