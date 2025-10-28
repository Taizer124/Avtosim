using UnityEngine;

namespace Assets.VehicleController
{
    public class LookAtPlayer : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        // Update is called once per frame
        void Update()
        {
            if (_vehicleController == null)
                return;

            Vector3 tempPos = _vehicleController.transform.position;
            tempPos.y = transform.position.y;

            transform.forward = tempPos - transform.position;
        }
    }
}
