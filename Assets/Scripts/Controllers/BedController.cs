using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Zenject;

public class BedController : MonoBehaviour
{
    [Inject] private TimeManager _timeManager;
    [Inject] private PlayerDataManager _playerDataManager;
    [Inject] private GameDataManager _gameDataManager;

    [Header("Ayarlar")]
    [Tooltip("Bu yata��n yan�ndaki SpawnPoint'in ID'si (�rn: Home_Bed_Spawn)")]
    [SerializeField] private string bedSpawnID;
    [SerializeField] private GameObject sleepChoicePanel;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OpenSleepMenu();
        }
    }
    private void OpenSleepMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (sleepChoicePanel != null)
        {
            sleepChoicePanel.SetActive(true);
            //Time.timeScale = 0f; // BUNU A�INCA TU�LARA BASILMIYOR. ZAMAN DURUNCA CANVAS DA DONUYOR. BUNA B�R ��Z�M BULANA KADAR B�YLE KALSIN
        }
    }
    public void OnSleepSelected()
    {
        Time.timeScale = 1f; // Zaman� geri ba�lat
        sleepChoicePanel.SetActive(false);

        _playerDataManager.UpdateLastLocation(SceneManager.GetActiveScene().name, bedSpawnID);

        _gameDataManager.EndDayAndSave(true);
        _timeManager.SkipToNextDay();

    }
    public void OnRestSelected()
    {
        Time.timeScale = 1f;
        sleepChoicePanel.SetActive(false);

        _playerDataManager.currentEnergy = Mathf.Min(_playerDataManager.currentEnergy + 30, 100);
        Debug.Log("Dinlenildi, enerji bir miktar tazelendi.");
    }
    public void Exit()
    {
        sleepChoicePanel.SetActive(false);
    }
}