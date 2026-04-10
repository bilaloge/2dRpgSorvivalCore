using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class SceneLoadManager : MonoBehaviour
{
    [Inject] private DiContainer _container; // Prefablarý Zenject ile oluþturmak için motor(yeni öðrendim :D)
    [Inject] private GameManager _gameManager;
    [Inject] private PlayerStats _playerStats;
    [Inject] private PlayerDataManager _playerDataManager;

    [Inject(Id = "Player")] private GameObject _playerPrefab;
    // O anki sahnedeki SpawnPoint'lerin listesi

    // Gidilecek hedefin ID'si
    private string targetSpawnID;
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void Start()
    {
        if (_gameManager.MovementController == null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (!currentScene.ToLower().Contains("mainmenu"))
            {
                SpawnAndRegisterPlayer();
            }
        }
    }
    public void LoadNewScene(string sceneName, string spawnID)//portaldan geçilince çaðýrýlacak metod
    {
        targetSpawnID = spawnID;
        SceneManager.LoadScene(sceneName);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.ToLower().Contains("mainmenu")) return;

        if (_gameManager.MovementController == null)
        {
            SpawnAndRegisterPlayer();
        }
        else//karakter zaten oluþturulmuþsa
        {
            PositionPlayer();
            AssignCameraToPlayer();
            targetSpawnID = null;
        }
    }
    private void SpawnAndRegisterPlayer()
    {
        GameObject playerObj = _container.InstantiatePrefab(_playerPrefab);//karakter zenject ile prefabdan oluþturuluyor

        var mc = playerObj.GetComponentInChildren<PlayerMovementController>();
        var hs = playerObj.GetComponentInChildren<HealthSystem>();

        _gameManager.RegisterPlayer(mc, hs, _playerStats);
        //karakteri pozisyonlamazsak olmaz
        PositionPlayer();
        AssignCameraToPlayer();

        targetSpawnID = null;
    }
    private void PositionPlayer()
    {
        if (_gameManager.MovementController == null)
        {
            Debug.LogError("PositionPlayer baþarýsýz: MovementController hala NULL!");
            return;
        }
        SpawnPointController[] allSpawnPoints = Object.FindObjectsByType<SpawnPointController>(FindObjectsSortMode.None);

        string idToLook = "Default";

        if(!string.IsNullOrEmpty(targetSpawnID))
        {
            // A. Portaldan geldiyse kesinlikle portala git
            idToLook = targetSpawnID;
        }
        else if (_playerDataManager != null && !string.IsNullOrEmpty(_playerDataManager.lastSpawnID))
        {
            // B. Portaldan gelmediyse ama kayýtta bir yatak ID'si varsa yataða git
            idToLook = _playerDataManager.lastSpawnID;
        }
        Transform targetPoint = null;

        foreach (var sp in allSpawnPoints)
        {
            // Trim() -> Baþýndaki sonundaki boþluklarý siler
            // ToLower() -> Her þeyi küçük harfe çevirip öyle bakar (Hata payýný sýfýrlar)
            if (sp.SpawnID.Trim().ToLower() == idToLook.Trim().ToLower())
            {
                targetPoint = sp.transform;
                break;
            }
        }
        if (targetPoint == null)
        {
            Debug.LogWarning($"Hedef nokta ({idToLook}) bulunamadý! Default'a dönülüyor.");
            foreach (var sp in allSpawnPoints)
            {
                if (sp.SpawnID == "Default")
                {
                    targetPoint = sp.transform;
                    break;
                }
            }
        }
        if (targetPoint != null)//gidilmesi gereken spawnpoint tarandý ve bulundu yukarýda. þimdi de ýþýnlama
        {
            var playerTransform = _gameManager.MovementController.transform;
            playerTransform.position = targetPoint.position;
            _gameManager.MovementController.ResetLastImagePosition();
        }
        else
        {
            Debug.LogError("Sahne yüklendi ama hiçbir spawn noktasý (Default dahil) bulunamadý!");
        }
    }
    private void AssignCameraToPlayer()
    {
        var vCam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vCam != null && _gameManager.MovementController != null)
        {
            vCam.Follow = _gameManager.MovementController.transform;
            vCam.ForceCameraPosition(_gameManager.MovementController.transform.position, Quaternion.identity);
        }
    }
}