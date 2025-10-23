using UnityEngine;
#if VISUAL_EFFECT_GRAPH_INSTALLED
using UnityEngine.VFX;
#endif

namespace Assets.VehicleController
{
    public class CarVisualsNitrous
    {
        private CurrentCarStats _currentCarStats;
        private NitrousParameters _parameters;

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private VisualEffect[] _nitroVFXArray;
#endif

        private ParticleSystem[] _nitroPSArray;

        private const float SPAWN_AMOUNT_MAX_VFX = 30;
        private const float SPAWN_AMOUNT_MAX_PS = 300;

        private int _spawnRatePropertyID = Shader.PropertyToID("spawnRate");
        private int _colorPropertyID = Shader.PropertyToID("color");
        private int _forwardVelPropertyID = Shader.PropertyToID("forwardVelocity");
        private int _sideVelPropertyID = Shader.PropertyToID("sideVelocity");

        public CarVisualsNitrous(NitrousParameters nitrousParameters, CurrentCarStats currentCarStats)
        {
            _parameters = nitrousParameters;
            _currentCarStats = currentCarStats;

#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_parameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeVFX();
#endif
            if (_parameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                InitializePS();
        }

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void InitializeVFX()
        {
            if (_parameters.VisualEffect.VFXAsset == null)
            {
                Debug.LogWarning("You have Nitrous Visual Effect, but Visual Effect Asset is not assigned");
                return;
            }

            int size = _parameters.ExhaustsPositionArray.Length;

            _nitroVFXArray = new VisualEffect[size];

            for (int i = 0; i < size; i++)
            {
                GameObject holder = new GameObject("Nitro Position");
                holder.transform.parent = _parameters.ExhaustsPositionArray[i];
                holder.transform.localPosition = Vector3.zero;
                holder.transform.localRotation = Quaternion.Euler(0, 0, 0);
                _nitroVFXArray[i] = holder.gameObject.AddComponent<VisualEffect>();
                _nitroVFXArray[i].visualEffectAsset = _parameters.VisualEffect.VFXAsset;
                _nitroVFXArray[i].Play();
            }
        }


        private void HandleNitroVFX()
        {
            for (int i = 0; i < _nitroVFXArray.Length; i++)
            {
                if (_currentCarStats.NitroIntensity == 0 || !_currentCarStats.Accelerating)
                    _nitroVFXArray[i].SetFloat(_spawnRatePropertyID, 0);
                else
                {
                    _nitroVFXArray[i].SetGradient(_colorPropertyID, _parameters.Gradient);
                    _nitroVFXArray[i].SetFloat(_spawnRatePropertyID, _currentCarStats.NitroIntensity < 1 ? Random.Range(0, SPAWN_AMOUNT_MAX_VFX / 4) : SPAWN_AMOUNT_MAX_VFX);
                    _nitroVFXArray[i].SetFloat(_sideVelPropertyID, _currentCarStats.SidewaysForce / -10);
                    _nitroVFXArray[i].SetFloat(_forwardVelPropertyID, _currentCarStats.NitroIntensity < 1 ? 0 : -2);
                }
            }
        }

#endif

        public void HandleNitroEffect()
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_parameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                HandleNitroVFX();
#endif
            if (_parameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                HandleNitroPS();
        }

        private void InitializePS()
        {
            if (_parameters.VisualEffect.ParticleSystem == null)
            {
                Debug.LogWarning("You have Nitrous Visual Effect, but Particle System is not assigned");
                return;
            }

            int size = _parameters.ExhaustsPositionArray.Length;

            _nitroPSArray = new ParticleSystem[size];
            for (int i = 0; i < size; i++)
            {
                _nitroPSArray[i] = GameObject.Instantiate(_parameters.VisualEffect.ParticleSystem);
                _nitroPSArray[i].Play();
                _nitroPSArray[i].transform.parent = _parameters.ExhaustsPositionArray[i].transform;
                _nitroPSArray[i].transform.localPosition = new(0, 0, 0);
                _nitroPSArray[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        private void HandleNitroPS()
        {
            for (int i = 0; i < _nitroPSArray.Length; i++)
            {
                if (_currentCarStats.NitroIntensity == 0 || !_currentCarStats.Accelerating)
                {
                    ParticleSystem.EmissionModule emission = _nitroPSArray[i].emission;
                    emission.rateOverTime = 0;
                }
                else
                {
                    if (_nitroPSArray[i].isStopped)
                        _nitroPSArray[i].Play();

                    ParticleSystem.ForceOverLifetimeModule forceOverLife = _nitroPSArray[i].forceOverLifetime;
                    forceOverLife.x = -_currentCarStats.SidewaysForce * 3;

                    forceOverLife.z = _currentCarStats.NitroIntensity < 1 ? _currentCarStats.NitroIntensity * _currentCarStats.NitroIntensity * -160 : -80;

                    ParticleSystem.EmissionModule emission = _nitroPSArray[i].emission;
                    emission.rateOverTime = _currentCarStats.NitroIntensity < 1 ? Random.Range(0, SPAWN_AMOUNT_MAX_PS / 4) : SPAWN_AMOUNT_MAX_PS;
                }
            }
        }
    }
}
