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
    [SerializeField] private float _sliderSpeed = 0.5f;

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
            Debug.LogWarning("InputControllerReader not assigned Ч используем эмул€цию через мышь.");
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
        // === Ёмул€ци€ через мышь (дл€ тестов в Unity без рул€) ===
#if UNITY_EDITOR
        if (Input.mouseScrollDelta.y > 0f) OnRightTurn(true);
        if (Input.mouseScrollDelta.y < 0f) OnLeftTurn(true);
        if (Input.GetMouseButtonDown(2)) OnReturn(true);
        if (Input.GetMouseButtonUp(2)) OnReturn(false);
#endif
    }

    private void OnLeftTurn(bool pressed)
    {
        if (!pressed || _returnHeld || _menuElements.Count == 0) return;

        if (_currentIndex > 0)
        {
            _currentIndex--;
            HighlightSelected();
        }
    }

    private void OnRightTurn(bool pressed)
    {
        if (!pressed || _returnHeld || _menuElements.Count == 0) return;

        if (_currentIndex < _menuElements.Count - 1)
        {
            _currentIndex++;
            HighlightSelected();
        }
    }

    private void OnReturn(bool pressed)
    {
        _returnHeld = pressed;

        if (_menuElements.Count == 0) return;

        var current = _menuElements[_currentIndex];
        if (current == null) return;

        if (pressed)
        {
            // === ≈сли это слайдер ===
            var slider = current.GetComponent<Slider>();
            if (slider != null)
            {
                StartCoroutine(AdjustSlider(slider));
                return;
            }

            // === ≈сли это кнопка ===
            var button = current.GetComponent<Button>();
            if (button != null)
            {
                button.onClick?.Invoke();
                return;
            }
        }
    }

    private IEnumerator AdjustSlider(Slider slider)
    {
        while (_returnHeld)
        {
            float steer = 0f;

            // если есть реальный руль
            if (_inputReader != null)
                steer = _inputReader.Steering;
#if UNITY_EDITOR
            // в редакторе можно регулировать колЄсиком
            steer += Input.mouseScrollDelta.y;
#endif

            if (Mathf.Abs(steer) > 0.01f)
                slider.value = Mathf.Clamp01(slider.value + steer * _sliderSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private void HighlightSelected()
    {
        for (int i = 0; i < _menuElements.Count; i++)
        {
            var img = _menuElements[i].GetComponent<Image>();
            if (img != null)
                img.color = i == _currentIndex ? _selectedColor : _normalColor;
        }

        // ¬ыделение фокуса в Unity UI
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
