using UnityEngine;
using TMPro;

public class ClockUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clockText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayNightCycleUpdated += OnDayNightCycleHandler;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayNightCycleUpdated -= OnDayNightCycleHandler;
        }
    }

    private void OnDayNightCycleHandler(int hours, int minutes, float normalizedTime)
    {
        clockText.text = $"{hours:D2}:{minutes:D2}";
    }
}
