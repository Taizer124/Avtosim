using UnityEngine;

namespace Assets.VehicleController
{
    // 3D-модель руля в кабине (не UI) — крутится строго по нормализованному
    // вводу руля (-1..1) из AllInOneInputProvider, без сглаживания/инерции,
    // чтобы поворот совпадал с реальным физическим рулём 1-в-1 по
    // интенсивности: сколько градусов повернул реальный руль, столько же
    // (в масштабе _wheelRotationRangeDegrees) поворачивается модель.
    [AddComponentMenu("CustomVehicleController/Input/Cockpit Steering Wheel")]
    public class CockpitSteeringWheel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _wheelTransform;
        [SerializeField] private AllInOneInputProvider _inputProvider;

        [Header("Rotation Settings")]
        [Tooltip("Локальная ось, вокруг которой крутится модель руля (обычно вперёд/forward для баранки, лежащей в плоскости XY).")]
        [SerializeField] private Vector3 _rotationAxis = Vector3.forward;

        // Знак угла поворота зависит от того, как именно смоделирован и
        // ориентирован в пространстве 3D-объект руля (и от выбранной оси
        // выше) — заранее это не определить программно. Если поворот
        // оказался зеркальным (жмёшь влево — крутится вправо), просто
        // включи этот флажок вместо того, чтобы менять ось или знак ввода.
        [SerializeField] private bool _invertDirection = false;

        // Ограничение скорости ТОЛЬКО визуального поворота модели (град/сек).
        // Клавиатура даёт мгновенные -1/0/1 (без промежуточных значений), из-за
        // чего руль дёргался бы сразу на весь угол — это сглаживает именно
        // анимацию, не трогая сам ввод/физику. Для реального руля значение
        // не мешает: человек физически не крутит быстрее этого предела, так
        // что визуал остаётся 1-в-1. Подобрано с запасом — можно уменьшить,
        // если хочется более плавную (инерционную) анимацию.
        [SerializeField] private float _visualRotationSpeed = 1800f;

        // Исходный localRotation модели на старте (как выставил в редакторе,
        // по всем осям) — поворот руля применяется ПОВЕРХ него, а не вместо,
        // чтобы не сбрасывать ручную настройку наклона/позы модели.
        private Quaternion _baseRotation;
        private float _currentAngle;

        private void Start()
        {
            if (_inputProvider == null)
                _inputProvider = FindFirstObjectByType<AllInOneInputProvider>();

            if (_wheelTransform == null)
                _wheelTransform = transform;

            _baseRotation = _wheelTransform.localRotation;
        }

        private void Update()
        {
            if (_wheelTransform == null)
                return;

            if (_inputProvider == null)
            {
                _inputProvider = FindFirstObjectByType<AllInOneInputProvider>();
                if (_inputProvider == null) return;
            }

            float steer = _inputProvider.GetHorizontalInput();
            if (_invertDirection)
                steer = -steer;

            float rangeDegrees = _inputProvider.GetWheelRotationRangeDegrees();
            float targetAngle = steer * (rangeDegrees / 2f);

            _currentAngle = Mathf.MoveTowards(_currentAngle, targetAngle, _visualRotationSpeed * Time.deltaTime);

            _wheelTransform.localRotation = _baseRotation * Quaternion.AngleAxis(_currentAngle, _rotationAxis);
        }
    }
}
