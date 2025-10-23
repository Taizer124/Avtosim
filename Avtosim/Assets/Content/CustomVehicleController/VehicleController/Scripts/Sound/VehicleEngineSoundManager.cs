using UnityEngine;
using UnityEngine.Audio;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Sound/Vehicle Engine Sound Manager"),
    HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/extra/adding-sound-effects/adding-engine-sound")]
    public class VehicleEngineSoundManager : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        [SerializeField]
        public CarEngineSoundSO _engineSoundsSO;

        [SerializeField, Space, Header(" Optional fields")]
        private AudioMixerGroup _vehicleSoundAudioMixerGroup;
        public float EngineSoundPitch = 1;


        private AudioSource[] _engineAudioSources;


        [SerializeField, Tooltip("Keep the amount of active Audio Sources at minumum.")]
        private bool _optimizeAudioPerformance = true;

        [Separator]
        public bool EngineModificationsAffectPitch = true;

        [SerializeField, Separator]
        private bool _3DSound;

        [SerializeField, Range(0, 1f)]
        private float _spatialBlend = 0;

        [SerializeField, Range(0, 5f)]
        private float _dopplerLevel = 1;

        [SerializeField, Range(0, 360)]
        private int _spread = 0;

        [SerializeField]
        private AudioRolloffMode _volumeRolloff;

        [SerializeField]
        private float _minDistance = 1;
        [SerializeField]
        private float _maxDistance = 500;

        private GameObject _engineAudioHolder;
        private bool _engineSoundInitialized = false;

        private float _lastRPM = 0;

        private void Start()
        {
            if (_engineSoundsSO == null)
            {
                Debug.LogWarning("No car sound SO assigned on " + gameObject.name);
                return;
            }
            if (_vehicleController == null)
            {
                Debug.LogWarning("No Vehicle Controller assigned on " + gameObject.name);
                return;
            }

            InitializeEngineSound();
        }
        private void Update()
        {
            if (_engineSoundInitialized)
                HandleEngineSound();
        }

        public void SetNewCarEngineSoundSO(CarEngineSoundSO engineSoundSO)
        {
            if (engineSoundSO == null)
                return;

            if (engineSoundSO == _engineSoundsSO)
                return;

            int size = engineSoundSO.EngineRPMRangeArray.Length;

            _engineSoundsSO = engineSoundSO;

            if (_engineAudioSources != null)
            {
                    Destroy(_engineAudioHolder);
                    InitializeEngineSound();
            }
            else
            {
                InitializeEngineSound();
                return;
            }
        }

        private void InitializeEngineSound()
        {
            if (_engineSoundsSO == null)
                return;

            if (_engineSoundsSO.EngineRPMRangeArray.Length == 0)
                return;

            _engineAudioHolder = new("EngineAudioHolder");
            _engineAudioHolder.transform.parent = transform;
            _engineAudioHolder.transform.localPosition = new(0, 0, 0);
            _engineAudioSources = new AudioSource[_engineSoundsSO.EngineRPMRangeArray.Length];
            int size = _engineAudioSources.Length;
            for (int i = 0; i < size; i++)
            {
                CreateEngineAudioSource(i, _engineAudioHolder);
            }

            _engineSoundInitialized = true;
        }

        private void CreateEngineAudioSource(int i, GameObject parent)
        {
            _engineAudioSources[i] = parent.AddComponent<AudioSource>();
            _engineAudioSources[i].clip = _engineSoundsSO.EngineRPMRangeArray[i];
            _engineAudioSources[i].outputAudioMixerGroup = _vehicleSoundAudioMixerGroup;
            _engineAudioSources[i].volume = 0;
            _engineAudioSources[i].loop = true;
            _engineAudioSources[i].Play();

            if (_3DSound)
            {
                _engineAudioSources[i].spatialBlend = 1;
            }
        }

        private void HandleEngineSound()
        {
            int size = _engineAudioSources.Length;

            var stats = _vehicleController.GetCurrentCarStats();

            float minRPM = stats.MinRPM;
            float currentEngineRPM = stats.EngineRPM;
            float engineRPMRangeChangeMultiplier = stats.EngineMaxRPMChangeMultiplier;

            float rpmStep = _engineSoundsSO.RPMStep;
            float doubleRpmStep = _engineSoundsSO.RPMStep * 2;
            for (int i = 0; i < size; i++)
            {
                AudioSource source = _engineAudioSources[i];

                float prefferedRPM = i * rpmStep * engineRPMRangeChangeMultiplier + minRPM;

                float rpmDifference = prefferedRPM - currentEngineRPM;

                if (rpmDifference <= doubleRpmStep && rpmDifference >= -doubleRpmStep)
                {
                    source.volume = Mathf.Clamp01(rpmStep / (Mathf.Abs(rpmDifference) + rpmStep) * engineRPMRangeChangeMultiplier);

                    float pitch = currentEngineRPM / prefferedRPM * EngineSoundPitch;

                    if (EngineModificationsAffectPitch)
                        pitch *= engineRPMRangeChangeMultiplier;

                    source.pitch = pitch > 0 ? pitch : 0;

                    UpdateAudioSourceSettings(source);
                }
                else
                    source.volume *= 0.8f;
            }

            if (!_optimizeAudioPerformance)
            {
                for (int i = 0; i < size; i++)              
                    _engineAudioSources[i].enabled = true;
                
                return;
            }

            float rpmChangeRate = Mathf.Abs(_lastRPM - currentEngineRPM) / Time.deltaTime;
            _lastRPM = currentEngineRPM;

            //if the rpm changes too quickly, enable all audio sources to avoid audio cracking sound
            if (rpmChangeRate > size * _engineSoundsSO.RPMStep)
            {
                for (int i = 0; i < size; i++)
                    _engineAudioSources[i].enabled = true;

                return;
            }

            //otherwise, enable audio sources based on their volume + enable 1 audio source before and after all the working audio sources
            //to prepare for audio source change.
            for (int i = 0; i < size; i++)
            {
                AudioSource source = _engineAudioSources[i];
                if (source.volume < 0.01f)
                    source.volume = 0;

                if (i - 1 >= 0)
                    if (source.volume == 0 && _engineAudioSources[i - 1].volume != 0)
                    {
                        source.enabled = true;
                        continue;
                    }

                if (i + 1 < size)
                    if (source.volume == 0 && _engineAudioSources[i + 1].volume != 0)
                    {
                        source.enabled = true;
                        continue;
                    }

                source.enabled = source.volume != 0;
            }
        }

        private void UpdateAudioSourceSettings(AudioSource audioSource)
        {
            if (!_3DSound)
            {
                audioSource.spatialBlend = 0;
                return;
            }

            audioSource.spatialBlend = _spatialBlend;
            audioSource.dopplerLevel = _dopplerLevel;
            audioSource.spread = _spread;
            audioSource.rolloffMode = _volumeRolloff;
            audioSource.minDistance = _minDistance;
            audioSource.maxDistance = _maxDistance;
        }
    }
}
