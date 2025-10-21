using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class ChangeMobileSteeringInput : MonoBehaviour
    {
        [SerializeField]
        private GameObject _stickInputParent;
        [SerializeField]
        private GameObject _buttonsInputParent;
        private bool _stick = true;
        public void Swap()
        {
            _stick = !_stick;
            _stickInputParent.SetActive(_stick);
            _buttonsInputParent.SetActive(!_stick);
        }
    }
}
