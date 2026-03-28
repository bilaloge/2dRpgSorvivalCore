using UnityEngine;
using UnityEngine.SceneManagement;

public class BedController : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Bu yataðýn yanýndaki SpawnPoint'in ID'si (Örn: Home_Bed_Spawn)")]
    [SerializeField] private string bedSpawnID;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Sadece "Player" tagine sahip obje girince çalýþsýn
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Yataða girildi, uyuma iþlemi baþlatýlýyor...");
            UseBed();
        }
    }
    public void UseBed()
    {
        // 1. Karakterin uyanacaðý yeri güncelle
        PlayerDataManager.Instance.UpdateLastLocation(
            SceneManager.GetActiveScene().name,
            bedSpawnID
        );

        // 2. Günü bitir (true = yatakta uyudu)
        GameDataManager.Instance.EndDayAndSave(true);
        TimeManager.Instance.SkipToNextDay();

        Debug.Log($"Yatakta uyundu. Konum kaydedildi: {bedSpawnID}");
    }
}