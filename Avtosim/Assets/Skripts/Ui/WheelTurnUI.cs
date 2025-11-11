using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LogitechG29.Sample.Input;

public class WheelUINavigation : MonoBehaviour
{
    [Header("Input Source")]
    [SerializeField] private InputControllerReader _inputReader;

    [Header("Navigation Colors")]
    [SerializeField] private Color _selectedColor = Color.cyan;
    [SerializeField] private Color _normalColor = Color.white;

    [Header("Slider Settings")]
    [SerializeField] private float _sliderSpeed = 30f; // оптимально для 0–100
    [SerializeField] private float _deadZone = 0.05f;

    private List<Selectable> _menuElements = new List<Selectable>();
    private int _currentIndex = 0;
    private bool _returnHeld = false;
    private Transform _currentPanelRoot;

    private void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.OnLeftTurnCallback += OnLeftTurn;
            _inputReader.OnRightTurnCallback += OnRightTurn;
            _inputReader.OnReturnCallback += OnReturn;
        }
        else
        {
            Debug.LogWarning("InputControllerReader not assigned — используем эмуляцию через мышь.");
        }

        if (_menuElements.Count > 0)
            HighlightSelected();
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.OnLeftTurnCallback -= OnLeftTurn;
            _inputReader.OnRightTurnCallback -= OnRightTurn;
            _inputReader.OnReturnCallback -= OnReturn;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Эмуляция через колесико мыши
        if (Input.mouseScrollDelta.y > 0f) OnRightTurn(true);
        if (Input.mouseScrollDelta.y < 0f) OnLeftTurn(true);
        if (Input.GetMouseButtonDown(2)) OnReturn(true);
        if (Input.GetMouseButtonUp(2)) OnReturn(false);
#endif
    }

    private void OnLeftTurn(bool pressed)
    {
        if (!pressed || _returnHeld || _menuElements.Count == 0) return;

        _currentIndex = Mathf.Max(0, _currentIndex - 1);
        HighlightSelected();
    }

    private void OnRightTurn(bool pressed)
    {
        if (!pressed || _returnHeld || _menuElements.Count == 0) return;

        _currentIndex = Mathf.Min(_menuElements.Count - 1, _currentIndex + 1);
        HighlightSelected();
    }

    private void OnReturn(bool pressed)
    {
        _returnHeld = pressed;

        if (!pressed || _menuElements.Count == 0)
            return;

        var current = _menuElements[_currentIndex];
        if (current == null) return;

        // === Если это слайдер ===
        var slider = current.GetComponent<Slider>();
        if (slider != null)
        {
            StopAllCoroutines();
            StartCoroutine(AdjustSlider(slider));
            return;
        }

        // === Если это кнопка ===
        var button = current.GetComponent<Button>();
        if (button != null)
        {
            button.onClick?.Invoke();
            return;
        }
    }


    private IEnumerator AdjustSlider(Slider slider)
    {
        while (true)
        {
            bool returnState = _returnHeld;

            // Дополнительно проверяем напрямую InputReader.Return
            if (_inputReader != null)
                returnState |= _inputReader.Return;

            if (!returnState)
                break;

            float steer = 0f;
            if (_inputReader != null)
                steer = _inputReader.Steering;

#if UNITY_EDITOR
            steer += Input.mouseScrollDelta.y;
#endif
            if (Mathf.Abs(steer) < _deadZone)
                steer = 0f;

            if (steer != 0f)
            {
                float delta = steer * _sliderSpeed * Time.unscaledDeltaTime;
                slider.value = Mathf.Clamp(slider.value + delta, slider.minValue, slider.maxValue);
            }

            yield return null;
        }
    }


    private void HighlightSelected()
    {
        for (int i = 0; i < _menuElements.Count; i++)
        {
            var selectable = _menuElements[i];
            if (selectable == null) continue;

            // === Для Slider выделяем именно Handle ===
            var slider = selectable.GetComponent<Slider>();
            if (slider != null && slider.handleRect != null)
            {
                var handleImage = slider.handleRect.GetComponent<Image>();
                if (handleImage != null)
                    handleImage.color = (i == _currentIndex) ? _selectedColor : _normalColor;

                // Дополнительно сбрасываем цвет фона
                var bgImage = slider.GetComponent<Image>();
                if (bgImage != null)
                    bgImage.color = _normalColor;

                continue;
            }

            // === Для кнопок и других Selectable ===
            var img = selectable.GetComponent<Image>();
            if (img == null)
                img = selectable.GetComponentInChildren<Image>();

            if (img != null)
                img.color = (i == _currentIndex) ? _selectedColor : _normalColor;
        }

        if (_menuElements.Count > 0)
            _menuElements[_currentIndex].Select();
    }

    public void SetActivePanel(Transform newPanel)
    {
        _currentPanelRoot = newPanel;
        _menuElements.Clear();

        foreach (var selectable in newPanel.GetComponentsInChildren<Selectable>(true))
        {
            if (selectable.gameObject.activeInHierarchy)
                _menuElements.Add(selectable);
        }

        _currentIndex = 0;
        HighlightSelected();
    }
}
