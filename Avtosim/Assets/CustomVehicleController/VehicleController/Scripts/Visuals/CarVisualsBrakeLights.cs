using UnityEngine;

namespace Assets.VehicleController
{
    public class CarVisualsBrakeLights
    {
        private BrakeLightsParameters _parameters;
        private Material[] _defaultMaterialArray;
        private Color[] _defaultColorArray;

        private int _colorPropertyID = Shader.PropertyToID("_BaseColor");

        public CarVisualsBrakeLights(BrakeLightsParameters parameters)
        {
            _parameters = parameters;

            if (_parameters.RearLightMeshes.Length == 0)
            {
                Debug.LogWarning("You have Brake Lights Effect, but MeshRenderer array is not assigned");
                return;
            }

            if (_parameters.MaterialsAtSpecificIndex)
            {
                Material[] materials = _parameters.RearLightMeshes[0].materials;

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


                for (int i = 0; i < parameters.RearLightMeshes.Length; i++)
                {
                    parameters.RearLightMeshes[i].materials = materials;
                }
            }
            else
            {
                _defaultMaterialArray = new Material[1];

                _defaultMaterialArray[0] = new Material(_parameters.RearLightMeshes[0].material);
                _defaultColorArray = new Color[1];
                _defaultColorArray[0] = _defaultMaterialArray[0].color;

                for (int i = 0; i < parameters.RearLightMeshes.Length; i++)
                {
                    parameters.RearLightMeshes[i].material = _defaultMaterialArray[0];
                }
            }
        }

        private bool _lastState = false;

        public void HandleRearLights(bool braking)
        {
            if (_lastState == braking)
                return;

            _lastState = braking;
            if(_parameters.MaterialsAtSpecificIndex)
            {
                int size = _parameters.MaterialIndexArray.Length;
                for (int i = 0; i < size; i++)
                {
                    _defaultMaterialArray[i].SetColor(_colorPropertyID, braking ? _parameters.BrakeColor : _defaultColorArray[i]);
                }
            }
            else
            {
                int size = _defaultMaterialArray.Length;
                for (int i = 0; i < size; i++)
                {
                    _defaultMaterialArray[i].SetColor(_colorPropertyID, braking ? _parameters.BrakeColor : _defaultColorArray[i]);
                }
            }
        }
    }
}

