using UnityEngine;

public class PortalController : MonoBehaviour
{
    [Header("Hedef Ayarlarý")]
    [Tooltip("Gidilecek sahnenin tam adý (Build Settings'te ekli olmalý)")]
    [SerializeField] private string targetSceneName;

    [Tooltip("Gidilecek sahnedeki SpawnPoint objesinin ID'si")]
    [SerializeField] private string targetSpawnID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneLoadManager.Instance.LoadNewScene(targetSceneName, targetSpawnID);
        }
    }
}
