using UnityEngine;

namespace Assets.VehicleController
{
    public class CarVisualBrakeDisksGlow
    {
        private BrakeDisksGlowParameters _parameters;
        private CurrentCarStats _currentCarStats;
        private Material[] _defaultMaterialArray;
        private Color[] _defaultColorArray;
        private const float MIN_SPEED_TO_GLOW = 15;
        private const float MAX_HEAT_SPEED = 80;

        private int _colorPropertyID = Shader.PropertyToID("_BaseColor");

        private float _glowIntensity = 0;

        public CarVisualBrakeDisksGlow(BrakeDisksGlowParameters parameters, CurrentCarStats currentCarStats)
        {
            _parameters = parameters;
            _currentCarStats = currentCarStats;

            if (_parameters.BrakeDisksMeshes.Length == 0)
            {
                Debug.LogWarning("You have Brake Disks Glow Effect, but MeshRenderer array is not assigned");
                return;
            }

            if (_parameters.MaterialsAtSpecificIndex)
            {
                Material[] materials = _parameters.BrakeDisksMeshes[0].materials;

                int size = _parameters.MaterialIndexArray.Length;
                _defaultMaterialArray = new Material[size];
                _defaultColorArray = new Color[size];

                //create unique materials
                for (int i = 0; i < size; i++)
                {
                    _defaultMaterialArray[i] = new Material(materials[_parameters.MaterialIndexArray[i]]);
                    materials[_parameters.MaterialIndexArray[i]] = _defaultMaterialArray[i];

                    _defaultColorArray[i] = _defaultMaterialArray[i].color;
                }


                for (int i = 0; i < parameters.BrakeDisksMeshes.Length; i++)
                {
                    parameters.BrakeDisksMeshes[i].materials = materials;
                }
            }
            else
            {
                _defaultMaterialArray = new Material[1];

                _defaultMaterialArray[0] = new Material(_parameters.BrakeDisksMeshes[0].material);
                _defaultColorArray = new Color[1];
                _defaultColorArray[0] = _defaultMaterialArray[0].color;

                for (int i = 0; i < parameters.BrakeDisksMeshes.Length; i++)
                {
                    parameters.BrakeDisksMeshes[i].material = _defaultMaterialArray[0];
                }
            }
        }

        private float _lastGlowIntenisty = 0;

        public void HandleBrakeDisksGlow()
        {
            float brakingIntensity = _currentCarStats.BrakingIntensity;

            if (brakingIntensity > 0 && !_currentCarStats.InAir)
            {
                brakingIntensity *= Mathf.Clamp01((_currentCarStats.SpeedInMsPerS - MIN_SPEED_TO_GLOW) / MAX_HEAT_SPEED);
                _glowIntensity += brakingIntensity * (Time.deltaTime / _parameters.HeatUpTime);
            }
            else
                _glowIntensity -= Time.deltaTime / _parameters.CoolDownTime;

            _glowIntensity = Mathf.Clamp01(_glowIntensity);

            if (_lastGlowIntenisty == _glowIntensity)
                return;

            Color newColor = Color.Lerp(_defaultColorArray[0], _parameters.GlowColor, _glowIntensity);

            if(_parameters.MaterialsAtSpecificIndex)
            {
                int size = _parameters.MaterialIndexArray.Length;
                for (int i = 0; i < size; i++)
                    _defaultMaterialArray[i].SetColor(_colorPropertyID, newColor);
            }
            else
            {
                int size = _defaultMaterialArray.Length;
                for (int i = 0; i < size; i++)
                    _defaultMaterialArray[i].SetColor(_colorPropertyID, newColor);
            }

            _lastGlowIntenisty = _glowIntensity;
        }
    }
}

