using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance { get; private set; }

    // O anki sahnedeki SpawnPoint'lerin listesi
    private Dictionary<string, Transform> activeSpawnPoints = new Dictionary<string, Transform>();

    // Gidilecek hedefin ID'si
    private string targetSpawnID;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // Doðuþ noktalarý sahneye gelince kendini bu listeye yazdýrýr
    public void RegisterSpawnPoint(string id, Transform pointTransform)
    {
        if (!activeSpawnPoints.ContainsKey(id))
        {
            activeSpawnPoints.Add(id, pointTransform);
        }
    }
    public void UnregisterSpawnPoint(string id)
    {
        if (activeSpawnPoints.ContainsKey(id))
        {
            activeSpawnPoints.Remove(id);
        }
    }
    // Portaldan geçildiðinde çaðrýlacak metod
    public void LoadNewScene(string sceneName, string spawnID)
    {
        targetSpawnID = spawnID;
        SceneManager.LoadScene(sceneName);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Hedef ID doluysa ve listede o ID'ye sahip bir yer varsa:
        if (!string.IsNullOrEmpty(targetSpawnID) && activeSpawnPoints.TryGetValue(targetSpawnID, out Transform targetPoint))
        {
            if (PlayerMovementController.Instance != null)
            {
                PlayerMovementController.Instance.transform.position = targetPoint.position;
                PlayerMovementController.Instance.ResetLastImagePosition();

                var vCam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
                if (vCam != null)
                {
                    vCam.Follow = PlayerMovementController.Instance.transform;
                    // Kameranýn ýþýnlanan karakteri ararken ekraný "kaydýrmasýný" engellemek için:
                    vCam.ForceCameraPosition(targetPoint.position, Quaternion.identity);
                }
                // Ýþlem bitti, hedefi temizle
                targetSpawnID = null;
            }
        }
    }
}