using System.IO.MemoryMappedFiles;
using System.Threading;

namespace _2DOF
{
    /// <summary>
    /// Этот класс используется для отправки данных
    /// </summary>
    public sealed class SendingData
    {
        /// <summary>
        /// Время ожидания между отправками данных.
        /// В миллисекундах.
        /// </summary>
        public const int WAIT_TIME = 20;

        /// <summary>
        /// Данные телеметрии объекта.
        /// </summary>
        public readonly ObjectTelemetryData ObjectTelemetryData = new();

        private const string MAP_NAME = "2DOFMemoryDataGrabber";
        private Thread _thread;

        /// <summary>
        /// Запуск отправки данных.
        /// </summary>
        public void SendingStart()
        {
            _thread = new Thread(HandlerData);
            _thread.Start();
        }

        /// <summary>
        /// Остановка отправки данных.
        /// </summary>
        public void SendingStop()
        {
            _thread?.Abort();
        }

        private void HandlerData()
        {
            using var memoryMappedFile = MemoryMappedFile.CreateOrOpen(MAP_NAME, ObjectTelemetryData.DataArray.Length);

            while (true)
            {
                using var accessor = memoryMappedFile.CreateViewAccessor();

                accessor.WriteArray(0, ObjectTelemetryData.DataArray, 0, 6);

                Thread.Sleep(WAIT_TIME);
            }
        }
    }
}