using UnityEngine;
#if VISUAL_EFFECT_GRAPH_INSTALLED
using UnityEngine.VFX;
#endif

namespace Assets.VehicleController
{
    public class CarVisualsCollisionEffects
    {
        private readonly CollisionHandler _collisionHandler;
        private readonly CollisionParameters _collisionParameters;
        private readonly Transform _transform;

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private VisualEffect _impactEffectVFX;
        private VisualEffect _leftSideSparksVFX;
        private VisualEffect _rightSideSparksVFX;
#endif

        private ParticleSystem _impactPS;
        private ParticleSystem _leftSidePS;
        private ParticleSystem _rightSidePS;

        private GameObject _effectsRoot;
        private bool _subscribed;

        public CarVisualsCollisionEffects(CollisionHandler collisionHandler, CollisionParameters collisionParameters, Transform transform)
        {
            _collisionHandler = collisionHandler;
            _collisionParameters = collisionParameters;
            _transform = transform;

            Subscribe();

            _effectsRoot = new GameObject("CollisionEffects");
            _effectsRoot.transform.SetParent(_transform, false);

#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeStayVFX(_effectsRoot);
            if (_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeImpactVFX(_effectsRoot);
#endif
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                InitializeStayParticles(_effectsRoot);
            if (_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                InitializeImpactParticles(_effectsRoot);
        }

        #region Initialization
#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void InitializeStayVFX(GameObject collEffects)
        {
            GameObject leftSparks = new("Left Sparks");
            leftSparks.transform.SetParent(collEffects.transform, false);
            leftSparks.transform.forward = _transform.forward;
            leftSparks.transform.localScale = new Vector3(-1, 1, 1);
            _leftSideSparksVFX = leftSparks.AddComponent<VisualEffect>();
            _leftSideSparksVFX.visualEffectAsset = _collisionParameters.StayVisualEffect.VFXAsset;
            _leftSideSparksVFX.Stop();

            GameObject rightSparks = new("Right Sparks");
            rightSparks.transform.SetParent(collEffects.transform, false);
            rightSparks.transform.forward = _transform.forward;
            _rightSideSparksVFX = rightSparks.AddComponent<VisualEffect>();
            _rightSideSparksVFX.visualEffectAsset = _collisionParameters.StayVisualEffect.VFXAsset;
            _rightSideSparksVFX.Stop();
        }

        private void InitializeImpactVFX(GameObject collEffects)
        {
            GameObject impact = new("Impact Sparks");
            impact.transform.SetParent(collEffects.transform, false);
            _impactEffectVFX = impact.AddComponent<VisualEffect>();
            _impactEffectVFX.visualEffectAsset = _collisionParameters.ImpactVisualEffect.VFXAsset;
            _impactEffectVFX.Stop();
        }
#endif

        private void InitializeStayParticles(GameObject collEffects)
        {
            _leftSidePS = GameObject.Instantiate(_collisionParameters.StayVisualEffect.ParticleSystem, collEffects.transform);
            _leftSidePS.transform.forward = _transform.forward;
            _leftSidePS.transform.localScale = new Vector3(-1, 1, 1);
            _leftSidePS.Stop();

            _rightSidePS = GameObject.Instantiate(_collisionParameters.StayVisualEffect.ParticleSystem, collEffects.transform);
            _rightSidePS.transform.forward = _transform.forward;
            _rightSidePS.Stop();
        }

        private void InitializeImpactParticles(GameObject collEffects)
        {
            _impactPS = GameObject.Instantiate(_collisionParameters.ImpactVisualEffect.ParticleSystem, collEffects.transform);
            _impactPS.Stop();
        }
        #endregion

        #region Collision Event Handlers
        private void CollisionHandler_OnAllCollisionsExit()
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                _leftSideSparksVFX?.Stop();
                _rightSideSparksVFX?.Stop();
            }
#endif
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
            {
                _leftSidePS?.Stop();
                _rightSidePS?.Stop();
            }
        }

        private void CollisionHandler_OnCollisionSideExit(CollisionSide obj)
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                if (obj == CollisionSide.Left) _leftSideSparksVFX?.Stop();
                if (obj == CollisionSide.Right) _rightSideSparksVFX?.Stop();
            }
#endif
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
            {
                if (obj == CollisionSide.Left) _leftSidePS?.Stop();
                if (obj == CollisionSide.Right) _rightSidePS?.Stop();
            }
        }

        private void CollisionHandler_OnCollisionSideStay(CollisionStayInfo[] obj)
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                HandleVFXCollisionStay(obj);
#endif
            if (_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                HandleParticleCollisionStay(obj);
        }

        private void CollisionHandler_OnCollisionImpact(CollisionImpactInfo info)
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                HandleVFXCollisionImpact(info);
#endif
            if (_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                HandleParticleCollisionImpact(info);
        }
        #endregion

        #region VFX / Particle Handlers
