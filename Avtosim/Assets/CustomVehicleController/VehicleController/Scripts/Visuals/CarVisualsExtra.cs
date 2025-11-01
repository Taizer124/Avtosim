using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Car Visuals Extra"),
    HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/extra/adding-visual-effects")]
    public class CarVisualsExtra : MonoBehaviour
    {
        [SerializeField] private CarVisualsEssentials _carVisualsEssentials;
        [SerializeField] private CollisionHandler _collisionHandler;
        [SerializeField] private CurrentCarStats _currentCarStats;
        [SerializeField] private Rigidbody _rigidbody;

        [Header("Wheel Settings")]
        [SerializeField] public VehicleAxle[] _axleArray;
        private WheelController[] _wheelControllerArray;
        private Transform[] _wheelMeshesArray;

        #region Extra Effects
        [Header("Extra Visual Effects")]
        public bool EnableTireSmoke;
        [SerializeField] private TireSmokeParameters _tireSmokeParameters;
        private CarVisualsTireSmoke _tireSmoke;

        public bool EnableTireTrails;
        [SerializeField] private TireTrailParameters _tireTrailParameters;
        private CarVisualsTireTrails _tireTrails;

        public bool EnableBrakeLightsEffect;
        [SerializeField] private BrakeLightsParameters _brakeLightsParameters;
        private CarVisualsBrakeLights _brakeLightsEffect;

        public bool EnableBrakeDisksGlowEffect;
        [SerializeField] private BrakeDisksGlowParameters _brakeDisksGlowParameters;
        private CarVisualBrakeDisksGlow _brakeDisksGlowEffect;

        public bool EnableBodyAeroEffect;
        [SerializeField] private EffectTypeParameters _bodyEffectParameters;
        private CarVisualsBodyWindEffect _bodyWindEffect;

        public bool EnableWingAeroEffect;
        [SerializeField] private WingAeroParameters _wingAeroParameters;
        private CarVisualsWingAeroEffect _wingAeroEffect;

        public bool EnableAntiLagEffect;
        [SerializeField] private AntiLagParameters _antiLagParameters;
        private CarVisualsAntiLag _antiLagEffect;

        public bool EnableNitroEffect;
        [SerializeField] private NitrousParameters _nitroParameters;
        private CarVisualsNitrous _nitroEffect;

        public bool EnableCollisionEffects;
        [SerializeField] private CollisionParameters _collisionParameters;
        private CarVisualsCollisionEffects _collisionEffects;

        public bool EnableEngineSmokeEffect;
        [SerializeField] private EngineSmokeParameters _engineSmokeEffectParameters;
        private CarVisualsEngineSmoke _engineSmokeEffect;
        #endregion

        private const float DELAY_BEFORE_DISABLING_EFFECTS = 0.33f;
        private float[] _lastStopEmitTimeArray;
        private bool[] _shouldEmitArray;

        private void Start()
        {
            if (_currentCarStats == null)
            {
                if (_carVisualsEssentials == null)
                    Debug.LogError("CarVisualsEssentials not assigned!");
                else
                    _currentCarStats = _carVisualsEssentials.GetCurrentCarStats();
            }

            _wheelControllerArray = VehicleAxle.ExtractVehicleWheelControllerArray(_axleArray);
            _wheelMeshesArray = VehicleAxle.ExtractVehicleWheelVisualTransformArray(_axleArray);

            _lastStopEmitTimeArray = new float[_axleArray.Length * 2];
            _shouldEmitArray = new bool[_axleArray.Length * 2];

            TryInstantiateExtraEffects();
        }
        private void OnEnable()
        {
            // При повторной активации пересоздаём все визуальные эффекты
            if (_wheelControllerArray != null && _wheelMeshesArray != null)
            {
                TryInstantiateExtraEffects();
            }
        }

        private void OnDisable()
        {
            DestroyAllEffects();
        }

        private void OnDestroy()
        {
            DestroyAllEffects();
        }

        private void TryInstantiateExtraEffects()
        {
            DestroyAllEffects(); // гарантируем, что старые эффекты удалены

            if (EnableTireSmoke)
                _tireSmoke = new(_wheelMeshesArray, _wheelControllerArray, transform, _tireSmokeParameters);

            if (EnableTireTrails)
                _tireTrails = new(_wheelMeshesArray, _wheelControllerArray, _tireTrailParameters);

            if (EnableAntiLagEffect)
                _antiLagEffect = new(this, _currentCarStats, _antiLagParameters);

            if (EnableNitroEffect)
                _nitroEffect = new(_nitroParameters, _currentCarStats);

            if (EnableBrakeLightsEffect)
                _brakeLightsEffect = new(_brakeLightsParameters);

            if (EnableBrakeDisksGlowEffect)
                _brakeDisksGlowEffect = new(_brakeDisksGlowParameters, _currentCarStats);

            if (EnableBodyAeroEffect)
                _bodyWindEffect = new(_bodyEffectParameters, transform);

            if (EnableWingAeroEffect)
                _wingAeroEffect = new(_wingAeroParameters);

            if (_collisionHandler != null && EnableCollisionEffects)
                _collisionEffects = new(_collisionHandler, _collisionParameters, transform);

            if (EnableEngineSmokeEffect)
                _engineSmokeEffect = new(_engineSmokeEffectParameters);
        }

        private void LateUpdate()
        {
            if (EnableTireSmoke || EnableTireTrails)
                ShouldEmitWheelEffects();

            if (EnableTireSmoke)
                DisplaySmokeEffects();

            if (EnableTireTrails)
                DisplaySkidMarksEffects();

            if (EnableNitroEffect)
                _nitroEffect.HandleNitroEffect();

            if (EnableBodyAeroEffect)
                _bodyWindEffect.HandleSpeedEffect(_currentCarStats.SpeedInMsPerS, _rigidbody.linearVelocity);

            if (EnableWingAeroEffect)
                _wingAeroEffect.HandleWingAeroEffect(_currentCarStats.SpeedInMsPerS);
            else if (_wingAeroEffect != null)
                _wingAeroEffect.Disable();

            if (EnableBrakeLightsEffect)
                _brakeLightsEffect.HandleRearLights(_currentCarStats.Braking);

            if (EnableBrakeDisksGlowEffect)
                _brakeDisksGlowEffect.HandleBrakeDisksGlow();
        }

        private void ShouldEmitWheelEffects()
        {
            for (int i = 0; i < _axleArray.Length * 2; i++)
            {
                if (_currentCarStats.WheelSlipArray[i])
                {
                    _shouldEmitArray[i] = true;
                    _lastStopEmitTimeArray[i] = Time.time;
                }
                else
                {
                    _shouldEmitArray[i] = false;
                }
            }
        }

        private void DisplaySmokeEffects()
        {
            Vector3 velocityNormalized = _rigidbody.linearVelocity.normalized;
            for (int i = 0; i < _wheelMeshesArray.Length; i++)
            {
                if (!_wheelControllerArray[i].HasContactWithGround)
                {
                    _tireSmoke.HandleSmokeEffects(false, i, velocityNormalized, _currentCarStats.SpeedInMsPerS);
                    continue;
                }

                bool display = _shouldEmitArray[i] ||
                    Time.time < _lastStopEmitTimeArray[i] + DELAY_BEFORE_DISABLING_EFFECTS;

                _tireSmoke.HandleSmokeEffects(display, i, velocityNormalized, _currentCarStats.SpeedInMsPerS);
            }
        }

        private void DisplaySkidMarksEffects()
        {
            for (int i = 0; i < _wheelMeshesArray.Length; i++)
            {
                if (!_wheelControllerArray[i].HasContactWithGround)
                {
                    _tireTrails.DisplayTireTrail(false, i);
                    continue;
                }

                bool display = _shouldEmitArray[i] ||
                    Time.time < _lastStopEmitTimeArray[i] + DELAY_BEFORE_DISABLING_EFFECTS;

                _tireTrails.DisplayTireTrail(display, i);
            }
        }

        private void DestroyAllEffects()
        {
            // Уничтожаем коллизионные эффекты с отпиской
            if (_collisionEffects != null)
            {
                _collisionEffects.Destroy();
                _collisionEffects = null;
            }

            // Сбрасываем остальные эффекты
            _tireSmoke = null;
            _tireTrails = null;
            _brakeLightsEffect = null;
            _brakeDisksGlowEffect = null;
            _bodyWindEffect = null;
            _wingAeroEffect = null;
            _antiLagEffect = null;
            _nitroEffect = null;
            _engineSmokeEffect = null;
        }

        public void CopyValuesFromEssentials()
        {
            if (_carVisualsEssentials == null)
            {
                Debug.LogError("CarVisualsEssentials is not assigned");
                return;
            }

            _axleArray = _carVisualsEssentials.GetAxleArray();
            _currentCarStats = _carVisualsEssentials.GetCurrentCarStats();
            _rigidbody = _carVisualsEssentials.GetRigidbody();
        }
    }
}
