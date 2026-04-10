using UnityEngine;
using TMPro;
using System.Collections;
using Zenject;

public class ClockUIController : MonoBehaviour
{
    [Inject] private TimeManager _timeManager;

    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private RectTransform dayNightWheel;
    [SerializeField] private float rotationDuration = 0.5f;

    private Coroutine rotationCoroutine;
    private void Start()
    {
        if (_timeManager != null)
        {
            // 1. O ANKÝ ZAMANI HEMEN YAZDIR (00:00 kalmamasý için)
            HandleTimeChange(_timeManager.Hour, _timeManager.Minute);

            // 2. ABONE OL (Bundan sonraki her dakika deðiþimini dinle)
            _timeManager.OnTimeChanged += HandleTimeChange;
        }
    }
    private void OnDisable()
    {
        if (_timeManager != null)
            _timeManager.OnTimeChanged -= HandleTimeChange;
    }
    private void HandleTimeChange(int hours, int minutes)
    {
        if (clockText != null)
            clockText.text = $"{hours:D2}:{minutes:D2}";

        if (dayNightWheel != null)
        {
            float targetZ = CalculateTargetZ(hours, minutes);

            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
            rotationCoroutine = StartCoroutine(SmoothRotate(targetZ));
        }
    }
    private IEnumerator SmoothRotate(float targetZ)
    {
        // KULLANDIÐIM ASSET YAMUK OLDUÐU ÝÇÝN -90 DERECE DÖNDÜRDÜM. BAÞKA ASSET KOYUNCA BUNU DÜZELT!!!!!
        float finalTargetZ = targetZ; //- 90f;
        Quaternion startRot = dayNightWheel.rotation;
        Quaternion endRot = Quaternion.Euler(0, 0, finalTargetZ);
        float time = 0;

        while (time < 1f)
        {
            // zaman durduðunda, timeScale 0 olduðunda bu döngü de Time.deltaTime sayesinde donar
            time += Time.deltaTime / rotationDuration;
            dayNightWheel.rotation = Quaternion.Lerp(startRot, endRot, time);
            yield return null;
        }
        dayNightWheel.rotation = endRot;
    }
    private float CalculateTargetZ(int hours, int minutes)
    {
        float currentTime = hours + (minutes / 60f);
        float startHour = 6f;
        float elapsed = (currentTime >= startHour) ? (currentTime - startHour) : (24f - startHour + currentTime);
        return -(elapsed / 20f) * 360f;
    }
}
