using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace KoxyLab.BloodAndFire.Scripts.Behaviour.AmbientLight
{
    public class DayCycleController : UnityEngine.MonoBehaviour
    {
        private int _miInDay = 1440;
        
        [Header("Light configs")]
        public float orbitSpeed = 1.0f;
    
        [Header("Day")]
        public EnvironmentLight sun;
        public AnimationCurve daytLightCurve;


        [Header("Night")]
        public EnvironmentLight moon;
        public AnimationCurve nightLightCurve;
        public AnimationCurve starsCurve;
        private bool _isNight;

        [Header("Sky volume")]
        public Volume skyVolume;
        public float spaceEmissionMultiplier = 1.0f;

        private PhysicallyBasedSky _sky;

        [Header("Fog")] 
        private Fog _fog;
        public float fogHeight = 0;
        public float fogDay = 200;
        public float fogNight = 65;
        public float fogNightHeight = 6;

        [Header("Time")] 
        [Range(0, 30)] public int currentDay;
        [Range(0, 24)] public int currentHour;
        [Range(0, 60)] public int currentMin;
        public float timeOfDay;
        
        [Header("Events")] 
        [SerializeField] private UnityEvent dawn;
        [SerializeField] private UnityEvent sunset;
        [SerializeField] private UnityEvent newDay;
        
        private void OnValidate()
        {
            GetVolumeProfiles();
            UpdateTime();
            timeOfDay = (currentHour * 60) + currentMin;
        }

        private void Start()
        {
            GetVolumeProfiles();
        }

        private void Update()
        {
            timeOfDay += Time.deltaTime * orbitSpeed;

            if (timeOfDay >= _miInDay)
            {
                timeOfDay = 0;
                newDay.Invoke();
            }
        
            UpdateTime();
            
            currentMin = (int) (timeOfDay % 60);
            currentHour = (int) (timeOfDay / 60);
        }

        private void GetVolumeProfiles()
        {
            skyVolume.profile.TryGet(out _sky);
            skyVolume.profile.TryGet(out _fog);
        }

        private void UpdateTime()
        {
            if (currentHour >= 24)
            {
                currentDay++;
                currentHour = 0;
            }
            if (currentMin >= 60)
            {
                currentMin = 0;
            }
            
            var alpha = timeOfDay / (float)_miInDay;

            float sunRotation = Mathf.Lerp(-90, 270, alpha);
            float moonRotation = sunRotation - 180;
            
            sun.transform.rotation = Quaternion.Euler(sunRotation, -150.0f, 0);
            moon.transform.rotation = Quaternion.Euler(moonRotation, -150.0f, 0);

            CheckNightDayTransition();
            
            _sky.spaceEmissionMultiplier.value = starsCurve.Evaluate(alpha) * spaceEmissionMultiplier; 

            sun.intensityDivider = daytLightCurve.Evaluate(alpha);
            moon.intensityDivider = nightLightCurve.Evaluate(alpha);

            if (_isNight)
            {
                _fog.meanFreePath.value = nightLightCurve.Evaluate(alpha) * fogNight;
                _fog.baseHeight.value = nightLightCurve.Evaluate(alpha) * fogNightHeight;
            }
            else
            {
                _fog.meanFreePath.value = daytLightCurve.Evaluate(alpha) * fogDay;
            }
        }

        private void CheckNightDayTransition()
        {
            if (_isNight)
            {
                if (moon.transform.rotation.eulerAngles.x > 180)
                {
                    StartDay();
                }
            }
            else
            {
                if (sun.transform.rotation.eulerAngles.x > 180)
                {
                    StartNight();
                }
            }
        }

        void StartDay()
        {
            _isNight = false;
            dawn.Invoke();
        }

        void StartNight()
        {
            _isNight = true;
            sunset.Invoke();
        }
        
    }
}
