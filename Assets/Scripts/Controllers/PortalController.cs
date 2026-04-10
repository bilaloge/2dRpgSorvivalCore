using UnityEngine;
using Zenject;

/// <summary>
/// ?ki bölge aras? do?rudan geçi?: oyuncu tetik alan?na girdi?inde sahne yüklenir (E gerekmez).
/// </summary>
public class PortalController : MonoBehaviour
{
    [Inject] private SceneLoadManager _sceneLoadManager;

    [Header("Hedef")]
    [Tooltip("Gidilecek sahnenin ad? (Build Settings'te ekli olmal?)")]
    [SerializeField] private string targetSceneName;

    [Tooltip("Gidilecek sahnedeki SpawnPoint ID'si")]
    [SerializeField] private string targetSpawnID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            _sceneLoadManager.LoadNewScene(targetSceneName, targetSpawnID);
    }
}
