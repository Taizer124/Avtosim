using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Ставит world-space меню снимком перед камерой ОДИН раз при открытии, а не
    /// каждый кадр. Именно поэтому меню не «трясётся за головой» в VR — это
    /// решение той проблемы, что была с в притык прикреплённым к лицу Canvas
    /// (он не успевал за движением камеры). После снимка меню статично в мире:
    /// игрок может спокойно рассмотреть его и повернуть голову.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/World Space Menu Placer")]
    public class WorldSpaceMenuPlacer : MonoBehaviour
    {
        [Tooltip("Камера, перед которой ставится меню. Пусто — Camera.main.")]
        [SerializeField] private Camera _camera;
        [Tooltip("Расстояние от камеры до меню, метры.")]
        [SerializeField] private float _distance = 1.8f;
        [Tooltip("Держать меню вертикально (игнорировать наклон головы), чтобы текст не заваливался.")]
        [SerializeField] private bool _keepUpright = true;

        public void PlaceInFrontOfCamera()
        {
            Camera cam = _camera != null ? _camera : Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[WorldSpaceMenuPlacer] Камера не найдена — меню осталось на месте.");
                return;
            }

            Transform camT = cam.transform;
            Vector3 forward = camT.forward;
            if (_keepUpright)
            {
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f)
                    forward = camT.forward; // камера смотрит строго вверх/вниз — берём как есть
                forward.Normalize();
            }

            transform.position = camT.position + forward * _distance;
            transform.rotation = Quaternion.LookRotation(_keepUpright ? forward : camT.forward, Vector3.up);
        }
    }
}
