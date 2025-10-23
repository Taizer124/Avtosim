#if VISUAL_EFFECT_GRAPH_INSTALLED
using UnityEngine.VFX;
#endif

using UnityEngine;

namespace Assets.VehicleController
{
    public class CarVisualsEngineSmoke
    {
        private EngineSmokeParameters _parameters;

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private VisualEffect _smokeEffectVFX;
#endif
        private ParticleSystem _smokeEffectPS;

        public CarVisualsEngineSmoke(EngineSmokeParameters parameters)
        {
            _parameters = parameters;
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_parameters.SmokeVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeVFX();
#endif
            if(_parameters.SmokeVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                InitializePS();
        }

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void InitializeVFX()
        {
            GameObject go = _parameters.EmitPoint.gameObject;
            _smokeEffectVFX = go.AddComponent<VisualEffect>();
            _smokeEffectVFX.visualEffectAsset = _parameters.SmokeVisualEffect.VFXAsset;
            if (_parameters.PlayOnAwake)
                _smokeEffectVFX.Play();
            else
                _smokeEffectVFX.Stop();
        }
#endif

        private void InitializePS()
        {
            _smokeEffectPS = GameObject.Instantiate(_parameters.SmokeVisualEffect.ParticleSystem);
            _smokeEffectPS.transform.SetParent(_parameters.EmitPoint, false);
            _smokeEffectPS.transform.localPosition = Vector3.zero;
            var main = _smokeEffectPS.main;
            main.playOnAwake = _parameters.PlayOnAwake;
        }

        public void EmitSmoke(bool emit)
        {
            if (_parameters.SmokeVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                EmitPS(emit);

#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_parameters.SmokeVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                EmitVFX(emit);
#endif
        }

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void EmitVFX(bool emit)
        {
            if (emit)
            {
                _smokeEffectVFX.Play();
            }
            else
                _smokeEffectVFX.Stop();
        }
#endif

        private void EmitPS(bool emit)
        {
            if (emit)
                _smokeEffectPS.Play();
            else
                _smokeEffectPS.Stop();
        }
    }
}