#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void HandleVFXCollisionStay(CollisionStayInfo[] obj)
        {
            foreach (var info in obj)
            {
                if (info.CollisionSide == CollisionSide.Right)
                {
                    _rightSideSparksVFX?.SetVector3("position", info.Position);
                    _rightSideSparksVFX?.SetFloat("spawnAmount", info.RelativeMagnitude * 2);
                    _rightSideSparksVFX?.Play();
                }

                if (info.CollisionSide == CollisionSide.Left)
                {
                    _leftSideSparksVFX?.SetVector3("position", info.Position);
                    _leftSideSparksVFX?.SetFloat("spawnAmount", info.RelativeMagnitude * 2);
                    _leftSideSparksVFX?.Play();
                }
            }
        }

        private void HandleVFXCollisionImpact(CollisionImpactInfo info)
        {
            if (_impactEffectVFX == null) return;

            if (info.Side == CollisionSide.Front || info.Side == CollisionSide.Rear)
                _impactEffectVFX.transform.up = Vector3.up;
            else if (info.Side == CollisionSide.Bottom || info.Side == CollisionSide.Top)
                _impactEffectVFX.transform.up = info.RelativeVelocity;
            else
                _impactEffectVFX.transform.right = info.Normal;

            float scale = (info.Side == CollisionSide.Bottom || info.Side == CollisionSide.Top ? 3 : 2)
                          * (info.DotToMyVelocity + Mathf.Clamp01(info.CollisionMagnitude * 2 / 30))
                          + 0.07f;

            _impactEffectVFX.SetVector3("position", info.Point);
            _impactEffectVFX.SetFloat("spawnAmount", 25 + info.CollisionMagnitude / 25);
            _impactEffectVFX.SetFloat("scaleMultiplier", scale);
            _impactEffectVFX.SetVector3("velocityMultiplier", Vector3.one * Mathf.Clamp01(0.1f + info.CollisionMagnitude / 100));
            _impactEffectVFX.Play();
        }
#endif

        private void HandleParticleCollisionStay(CollisionStayInfo[] obj)
        {
            foreach (var info in obj)
            {
                if (info.CollisionSide == CollisionSide.Right && _rightSidePS != null)
                {
                    _rightSidePS.transform.position = info.Position;
                    _rightSidePS.Play();
                }

                if (info.CollisionSide == CollisionSide.Left && _leftSidePS != null)
                {
                    _leftSidePS.transform.position = info.Position;
                    _leftSidePS.Play();
                }
            }
        }

        private void HandleParticleCollisionImpact(CollisionImpactInfo info)
        {
            if (_impactPS == null) return;

            if (info.Side == CollisionSide.Front || info.Side == CollisionSide.Rear)
                _impactPS.transform.up = Vector3.up;
            else if (info.Side == CollisionSide.Bottom || info.Side == CollisionSide.Top)
                _impactPS.transform.up = info.RelativeVelocity;
            else
                _impactPS.transform.right = info.Normal;

            float scale = info.DotToMyVelocity;
            if (info.Side == CollisionSide.Bottom && info.DotToMyVelocity < 0.2f)
                scale = 0.05f;

            var main = _impactPS.main;
            main.startSizeXMultiplier = 0.05f * scale;
            main.startSizeYMultiplier = 0.2f * scale;
            _impactPS.transform.position = info.Point;
            _impactPS.Play();
        }
        #endregion

        #region Subscriptions
        private void Subscribe()
        {
            if (_collisionHandler == null || _subscribed) return;

            _collisionHandler.OnCollisionImpact += CollisionHandler_OnCollisionImpact;
            _collisionHandler.OnCollisionSideStay += CollisionHandler_OnCollisionSideStay;
            _collisionHandler.OnCollisionSideExit += CollisionHandler_OnCollisionSideExit;
            _collisionHandler.OnAllCollisionsExit += CollisionHandler_OnAllCollisionsExit;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (_collisionHandler == null || !_subscribed) return;

            _collisionHandler.OnCollisionImpact -= CollisionHandler_OnCollisionImpact;
            _collisionHandler.OnCollisionSideStay -= CollisionHandler_OnCollisionSideStay;
            _collisionHandler.OnCollisionSideExit -= CollisionHandler_OnCollisionSideExit;
            _collisionHandler.OnAllCollisionsExit -= CollisionHandler_OnAllCollisionsExit;
            _subscribed = false;
        }

        public void Destroy()
        {
            Unsubscribe();

            if (_effectsRoot != null)
                GameObject.Destroy(_effectsRoot);
        }
        #endregion
    }
}
