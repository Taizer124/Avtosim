using UnityEngine;
#if VISUAL_EFFECT_GRAPH_INSTALLED
using UnityEngine.VFX;
#endif

namespace Assets.VehicleController
{
    public class CarVisualsCollisionEffects
    {
        private CollisionParameters _collisionParameters;
        private Transform _transform;

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private VisualEffect _impactEffectVFX;
        private VisualEffect _leftSideSparksVFX;
        private VisualEffect _rightSideSparksVFX;
#endif

        private ParticleSystem _impactPS;
        private ParticleSystem _leftSidePS;
        private ParticleSystem _rightSidePS;

        public CarVisualsCollisionEffects(CollisionHandler collisionHandler, CollisionParameters collisionParameters, Transform transform)
        {
            _collisionParameters = collisionParameters;
            _transform = transform;

            collisionHandler.OnCollisionImpact += CollisionHandler_OnCollisionImpact;
            collisionHandler.OnCollisionSideStay += CollisionHandler_OnCollisionSideStay;
            collisionHandler.OnCollisionSideExit += CollisionHandler_OnCollisionSideExit;
            collisionHandler.OnAllCollisionsExit += CollisionHandler_OnAllCollisionsExit;

            GameObject collEffects = new GameObject("CollisionEffects");
            collEffects.transform.SetParent(_transform, false);

#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeStayVFX(collEffects);
            if (_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                InitializeImpactVFX(collEffects);
#endif
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                InitializeStayParticles(collEffects);
            if (_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                InitializeImpactParticles(collEffects);
        }

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void InitializeStayVFX(GameObject collEffects)
        {
            GameObject leftSparks = new GameObject("Left Sparks");
            leftSparks.transform.SetParent(collEffects.transform, false);
            leftSparks.transform.forward = _transform.forward;
            leftSparks.transform.localScale = new Vector3(-1, 1, 1);
            _leftSideSparksVFX = leftSparks.AddComponent<VisualEffect>();
            _leftSideSparksVFX.visualEffectAsset = _collisionParameters.StayVisualEffect.VFXAsset;
            _leftSideSparksVFX.Stop();

            GameObject rightSparks = new GameObject("Right Sparks");
            rightSparks.transform.SetParent(collEffects.transform, false);
            rightSparks.transform.forward = _transform.forward;
            _rightSideSparksVFX = rightSparks.AddComponent<VisualEffect>();
            _rightSideSparksVFX.visualEffectAsset = _collisionParameters.StayVisualEffect.VFXAsset;
            _rightSideSparksVFX.Stop();
        }
        private void InitializeImpactVFX(GameObject collEffects)
        {
            GameObject impact = new GameObject("Impact Sparks");
            impact.transform.SetParent(collEffects.transform, false);
            _impactEffectVFX = impact.AddComponent<VisualEffect>();
            _impactEffectVFX.visualEffectAsset = _collisionParameters.ImpactVisualEffect.VFXAsset;
            _impactEffectVFX.Stop();
        }
#endif
        private void InitializeStayParticles(GameObject collEffects)
        {
            _leftSidePS = GameObject.Instantiate(_collisionParameters.StayVisualEffect.ParticleSystem);
            _leftSidePS.transform.SetParent(collEffects.transform, false);
            _leftSidePS.transform.forward = _transform.forward;
            _leftSidePS.transform.localScale = new Vector3(-1, 1, 1);
            _leftSidePS.Stop();

            _rightSidePS = GameObject.Instantiate(_collisionParameters.StayVisualEffect.ParticleSystem);
            _rightSidePS.transform.SetParent(collEffects.transform, false);
            _rightSidePS.transform.forward = _transform.forward;
            _rightSidePS.Stop();
        }

        private void InitializeImpactParticles(GameObject collEffects)
        {
            _impactPS = GameObject.Instantiate(_collisionParameters.ImpactVisualEffect.ParticleSystem);
            _impactPS.transform.SetParent(collEffects.transform, false);
            _impactPS.Stop();
        }

        private void CollisionHandler_OnAllCollisionsExit()
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                _leftSideSparksVFX.Stop();
                _rightSideSparksVFX.Stop();
            }
#endif
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
            {
                _leftSidePS.Stop();
                _rightSidePS.Stop();
            }
        }

