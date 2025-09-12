using UnityEngine;

namespace _2DOF
{
    /// <summary>
    /// Данные телеметрии объекта.
    /// </summary>
    public class ObjectTelemetryData
    {
        /// <summary>
        /// Угл поворота
        /// </summary>
        public Vector3 Angles { get; set; }

        /// <summary>
        /// Cкорость
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Массив данных.
        /// </summary>
        public double[] DataArray => new[]
        {
            (double)
            Angles.x,
            Angles.y,
            Angles.z,
            Velocity.x,
            Velocity.y,
            Velocity.z
        };

        /// <summary>
        /// Сброс данных.
        /// </summary>
        public void Reset()
        {
            Angles = Vector3.zero;
            Velocity = Vector3.zero;
        }

        /// <summary>
        /// Преобразование в строку.
        /// </summary>
        public override string ToString() => $"Angles: {Angles}, Velocity: {Velocity}";
    }
}