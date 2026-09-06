using System.Collections.Generic;
using Assets.VehicleController;
using UnityEngine;

/// <summary>
/// Снимает движение платформы с ПОДВЕСКИ, а не с ускорений кузова.
///
/// Зачем: ускорение центра масс не отличает наезд одним колесом на бордюр от
/// проезда по ровной дороге — кузов в обоих случаях почти не разгоняется
/// вертикально. А водителю нужно почувствовать именно перекос: одно колесо
/// выше остальных. Такой перекос виден только по ходу отдельных стоек.
///
/// Что считается:
///   Roll  — разность сжатия левого и правого бортов (наезд бортом на бордюр);
///   Pitch — разность сжатия передней и задней осей (въезд/съезд);
///   Heave — общее сжатие всех стоек (подброс, приземление, лежачий полицейский).
///
/// Абсолютную длину пружины не калибруем: у разных машин и пресетов подвески
/// свои величины. Вместо этого для каждой стойки ведём медленно плывущую
/// базовую линию и работаем с ОТКЛОНЕНИЕМ от неё. Это самокалибрующийся
/// подход: он одинаково работает на любой машине и не зависит от того, что
/// именно включено в CurrentSpringLengthPlusGroundOffset.
/// </summary>
public class SuspensionMotionSampler
{
    private class Corner
    {
        public SuspensionController Suspension;
        public bool IsLeft;
        public bool IsFront;
        public float Baseline;
        public bool BaselineReady;
    }

    private readonly List<Corner> _corners = new List<Corner>();
    private float _travelLength = 1f;

    /// <summary>Нашлись ли стойки. Если нет — все каналы отдают ноль.</summary>
    public bool IsValid => _corners.Count > 0;

    /// <summary>Перекос вбок, -1..1. Положительное — левый борт приподнят.</summary>
    public float Roll { get; private set; }

    /// <summary>Перекос вперёд-назад, -1..1. Положительное — передняя ось приподнята.</summary>
    public float Pitch { get; private set; }

    /// <summary>Общий подъём, -1..1. Положительное — кузов поджат вверх.</summary>
    public float Heave { get; private set; }

    /// <summary>Скорость изменения подъёма, 1/с. Даёт резкий толчок на кромке бордюра.</summary>
    public float HeaveRate { get; private set; }

    private float _previousHeave;

    /// <summary>
    /// Постоянная времени базовой линии, с. Должна быть заметно больше
    /// длительности неровности (доли секунды), иначе базовая линия «догонит»
    /// бордюр и перекос исчезнет прямо во время наезда.
    /// </summary>
    private const float BaselineTau = 2.0f;

    /// <summary>
    /// Находит стойки среди потомков и раскладывает их по углам машины.
    /// Сторона определяется по положению стойки в системе координат кузова,
    /// поэтому ручная разметка в инспекторе не нужна — работает на любом
    /// префабе машины.
    /// </summary>
    public void Initialize(Transform body)
    {
        _corners.Clear();

        if (body == null)
        {
            return;
        }

        var found = body.GetComponentsInChildren<SuspensionController>(true);

        foreach (var suspension in found)
        {
            Vector3 local = body.InverseTransformPoint(suspension.transform.position);

            _corners.Add(new Corner
            {
                Suspension = suspension,
                IsLeft = local.x < 0f,
                IsFront = local.z > 0f,
            });

            if (suspension.SpringTravelLength > 0.001f)
            {
                _travelLength = suspension.SpringTravelLength;
            }
        }
    }

    /// <summary>Обновляет каналы. Вызывать из FixedUpdate.</summary>
    public void Sample(float dt)
    {
        if (_corners.Count == 0 || dt <= 0f)
        {
            Roll = Pitch = Heave = HeaveRate = 0f;
            return;
        }

        float left = 0f, right = 0f, front = 0f, rear = 0f, all = 0f;
        int leftCount = 0, rightCount = 0, frontCount = 0, rearCount = 0;

        // Коэффициент экспоненциального сглаживания, не зависящий от частоты кадров.
        float baselineAlpha = 1f - Mathf.Exp(-dt / BaselineTau);

        foreach (var corner in _corners)
        {
            if (corner.Suspension == null)
            {
                continue;
            }

            float length = corner.Suspension.CurrentSpringLengthPlusGroundOffset;

            if (!corner.BaselineReady)
            {
                corner.Baseline = length;
                corner.BaselineReady = true;
            }

            // Пружина короче обычного — колесо поджато вверх, этот угол кузова
            // приподнят. Именно это и происходит при наезде на бордюр.
            float deviation = (corner.Baseline - length) / _travelLength;
            deviation = Mathf.Clamp(deviation, -1f, 1f);

            corner.Baseline = Mathf.Lerp(corner.Baseline, length, baselineAlpha);

            all += deviation;

            if (corner.IsLeft) { left += deviation; leftCount++; }
            else { right += deviation; rightCount++; }

            if (corner.IsFront) { front += deviation; frontCount++; }
            else { rear += deviation; rearCount++; }
        }

        float leftAvg = leftCount > 0 ? left / leftCount : 0f;
        float rightAvg = rightCount > 0 ? right / rightCount : 0f;
        float frontAvg = frontCount > 0 ? front / frontCount : 0f;
        float rearAvg = rearCount > 0 ? rear / rearCount : 0f;

        Roll = Mathf.Clamp(leftAvg - rightAvg, -1f, 1f);
        Pitch = Mathf.Clamp(frontAvg - rearAvg, -1f, 1f);

        float heave = Mathf.Clamp(all / _corners.Count, -1f, 1f);
        HeaveRate = (heave - _previousHeave) / dt;
        _previousHeave = heave;
        Heave = heave;
    }
}