        private void CollisionHandler_OnCollisionSideExit(CollisionSide obj)
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if(_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
            {
                if (obj == CollisionSide.Left)
                    _leftSideSparksVFX.Stop();

                if (obj == CollisionSide.Right)
                    _rightSideSparksVFX.Stop();
            }
#endif
            if (_collisionParameters.StayVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
            {
                if (obj == CollisionSide.Left)
                    _leftSidePS.Stop();

                if (obj == CollisionSide.Right)
                    _rightSidePS.Stop();
            }
        }

        private void CollisionHandler_OnCollisionSideStay(CollisionStayInfo[] obj)
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            if(_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                HandleVFXCollisionStay(obj);
#endif
            if (_collisionParameters.ImpactVisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                HandleParticleCollisionStay(obj);
        }

        private void HandleVFXCollisionStay(CollisionStayInfo[] obj)
        {
#if VISUAL_EFFECT_GRAPH_INSTALLED
            for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i].CollisionSide == CollisionSide.Right)
                {
                    _rightSideSparksVFX.SetVector3("position", obj[i].Position);
                    _rightSideSparksVFX.SetFloat("spawnAmount", obj[i].RelativeMagnitude * 2);
                    _rightSideSparksVFX.Play();
                }

                if (obj[i].CollisionSide == CollisionSide.Left)
                {
                    _leftSideSparksVFX.SetVector3("position", obj[i].Position);
                    _leftSideSparksVFX.SetFloat("spawnAmount", obj[i].RelativeMagnitude * 2);
                    _leftSideSparksVFX.Play();
                }
            }
#endif
        }

        private void HandleParticleCollisionStay(CollisionStayInfo[] obj)
        {
            for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i].CollisionSide == CollisionSide.Right)
                {
                    _rightSidePS.gameObject.transform.position = obj[i].Position;
                    _rightSidePS.Play();
                }

                if (obj[i].CollisionSide == CollisionSide.Left)
                {
                    _leftSidePS.gameObject.transform.position = obj[i].Position;
                    _leftSidePS.Play();
                }
            }
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

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void HandleVFXCollisionImpact(CollisionImpactInfo info)
        {
            if (info.Side == CollisionSide.Front || info.Side == CollisionSide.Rear)
                _impactEffectVFX.transform.up = Vector3.up;
            else if (info.Side == CollisionSide.Bottom || info.Side == CollisionSide.Top)
                _impactEffectVFX.transform.up = info.RelativeVelocity;
            else
                _impactEffectVFX.transform.right = info.Normal;

            float scale = info.Side == CollisionSide.Bottom || info.Side == CollisionSide.Top ? 3 : 2;
            scale *= info.DotToMyVelocity + Mathf.Clamp01(info.CollisionMagnitude * 2 / 30);
            scale += 0.07f;

            _impactEffectVFX.SetVector3("position", info.Point);
            _impactEffectVFX.SetFloat("spawnAmount", 25 + info.CollisionMagnitude / 25);
            _impactEffectVFX.SetFloat("scaleMultiplier", scale);
            _impactEffectVFX.SetVector3("velocityMultiplier", Vector3.one * Mathf.Clamp01(0.1f + info.CollisionMagnitude / 100));
            _impactEffectVFX.Play();
        }
#endif

        private void HandleParticleCollisionImpact(CollisionImpactInfo info)
        {
            if (info.Side == CollisionSide.Front || info.Side == CollisionSide.Rear)
                _impactPS.transform.up = Vector3.up;
            else if (info.Side == CollisionSide.Bottom || info.Side == CollisionSide.Top)
                _impactPS.transform.up = info.RelativeVelocity;
            else
                _impactPS.transform.right = info.Normal;

            float scale = info.DotToMyVelocity;
            if(info.Side == CollisionSide.Bottom && info.DotToMyVelocity < 0.2f)
                scale = 0.05f;

            var main = _impactPS.main;
            main.startSizeXMultiplier = 0.05f * scale;
            main.startSizeYMultiplier = 0.2f * scale;
            _impactPS.gameObject.transform.position = info.Point;
            _impactPS.Play();
        }
    }
}
