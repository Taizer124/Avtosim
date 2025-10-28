using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class MobileUIElement : MonoBehaviour
    {
        public void SetIncreasedScale() => transform.localScale = Vector3.one * 1.1f;
        public void SetDefaultScale() => transform.localScale = Vector3.one;
    }
}
