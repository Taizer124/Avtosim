using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Позволяет ПК-игроку осматриваться в сцене меню экранными стрелками
    /// (мышью), тогда как VR-игрок просто крутит головой физически.
    ///
    /// Вращает назначенный transform камеры (yaw/pitch). В VR за поворот
    /// отвечает TrackedPoseDriver шлема — он перезаписывает поворот камеры
    /// каждый кадр, поэтому наши записи в VR фактически игнорируются (то, что
    /// и нужно: VR-обзор головой не ломается). На ПК без шлема драйвер позы
    /// ничего не пишет, и стрелки крутят вид.
    ///
    /// Экранные стрелки — обычные UGUI-элементы: повесь методы Look*Down на
    /// EventTrigger → PointerDown, а Look*Up на PointerUp (удержание). Либо
    /// используй Nudge* на обычном Button.OnClick (поворот шагом за клик).
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Camera Arrow Look")]
    public class CameraArrowLook : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Камера/transform для поворота. Если пусто — берётся Camera.main.")]
        [SerializeField] private Transform _cameraToRotate;

        [Header("Speed (hold)")]
        [SerializeField] private float _yawSpeed = 90f;   // град/сек
        [SerializeField] private float _pitchSpeed = 60f; // град/сек

        [Header("Nudge (per click)")]
        [SerializeField] private float _nudgeDegrees = 15f;

        [Header("Pitch clamp")]
        [SerializeField] private float _minPitch = -40f;
        [SerializeField] private float _maxPitch = 40f;

        [Header("Extra")]
        [Tooltip("Разрешить также реальные стрелки клавиатуры (для ПК).")]
        [SerializeField] private bool _allowKeyboardArrows = true;

        private float _yawDir;   // -1..1, задаётся удержанием стрелок
        private float _pitchDir; // -1..1
        private float _yaw;
        private float _pitch;

        private void Start()
        {
            if (_cameraToRotate == null && Camera.main != null)
                _cameraToRotate = Camera.main.transform;

            if (_cameraToRotate != null)
            {
                Vector3 e = _cameraToRotate.localEulerAngles;
                _yaw = e.y;
                _pitch = NormalizeAngle(e.x);
            }
        }

        // --- Удержание (EventTrigger PointerDown/PointerUp) ---
        public void LookLeftDown() => _yawDir = -1f;
        public void LookRightDown() => _yawDir = +1f;
        public void LookHorizontalUp() => _yawDir = 0f;
        public void LookUpDown() => _pitchDir = -1f;   // вверх = отрицательный X в Unity
        public void LookDownDown() => _pitchDir = +1f;
        public void LookVerticalUp() => _pitchDir = 0f;

        // --- Шаг за клик (Button.OnClick) ---
        public void NudgeLeft() => ApplyYaw(-_nudgeDegrees);
        public void NudgeRight() => ApplyYaw(+_nudgeDegrees);
        public void NudgeUp() => ApplyPitch(-_nudgeDegrees);
        public void NudgeDown() => ApplyPitch(+_nudgeDegrees);

        private void Update()
        {
            if (_cameraToRotate == null)
                return;

            float yd = _yawDir;
            float pd = _pitchDir;

            if (_allowKeyboardArrows)
            {
                if (Input.GetKey(KeyCode.LeftArrow)) yd = -1f;
                if (Input.GetKey(KeyCode.RightArrow)) yd = +1f;
                if (Input.GetKey(KeyCode.UpArrow)) pd = -1f;
                if (Input.GetKey(KeyCode.DownArrow)) pd = +1f;
            }

            if (yd != 0f)
                ApplyYaw(yd * _yawSpeed * Time.unscaledDeltaTime);
            if (pd != 0f)
                ApplyPitch(pd * _pitchSpeed * Time.unscaledDeltaTime);
        }

        private void ApplyYaw(float delta)
        {
            _yaw += delta;
            ApplyRotation();
        }

        private void ApplyPitch(float delta)
        {
            _pitch = Mathf.Clamp(_pitch + delta, _minPitch, _maxPitch);
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            _cameraToRotate.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private static float NormalizeAngle(float a) => a > 180f ? a - 360f : a;
    }
}
