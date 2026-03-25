using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnvironmentController : MonoBehaviour
{
    [Header("Post-Processing")]
    [SerializeField] private Volume postProcessingVolume;

    [Space(10)]
    [SerializeField] private float dayExposure = 0f;
    [SerializeField] private float eveningExposure = 0.8f;
    [SerializeField] private float nightExposure = 0.3f;

    [Space(5)]
    [SerializeField] private float dayTemperature = 0f;
    [SerializeField] private float eveningTemperature = 50f;
    [SerializeField] private float nightTemperature = -20f;

    [SerializeField] private Color eveningTint = new Color(1f, 0.7f, 0.4f); // Ýkindi turuncusu- internetten buldum

    [Range(0.1f, 10f)]
    [SerializeField] private float lerpSpeed = 2f;//0 la 10 arasý bir rakam. renk deðiþimlerinin geçiþ hýzýný ayarlýyor

    private ColorAdjustments colorAdjustments;
    private WhiteBalance whiteBalance;

    private const float DAY_END = 0.6f;     //Ýkindi baþlangýcý 16:00-17 arasý
    private const float EVENING_END = 0.85f; // gece baþlangýcý  21:00

    private void Awake()
    {
        if (postProcessingVolume != null)
        {
            postProcessingVolume.profile.TryGet(out colorAdjustments);
            postProcessingVolume.profile.TryGet(out whiteBalance);
        }
    }
    private void Update()
    {
        if (TimeManager.Instance == null || colorAdjustments == null || whiteBalance == null) return;

        float dayProgress = TimeManager.Instance.GetActiveDayPercentage();

        CalculateTargetValues(dayProgress, out float targetExp, out float targetTemp, out Color targetColor);

        // Deðerleri yumuþak bir þekilde uygula Lerp ile
        colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, targetExp, Time.deltaTime * lerpSpeed);
        whiteBalance.temperature.value = Mathf.Lerp(whiteBalance.temperature.value, targetTemp, Time.deltaTime * lerpSpeed);
        colorAdjustments.colorFilter.value = Color.Lerp(colorAdjustments.colorFilter.value, targetColor, Time.deltaTime * lerpSpeed);
    }
    private void CalculateTargetValues(float progress, out float exposure, out float temp, out Color colorFilter)
    {
        if (progress < DAY_END)
        {
            exposure = dayExposure;
            temp = dayTemperature;
            colorFilter = Color.white;
        }
        else if (progress >= DAY_END && progress < EVENING_END)
        {
            float segmentProgress = (progress - DAY_END) / (EVENING_END - DAY_END);

            exposure = Mathf.Lerp(dayExposure, eveningExposure, segmentProgress);
            temp = Mathf.Lerp(dayTemperature, eveningTemperature, segmentProgress);
            colorFilter = Color.Lerp(Color.white, eveningTint, segmentProgress);
        }
        else
        {
            exposure = nightExposure;
            temp = nightTemperature;
            colorFilter = new Color(0.6f, 0.6f, 0.9f);
        }
    }
}