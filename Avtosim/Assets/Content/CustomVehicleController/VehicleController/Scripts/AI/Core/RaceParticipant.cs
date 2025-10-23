using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/ai-racers-setup")]
    public class RaceParticipant : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;
        [SerializeField, Space]
        private Collider _collider;
        [SerializeField]
        private bool _isPlayer;
        public bool IsPlayer => _isPlayer;

        public static Dictionary<Collider, Rigidbody> ColliderToRigidbodyDictionary;

        private void Start()
        {
            if (ColliderToRigidbodyDictionary == null)
                ColliderToRigidbodyDictionary = new();

            if (!ColliderToRigidbodyDictionary.ContainsKey(_collider))
            {
                if (_collider == null)
                    Debug.LogError("Collider is unassigned. This vehicle will not be detected ");
                else
                    ColliderToRigidbodyDictionary.Add(_collider, _vehicleController.GetRigidbody());
            }
            RaceManager.Instance.RegisterInRace(this, _isPlayer);
        }

        private void Update()
        {
            RaceManager.Instance.CalculateDirectionForRacer(this, transform.position, _vehicleController.GetCurrentCarStats().SpeedInMsPerS);
        }

        public Rigidbody TryFindRaceParticipant(Collider collider)
        {
            if (ColliderToRigidbodyDictionary.TryGetValue(collider, out Rigidbody hitRB))
                return hitRB;

            return null;
        }

        public void EnableInput(bool enable)
        {
            _vehicleController.EnableInput(enable);
        }
    }
}
