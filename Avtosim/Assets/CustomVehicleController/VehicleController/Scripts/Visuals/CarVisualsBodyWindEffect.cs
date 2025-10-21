using UnityEngine;
#if VISUAL_EFFECT_GRAPH_INSTALLED
using UnityEngine.VFX;
#endif

namespace Assets.VehicleController
{
    public class CarVisualsBodyWindEffect
    {
#if VISUAL_EFFECT_GRAPH_INSTALLED
        private VisualEffect _bodySpeedEffect;
#endif

        private int _speedPropertyID = Shader.PropertyToID("Speed");
        private int _velocityPropertyID = Shader.PropertyToID("Velocity");

        private ParticleSystem _bodyWindPSInstance;

        private EffectTypeParameters _parameters;

        private Transform _transform;

        public CarVisualsBodyWindEffect(EffectTypeParameters parameters, Transform transform)
        {
            _parameters = parameters;
            _transform = transform;

#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (parameters.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeVFX();
#endif
            if (parameters.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                InitializePS();

        }

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void InitializeVFX()
        {
            if (_parameters.VFXAsset == null)
                return;

            GameObject parent = new("Body Speed Effect");
            parent.transform.parent = this._transform.root;
            parent.transform.localPosition = Vector3.zero;
            parent.transform.localRotation = Quaternion.identity;
            _bodySpeedEffect = parent.AddComponent<VisualEffect>();
            _bodySpeedEffect.visualEffectAsset = _parameters.VFXAsset;
        }

        private void DisplayVFX(float speed, Vector3 rbVelocity)
        {
            if (_parameters.VFXAsset == null)
            {
                Debug.LogWarning("You have Body Wind Effect, but Visual Effect Asset is not assigned");
                return;
            }

            _bodySpeedEffect.SetFloat(_speedPropertyID, speed);
            _bodySpeedEffect.SetFloat(_velocityPropertyID, _transform.InverseTransformDirection(rbVelocity).x);
        }
#endif

        private void InitializePS()
        {
            if (_parameters.ParticleSystem == null)
                return;

            GameObject parent = new("Body Speed Effect");
            parent.transform.parent = this._transform.root;
            parent.transform.localPosition = Vector3.zero;
            parent.transform.localRotation = Quaternion.identity;
            _bodyWindPSInstance = GameObject.Instantiate(_parameters.ParticleSystem, parent.transform);
        }

        public void HandleSpeedEffect(float speed, Vector3 rbVelocity)
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_parameters.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                DisplayVFX(speed, rbVelocity);
#endif
            if (_parameters.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                DisplayPS(speed, rbVelocity);
        }



        private void DisplayPS(float speed, Vector3 rbVelocity)
        {
            if (rbVelocity == Vector3.zero)
                return;

            _bodyWindPSInstance.transform.forward = rbVelocity.normalized;

            var main = _bodyWindPSInstance.main;
            var emission = _bodyWindPSInstance.emission;
            main.startLifetime = Mathf.Clamp(1 / (speed / 120), 0.2f, 1);
            emission.rateOverTime = Mathf.Clamp(speed, 0, 100);
            main.startSpeed = -speed / 2;

            Color targetColor = Color.white;
            targetColor.a = Mathf.Clamp(1 / (150 / (speed + 1)), 0.1f, 0.5f);
            main.startColor = targetColor;
        }
    }
}