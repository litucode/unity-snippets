using UnityEngine;

namespace KoxyLab.BloodAndFire.Scripts.Behaviour.AmbientLight
{
    public class EnvironmentLight : UnityEngine.MonoBehaviour
    {

        public float intensityDivider;
        
        private float _startLux;
        private float _updatedLux;
        
        private Light _l;

        internal Light legacyLight
        {
            get
            {
                TryGetComponent<Light>(out _l);
                return _l;
            }
        }
        
        private void Start()
        {
            _startLux = legacyLight.intensity;
        }

        private void Update()
        {
            UpdateLight();
        }

        private void OnValidate()
        {
            _startLux = legacyLight.intensity;
        }

        public void EnableShadows() => legacyLight.shadows = LightShadows.Soft;
        
        public void DisableShadows() => legacyLight.shadows = LightShadows.None;


        public void UpdateLight()
        {
            legacyLight.intensity = _startLux * intensityDivider;
            _updatedLux = _l.intensity;
        }
    }
}