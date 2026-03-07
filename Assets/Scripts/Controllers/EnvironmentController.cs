using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using Unity.VisualScripting;


public class EnvironmentController : MonoBehaviour
{
    [Header("Post-Processing")]
    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private float dayExposure = 1.2f;
    [SerializeField] private float nightExposure = 0.3f;
    [SerializeField] private float dayTemperature = 20f;
    [SerializeField] private float nightTemperature = -20f;
    [SerializeField] private float transitionDuration = 1.0f;

    private ColorAdjustments colorAdjustments;
    private WhiteBalance whiteBalance;

    private const int DAY_START = 6;
    private const int DAY_END = 14;//akþamüstü, ikindi. kalk namaz kýl
    private const int EVENING_END = 18;

    private Coroutine lightingCoroutine;

    private void Awake()
    {
        if (postProcessingVolume != null)
        {
            postProcessingVolume.profile.TryGet(out colorAdjustments);
            postProcessingVolume.profile.TryGet(out whiteBalance);
        }
    }
    private void OnEnable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged += HandleEnvironmentUpdate;
        }
            
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= HandleEnvironmentUpdate;
        }
    }
    private void HandleEnvironmentUpdate(int hours, int minutes)
    {
        float timeOfDay = hours + minutes / 60f;
        CalculateTargetValues(timeOfDay, out float targetExp, out float targetTemp);

        if (lightingCoroutine != null) StopCoroutine(lightingCoroutine);
        lightingCoroutine = StartCoroutine(SmoothTransition(targetExp, targetTemp));
    }
    private void CalculateTargetValues(float time, out float exposure, out float temp)
    {
        if (time >= DAY_START && time < DAY_END)
        {
            exposure = dayExposure;
            temp = dayTemperature;
        }
        else if (time >= DAY_END && time < EVENING_END)
        {
            float progress = (time - DAY_END) / (EVENING_END - DAY_END);
            exposure = Mathf.Lerp(dayExposure, nightExposure, progress);
            temp = Mathf.Lerp(dayTemperature, nightTemperature, progress);//Lerp ile geçiþler þak diye deðil yumuþak bir þekilde olur. daha hoþ gözükür 
        }
        else
        {
            exposure = nightExposure;
            temp = nightTemperature;

            if (time < DAY_START && time >= 0) // Gece yarýsýndan sonra
            {
                float dawnProgress = time / DAY_START;
                exposure = Mathf.Lerp(nightExposure, dayExposure, dawnProgress);
                temp = Mathf.Lerp(nightTemperature, dayTemperature, dawnProgress);
            }
        }
    }
    private IEnumerator SmoothTransition(float targetExp, float targetTemp)
    {
        if (colorAdjustments == null || whiteBalance == null) yield break;

        float startExp = colorAdjustments.postExposure.value;
        float startTemp = whiteBalance.temperature.value;
        float elapsed = 0;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime / transitionDuration;
            colorAdjustments.postExposure.value = Mathf.Lerp(startExp, targetExp, elapsed);
            whiteBalance.temperature.value = Mathf.Lerp(startTemp, targetTemp, elapsed);
            yield return null;
        }
    }
}
