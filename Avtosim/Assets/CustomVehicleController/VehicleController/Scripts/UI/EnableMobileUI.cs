using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class EnableMobileUI : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            gameObject.SetActive(Application.isMobilePlatform);
        }
    }
}
