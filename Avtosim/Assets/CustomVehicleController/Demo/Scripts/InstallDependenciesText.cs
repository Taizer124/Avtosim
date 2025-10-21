using UnityEngine;
using UnityEngine.UI;

namespace Assets.VehicleController
{
    public class InstallDependenciesText : MonoBehaviour
    {
        [SerializeField]
        private Text _text;
        // Start is called before the first frame update
        void Start()
        {
#if SPLINE_PACKAGE_INSTALLED
#else
        _text.text = "AI requires Unity's Spline Package. Install it from the package manager. \n";
#endif

#if MATH_PACKAGE_INSTALLED
#else
        _text.text += "AI Requires Unity's Mathematics Package. Install it from the package manager.";
#endif
        }
    }
}

