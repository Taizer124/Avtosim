using UnityEngine;
#if VISUAL_EFFECT_GRAPH_INSTALLED
using UnityEngine.VFX;
#endif

namespace Assets.VehicleController
{
    public class CarVisualsTireSmoke
    {
#if VISUAL_EFFECT_GRAPH_INSTALLED
        private VisualEffect[] _tireSmokeVFXArray;
#endif
        private ParticleSystem[] _tireSmokePSArray;

        private TireSmokeParameters _effectParameters;

        private Transform[] _wheelMeshes;

        private Transform _transform;

        private float[] _radiusArray;

        private int _velocityPropertyID = Shader.PropertyToID("velocity");
        private int _positionPropertyID = Shader.PropertyToID("position");

        public CarVisualsTireSmoke(Transform[] wheelMeshes, WheelController[] wheelControllers, Transform transform, TireSmokeParameters effectParameters)
        {
            _wheelMeshes = wheelMeshes;
            _transform = transform;
            _effectParameters = effectParameters;

            _radiusArray = new float[wheelControllers.Length];
            for (int i = 0; i < wheelControllers.Length; i++)
                _radiusArray[i] = wheelControllers[i].WheelRadius;

#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_effectParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                TryInstantiateVFX(wheelMeshes);
#endif
            if (_effectParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                TryInstantiatePS(wheelMeshes);
        }

#if VISUAL_EFFECT_GRAPH_INSTALLED
        private void TryInstantiateVFX(Transform[] wheelMeshes)
        {
            if (_effectParameters.VisualEffect.VFXAsset == null)
            {
                Debug.LogWarning("You have Tire Smoke Effect, but Visual Effect Asset is not assigned");
                return;
            }
            _tireSmokeVFXArray = new VisualEffect[wheelMeshes.Length];

            int size = wheelMeshes.Length;
            for (int i = 0; i < size; i++)
            {
                _tireSmokeVFXArray[i] = wheelMeshes[i].gameObject.AddComponent<VisualEffect>();
                _tireSmokeVFXArray[i].visualEffectAsset = _effectParameters.VisualEffect.VFXAsset;
                _tireSmokeVFXArray[i].Stop();
            }
        }

        private void DisplayVFX(bool display, int id, Vector3 rbVelocityNorm, float speed)
        {
            if (_effectParameters.VisualEffect.VFXAsset == null)
                return;

            if (display)
            {
                _tireSmokeVFXArray[id].Play();
                _tireSmokeVFXArray[id].SetVector3(_positionPropertyID, _wheelMeshes[id].position - new Vector3(0, _radiusArray[id] - _effectParameters.VerticalOffset, 0));
                if (speed < 1)
                {
                    _tireSmokeVFXArray[id].SetVector3(_velocityPropertyID, -_transform.forward * 3);
                }
                else
                {
                    _tireSmokeVFXArray[id].SetVector3(_velocityPropertyID, -rbVelocityNorm);
                }
            }
            else
            {
                _tireSmokeVFXArray[id].Stop();
            }
        }
#endif

        private void TryInstantiatePS(Transform[] wheelMeshes)
        {
            if (_effectParameters.VisualEffect.ParticleSystem == null)
            {
                Debug.LogWarning("You have Tire Smoke Effect, but Particle System is not assigned");
                return;
            }
            _tireSmokePSArray = new ParticleSystem[wheelMeshes.Length];

            int size = wheelMeshes.Length;
            for (int i = 0; i < size; i++)
            {
                _tireSmokePSArray[i] = GameObject.Instantiate(_effectParameters.VisualEffect.ParticleSystem);
                _tireSmokePSArray[i].gameObject.name = wheelMeshes[i].name + "Smoke";
                _tireSmokePSArray[i].transform.parent = wheelMeshes[i].parent;
                _tireSmokePSArray[i].transform.localScale = Vector3.one;
                _tireSmokePSArray[i].Stop();
            }
        }

        public void HandleSmokeEffects(bool display, int id, Vector3 rbVelocityNorm, float speed)
        {

#if VISUAL_EFFECT_GRAPH_INSTALLED
            if (_effectParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.VisualEffect)
                DisplayVFX(display, id, rbVelocityNorm, speed);
#endif
            if (_effectParameters.VisualEffect.VisualEffectType == VisualEffectAssetType.Type.ParticleSystem)
                DisplayPS(display, id, rbVelocityNorm, speed);
        }

        /// <summary>
        /// Уничтожает созданные эффекты. Без этого каждый цикл выключения/
        /// включения машины (а он происходит на каждой гонке: городскую машину
        /// гасят на время заезда и включают на финише) оставлял прошлый комплект
        /// партиклов висеть на колёсах. Ссылку на них теряли, поэтому Stop()
        /// звать было уже некому — и если в момент выключения дым играл, он
        /// продолжал идти бесконечно.
        /// </summary>
        public void Destroy()
        {
            if (_tireSmokePSArray != null)
            {
                for (int i = 0; i < _tireSmokePSArray.Length; i++)
                {
                    if (_tireSmokePSArray[i] == null)
                        continue;
                    _tireSmokePSArray[i].Stop();
                    GameObject.Destroy(_tireSmokePSArray[i].gameObject);
                }
                _tireSmokePSArray = null;
            }

#if VISUAL_EFFECT_GRAPH_INSTALLED
            // VFX-вариант добавляет компонент прямо на меш колеса — иначе при
            // каждом включении на колесе копился ещё один VisualEffect.
            if (_tireSmokeVFXArray != null)
            {
                for (int i = 0; i < _tireSmokeVFXArray.Length; i++)
                {
                    if (_tireSmokeVFXArray[i] == null)
                        continue;
                    _tireSmokeVFXArray[i].Stop();
                    GameObject.Destroy(_tireSmokeVFXArray[i]);
                }
                _tireSmokeVFXArray = null;
            }
#endif
        }

        private void DisplayPS(bool display, int id, Vector3 rbVelocityNorm, float speed)
        {
            if (_effectParameters.VisualEffect.ParticleSystem == null)
                return;
            if (display)
            {
                if (Mathf.Abs(speed) < 1)
                    rbVelocityNorm = _transform.forward;

                _tireSmokePSArray[id].transform.position = _wheelMeshes[id].position - new Vector3(0, _radiusArray[id] - _effectParameters.VerticalOffset, 0);
                _tireSmokePSArray[id].transform.forward = -rbVelocityNorm;
                if (!_tireSmokePSArray[id].isPlaying)
                    _tireSmokePSArray[id].Play();
            }
            else if (_tireSmokePSArray[id].isPlaying)
                _tireSmokePSArray[id].Stop();
        }
    }
}
