using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnvironmentController : MonoBehaviour
{
    [Header("Post-Processing")]
    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private float dayExposure = 1.2f;
    [SerializeField] private float nightExposure = 0.3f;
    [SerializeField] private float dayTemperature = 20f;
    [SerializeField] private float nightTemperature = -20f;

    private ColorAdjustments colorAdjustments;
    private WhiteBalance whiteBalance;

    private const int DAY_START = 4;
    private const int DAY_END = 14;
    private const int EVENING_END = 18;

    private void Awake()
    {
        if (postProcessingVolume != null)
        {
            postProcessingVolume.profile.TryGet(out colorAdjustments);
            postProcessingVolume.profile.TryGet(out whiteBalance);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayNightCycleUpdated += UpdateEnvironmentLighting;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayNightCycleUpdated -= UpdateEnvironmentLighting;
        }
    }

    private void UpdateEnvironmentLighting(int hours, int minutes, float normalizedTime)
    {
        float timeOfDay = hours + minutes / 60f;
        float exposure = 0f;
        float temperature = 0f;

        if (timeOfDay >= DAY_START && timeOfDay < DAY_END)
        {
            exposure = dayExposure;
            temperature = dayTemperature;
        }
        else if (timeOfDay >= DAY_END && timeOfDay < EVENING_END)
        {
            float eveningProgress = (timeOfDay - DAY_END) / (EVENING_END - DAY_END);
            exposure = Mathf.Lerp(dayExposure, nightExposure, eveningProgress);
            temperature = Mathf.Lerp(dayTemperature, nightTemperature, eveningProgress);
        }
        else
        {
            exposure = nightExposure;
            temperature = nightTemperature;

            if (timeOfDay < DAY_START)
            {
                float dawnProgress = timeOfDay / DAY_START;
                exposure = Mathf.Lerp(nightExposure, dayExposure, dawnProgress);
                temperature = Mathf.Lerp(nightTemperature, dayTemperature, dawnProgress);
            }
        }

        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = exposure;

        if (whiteBalance != null)
            whiteBalance.temperature.value = temperature;
    }
}
