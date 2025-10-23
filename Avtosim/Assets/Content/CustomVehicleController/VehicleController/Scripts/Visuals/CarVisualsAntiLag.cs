using System.Collections;
using UnityEngine;
#if VISUAL_EFFECT_GRAPH_INSTALLED
using UnityEngine.VFX;
#endif
namespace Assets.VehicleController
{
    public class CarVisualsAntiLag
    {
        private CarVisualsExtra _carVisualsExtra;

        private AntiLagParameters _antiLagParameters;

        private ParticleSystem[] _antiLagParticleSystemArray;

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private VisualEffect[] _antiLagVFXArray;
#endif

        private CurrentCarStats _currentCarStats;

        public CarVisualsAntiLag(CarVisualsExtra carVisualsExtra, CurrentCarStats currentCarStats, AntiLagParameters antiLagParameters)
        {
            _carVisualsExtra = carVisualsExtra;
            _currentCarStats = currentCarStats;
            _antiLagParameters = antiLagParameters;
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_antiLagParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeVFX();
#endif
            if (_antiLagParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                InstantiateAntiLagPS();

            if (_currentCarStats == null)
                return;

            _currentCarStats.OnAntiLag += _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag += _currentCarStats_OnShiftedAntiLag;
        }

        private void InstantiateAntiLagPS()
        {
            if (_antiLagParameters.VisualEffect.ParticleSystem == null)
            {
                Debug.LogError("You have Anti-Lag Effect, but Particle System is not assigned"); ;
                return;
            }

            int size = _antiLagParameters.ExhaustsPositionArray.Length;
            _antiLagParticleSystemArray = new ParticleSystem[size];
            for (int i = 0; i < size; i++)
            {
                _antiLagParticleSystemArray[i] = GameObject.Instantiate(_antiLagParameters.VisualEffect.ParticleSystem);
                _antiLagParticleSystemArray[i].Stop();
                _antiLagParticleSystemArray[i].transform.parent = _antiLagParameters.ExhaustsPositionArray[i].transform;
                _antiLagParticleSystemArray[i].transform.localPosition = new(0, 0, 0);
                _antiLagParticleSystemArray[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
#if VISUAL_EFFECT_GRAPH_INSTALLED

        private void InitializeVFX()
        {
            if (_antiLagParameters.VisualEffect.VFXAsset == null)
            {
                Debug.LogWarning("You have Anti Lag Effect, but Visual Effect Asset is not assigned");
                return;
            }

            int size = _antiLagParameters.ExhaustsPositionArray.Length;

            _antiLagVFXArray = new VisualEffect[size];

            for (int i = 0; i < size; i++)
            {
                _antiLagVFXArray[i] = _antiLagParameters.ExhaustsPositionArray[i].gameObject.AddComponent<VisualEffect>();
                _antiLagVFXArray[i].visualEffectAsset = _antiLagParameters.VisualEffect.VFXAsset;
                _antiLagVFXArray[i].Stop();
            }
        }
#endif

        private void _currentCarStats_OnShiftedAntiLag()
        {
            if (_currentCarStats.NitroBoosting)
                return;

            int size = _antiLagParameters.ExhaustsPositionArray.Length;
            for (int i = 0; i < size; i++)
            {
                _carVisualsExtra.StartCoroutine(PlayAntilagNTimes(1, i, _antiLagParameters.BackfireDelay));
            }
        }

        private void _currentCarStats_OnAntiLag()
        {
            if (_currentCarStats.NitroBoosting)
                return;

            int size = _antiLagParameters.ExhaustsPositionArray.Length;
            for (int i = 0; i < size; i++)
            {
                _carVisualsExtra.StartCoroutine(PlayAntilagNTimes(Random.Range(_antiLagParameters.MinBackfireCount, _antiLagParameters.MaxBackfireCount), i, _antiLagParameters.BackfireDelay));
            }
        }

        public void OnDestroy()
        {

            if (_currentCarStats == null)
                return;

            _currentCarStats.OnAntiLag -= _currentCarStats_OnAntiLag;
            _currentCarStats.OnShiftedAntiLag -= _currentCarStats_OnShiftedAntiLag;
        }

        private IEnumerator PlayAntilagNTimes(int times, int id, float delay)
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_antiLagParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                for (int i = 0; i < times; i++)
                {
                    _antiLagVFXArray[id].Play();
                    yield return new WaitForSeconds(delay);
                }
            }
#endif
            if (_antiLagParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
            {
                for (int i = 0; i < times; i++)
                {
                    _antiLagParticleSystemArray[id].Play();
                    yield return new WaitForSeconds(delay);
                }
                _antiLagParticleSystemArray[id].TriggerSubEmitter(0);
            }
        }
    }
}
