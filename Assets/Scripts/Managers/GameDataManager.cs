using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Zenject;
public class GameDataManager : MonoBehaviour
{
    [Inject] private GameManager _gameManager;
    [Inject] private PlayerDataManager _playerDataManager;
    [Inject] private SceneLoadManager _sceneLoadManager;

    [Header("World Data (Runtime)")]
    private WorldSaveData _worldData = new WorldSaveData();
    private string _worldSavePath;
    private List<PlacedObjectSaveData> _temporaryPlacedObjects = new List<PlacedObjectSaveData>();

    public int currentDifficulty { get; private set; }
    private void Awake()
    {
        _worldSavePath = Path.Combine(Application.persistentDataPath, "World_Main.json");
    }
    public void EndDayAndSave(bool sleptInBed)
    {
        _worldData.currentDay++;//günü artýr

        if(sleptInBed)
        {
            _playerDataManager.currentHealth = _gameManager.PlayerStats.TotalMaxHealth;
            _playerDataManager.currentEnergy = _gameManager.PlayerStats.TotalMaxEnergy;
        }
        else
        {
            ApplyPassedOutDebuffs();
        }

        _gameManager.HealthSystem.SleepAndRestore(sleptInBed);

        //DÜNYA VERÝLERÝNÝ TOPLA
        CollectWorldDynamicData();

        //Karakter Dosyasýný Kaydet
        _playerDataManager.SaveCharacter();

        //Dünya Dosyasýný Kaydet
        SaveWorldToFile();

        string targetScene = _playerDataManager.lastSceneName;
        string targetID = _playerDataManager.lastSpawnID;

        _sceneLoadManager.LoadNewScene(targetScene, targetID);
    }
    private void ApplyPassedOutDebuffs()//sýzma senaryosunda
    {
        // Örn: inventorydeki itemler ve kuþanýlan itemlerden rastgele 1-2 item çalýnsýn. bu itemlerin kayý tutlarak karakola gittiðinde bunlarýn geri gelmesi saðlanacak. bunun için sonra kod yazýcam
        //þu an için test amacý ile enerjý yarýda baþlat
        _playerDataManager.currentEnergy = _gameManager.PlayerStats.TotalMaxEnergy/2;
        if (string.IsNullOrEmpty(_playerDataManager.lastSpawnID))
        {
            _playerDataManager.UpdateLastLocation("StartZone", "Default");
            Debug.Log("Hiç yatak kaydý yok, baþlangýca gönderildi.");
        }
        else
        {
            Debug.Log($"Sýzýldý. Son kayýtlý yatakta ({_playerDataManager.lastSpawnID}) uyanýlacak.");
        }
    }
    private void CollectWorldDynamicData()
    {
        // Bina seviyeleri ve zindan durumlarý buraya iþlenecek
        Debug.Log("Sistem: Dinamik dünya verileri toplandý.");
    }
    public void RegisterPlacedObject(string id, Vector3 pos, string data = "")//BURADA OTLAR SANDIKLAR FLN ÝÞLENÝR
    {
        // Listede zaten varsa üzerine yazmak veya yeni eklemek için küçük bir kontrol
        _temporaryPlacedObjects.Add(new PlacedObjectSaveData
        {
            objectPrefabID = id,
            position = pos,
            customStateData = data
        });
    }
    public void UnregisterPlacedObject(Vector3 pos)//BURDA DA OTLAR SANDIKLAR FLN SÝLÝNÝR. KIRILMA SENARTOLARI
    {
        // Belirli bir pozisyondaki nesneyi listeden kaldýr (Örn: Sandýk kýrýldýðýnda)
        _worldData.placedObjects.RemoveAll(x => x.position == pos);
    }
    private void SaveWorldToFile()
    {
        // Kayýt anýnda yapýlacak tek þey: RAM'deki listeyi Save paketine aktarmak.
        _worldData.placedObjects = new List<PlacedObjectSaveData>(_temporaryPlacedObjects);

        string json = JsonUtility.ToJson(_worldData, true);
        File.WriteAllText(_worldSavePath, json);
        Debug.Log($"Dünya Kaydedildi. Gün: {_worldData.currentDay}");
    }
    public void CreateNewWorld(string worldName, int difficulty)
    {
        _worldData = new WorldSaveData();
        _worldData.worldName = worldName;
        _worldData.currentDay = 1;
        _worldData.difficultyLevel = difficulty;
        _worldData.isNewWorld = true; // Karakterin sahilde doðmasýný tetikler

        SaveWorldToFile();
        _playerDataManager.ResetToDefaultStats();
        Debug.Log($"{worldName} adýnda yeni bir evren yaratýldý.");
    }
    public void LoadWorld()
    {
        if (!File.Exists(_worldSavePath)) return;
        string json = File.ReadAllText(_worldSavePath);
        _worldData = JsonUtility.FromJson<WorldSaveData>(json);
        currentDifficulty = _worldData.difficultyLevel;
        _temporaryPlacedObjects = new List<PlacedObjectSaveData>(_worldData.placedObjects);
    }
    public void ContinueLatestGame()
    {
        LoadWorld();
        _playerDataManager.LoadCharacter();

        string targetScene;
        string targetID;

        Debug.Log($"<color=green>Oyun Devam Ediyor. Mevcut Gün: {_worldData.currentDay}</color>");

        if (_worldData.isNewWorld)
        {
            targetScene = "StartZone";
            targetID = "Default";

            _worldData.isNewWorld = false;
            SaveWorldToFile();
        }
        else
        {
            targetScene = _playerDataManager.lastSceneName;
            targetID = _playerDataManager.lastSpawnID;
        }

        _sceneLoadManager.LoadNewScene(targetScene, targetID);  
    }
}