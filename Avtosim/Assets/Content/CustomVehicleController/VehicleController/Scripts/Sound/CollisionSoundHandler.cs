using UnityEngine;

namespace Assets.VehicleController
{
    public class CollisionSoundHandler
    {
        private AudioSource _scrapingAudioSource;
        private AudioSource _effectAudioSource;
        private CarExtraSoundsSO _carExtraSoundsSO;

        private bool _scripingSoundExists;

        private CollisionSoundParameters _collisionSoundParameters;

        private float _lastCollisionTime;
        private float _volumeScale = 0;
        private float _volumeScaleResetTime = 0.3f;
        private float _collisionVolumeGainTime = 1;

        public CollisionSoundHandler(CollisionHandler collisionHandler, AudioSource effectSource, 
            AudioSource scrapingAudioSource, CarExtraSoundsSO carExtraSoundsSO, CollisionSoundParameters collisionSoundParameters)
        {
            _scrapingAudioSource = scrapingAudioSource;
            _effectAudioSource = effectSource;
            _carExtraSoundsSO = carExtraSoundsSO;
            _collisionSoundParameters = collisionSoundParameters;

            collisionHandler.OnAllCollisionsExit += CollisionHandler_OnAllCollisionsExit;
            collisionHandler.OnCollisionSideStay += CollisionHandler_OnCollisionSideStay;
            collisionHandler.OnCollisionImpact += CollisionHandler_OnCollisionImpact;

            _scripingSoundExists = _carExtraSoundsSO.MetalScraping != null;

            if (!_scripingSoundExists)
                return;

            _scrapingAudioSource.clip = _carExtraSoundsSO.MetalScraping;
            _scrapingAudioSource.Stop();
            _scrapingAudioSource.volume = 0;
            _collisionSoundParameters = collisionSoundParameters;
        }

        private void CollisionHandler_OnCollisionImpact(CollisionImpactInfo info)
        {
            if (info.Side == CollisionSide.Bottom)
                return;

            float volumeScale = Mathf.Clamp01((info.CollisionMagnitude - 10) / _collisionSoundParameters.SpeedForMaxVolume);
            volumeScale *= info.DotToMyVelocity;
            volumeScale += 0.1f;

            _effectAudioSource.PlayOneShot(_carExtraSoundsSO.CrashImpact[Random.Range(0, _carExtraSoundsSO.CrashImpact.Length)], volumeScale);
        }

        private void CollisionHandler_OnCollisionSideStay(CollisionStayInfo[] obj)
        {
            if (!_scripingSoundExists)
                return;

            for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i].CollisionSide == CollisionSide.Right || obj[i].CollisionSide == CollisionSide.Left)
                {
                    HandleAudio(obj[i].RelativeMagnitude);
                    break;
                }
            }

        }

        private void HandleAudio(float speed)
        {
            if (!_scrapingAudioSource.isPlaying)
                _scrapingAudioSource.Play();

            if (_lastCollisionTime + _volumeScaleResetTime < Time.time)
                _volumeScale = 0;

            _volumeScale = Mathf.Clamp01(_volumeScale + Time.deltaTime / _collisionVolumeGainTime);

            _scrapingAudioSource.volume = _volumeScale * Mathf.Clamp01(_collisionSoundParameters.MinVolume + Mathf.Clamp01(speed / _collisionSoundParameters.SpeedForMaxVolume) * _collisionSoundParameters.MaxVolumeGain);
            _scrapingAudioSource.pitch = 1 + Mathf.Clamp01(speed / _collisionSoundParameters.SpeedForMaxVolume) * _collisionSoundParameters.MaxPitchGain;

            _lastCollisionTime = Time.time;
        }

        private void StopAudio()
        {
            _scrapingAudioSource.Stop();
        }

        private void CollisionHandler_OnAllCollisionsExit()
        {
            if (!_scripingSoundExists)
                return;
            StopAudio();
        }
    }
}
