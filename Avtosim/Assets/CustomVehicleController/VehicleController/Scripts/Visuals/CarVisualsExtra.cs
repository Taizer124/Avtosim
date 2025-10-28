using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Car Visuals Extra"),
    HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/extra/adding-visual-effects")]
    public class CarVisualsExtra : MonoBehaviour
    {
        [SerializeField]
        private CarVisualsEssentials _carVisualsEssentials;
        [SerializeField]
        private CollisionHandler _collisionHandler;

        [Header("If the CurrentCarStats SO isn't assigned to the controller, \n" +
            "the one that was created in Awake method will be used automatically."), SerializeField, Space, Space]
        private CurrentCarStats _currentCarStats;
        [SerializeField]
        private Rigidbody _rigidbody;

        #region Wheel Meshes
        [SerializeField]
        public VehicleAxle[] _axleArray;
        private WheelController[] _wheelControllerArray;
        private Transform[] _wheelMeshesArray;
        #endregion

        #region Extra Effects
        [Header("Extra Visual Effects")]
        public bool EnableTireSmoke;
        [SerializeField]
        private TireSmokeParameters _tireSmokeParameters;
        private CarVisualsTireSmoke _tireSmoke;
        public void SetTireSmokeEnabled(bool enable) => EnableTireSmoke = enable;

        [Separator]
        public bool EnableTireTrails;
        [SerializeField]
        private TireTrailParameters _tireTrailParameters;
        private CarVisualsTireTrails _tireTrails;
        public void SetTireTrailsEnabled(bool enable) => EnableTireTrails = enable;

        [Separator]
        public bool EnableBrakeLightsEffect;
        [SerializeField]
        private BrakeLightsParameters _brakeLightsParameters;
        private CarVisualsBrakeLights _brakeLightsEffect;
        public void SetBrakeLightsEnabled(bool enable) => EnableBrakeLightsEffect = enable;

        [Separator]
        public bool EnableBrakeDisksGlowEffect;
        [SerializeField]
        private BrakeDisksGlowParameters _brakeDisksGlowParameters;
        private CarVisualBrakeDisksGlow _brakeDisksGlowEffect;
        public void SetBrakeDisksGlowEnabled(bool enable) => EnableBrakeDisksGlowEffect = enable;

        [Separator]
        public bool EnableBodyAeroEffect;
        [SerializeField]
        private EffectTypeParameters _bodyEffectParameters;
        private CarVisualsBodyWindEffect _bodyWindEffect;
        public void SetBodyAeroEnabled(bool enable) => EnableBodyAeroEffect = enable;


        [Separator]
        public bool EnableWingAeroEffect;
        [SerializeField]
        private WingAeroParameters _wingAeroParameters;
        private CarVisualsWingAeroEffect _wingAeroEffect;
        public void SetWingAeroEnabled(bool enable) => EnableWingAeroEffect = enable;


        [Separator]
        public bool EnableAntiLagEffect;
        [SerializeField]
        private AntiLagParameters _antiLagParameters;
        private CarVisualsAntiLag _antiLagEffect;
        public void SetAntiLagEnabled(bool enable) => EnableAntiLagEffect = enable;

        [Separator]
        public bool EnableNitroEffect;
        [SerializeField]
        private NitrousParameters _nitroParameters;
        private CarVisualsNitrous _nitroEffect;
        public void SetNitroEnabled(bool enable) => EnableNitroEffect = enable;

        [Separator]
        public bool EnableCollisionEffects;
        [SerializeField]
        private CollisionParameters _collisionParameters;
        private CarVisualsCollisionEffects _collisionEffects;
        public void SetCollisionEnabled(bool enable) => EnableCollisionEffects = enable;

        [Separator]
        public bool EnableEngineSmokeEffect;
        [SerializeField]
        private EngineSmokeParameters _engineSmokeEffectParameters;
        private CarVisualsEngineSmoke _engineSmokeEffect;
        public void SetEngineSmokeEnabled(bool enable) => EnableCollisionEffects = enable;
        public void EmitEngineSmoke(bool emit) => _engineSmokeEffect?.EmitSmoke(emit);
        #endregion

        private const float DELAY_BEFORE_DISABLING_EFFECTS = 0.33f;
        private float[] _lastStopEmitTimeArray;
        private bool[] _shouldEmitArray;

        private void Start()
        {
            if (_currentCarStats == null)
            {
                if (_carVisualsEssentials == null)
                    Debug.LogError("CurrentCarStats wasn't assigned and couldn't be found");
                else
                    _currentCarStats = _carVisualsEssentials.GetCurrentCarStats();
            }

            _lastStopEmitTimeArray = new float[_axleArray.Length * 2];
            _shouldEmitArray = new bool[_axleArray.Length * 2];

            _wheelControllerArray = VehicleAxle.ExtractVehicleWheelControllerArray(_axleArray);
            _wheelMeshesArray = VehicleAxle.ExtractVehicleWheelVisualTransformArray(_axleArray);

            TryInstantiateExtraEffects();
        }

        private void Reset()
        {
#if !VISUAL_EFFECT_GRAPH_INSTALLED
            _tireSmokeParameters.VisualEffect.VisualEffectType = VisualEffectAssetType.Type.ParticleSystem;
            _bodyEffectParameters.VisualEffectType = VisualEffectAssetType.Type.ParticleSystem;
            _antiLagParameters.VisualEffect.VisualEffectType = VisualEffectAssetType.Type.ParticleSystem;
            _nitroParameters.VisualEffect.VisualEffectType = VisualEffectAssetType.Type.ParticleSystem;
#endif
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

        private void TryInstantiateExtraEffects()
        {
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
                    _tireSmoke.HandleSmokeEffects(false, i,
                        velocityNormalized, _currentCarStats.SpeedInMsPerS);
                    continue;
                }

                if (_shouldEmitArray[i])
                {
                    _tireSmoke.HandleSmokeEffects(true, i,
                        velocityNormalized, _currentCarStats.SpeedInMsPerS);
                }
                else
                {
                    bool display = Time.time < _lastStopEmitTimeArray[i] + DELAY_BEFORE_DISABLING_EFFECTS;
                    _tireSmoke.HandleSmokeEffects(display, i,
                        velocityNormalized, _currentCarStats.SpeedInMsPerS);
                }
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

                if (_shouldEmitArray[i])
                {
                    _tireTrails.DisplayTireTrail(true, i);

                }
                else
                {
                    bool display = Time.time < _lastStopEmitTimeArray[i] + DELAY_BEFORE_DISABLING_EFFECTS;
                    _tireTrails.DisplayTireTrail(display, i);
                }
            }
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

        private void OnDestroy()
        {
            if (EnableAntiLagEffect && _antiLagEffect != null)
                _antiLagEffect.OnDestroy();
        }
    }
}

