using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Transform playerTransform;

    private WorldSaveData _worldData = new WorldSaveData();
    private string _worldSavePath;

    public int currentDifficulty { get; private set; }

    private List<PlacedObjectSaveData> _temporaryPlacedObjects = new List<PlacedObjectSaveData>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _worldSavePath = Path.Combine(Application.persistentDataPath, "World_Main.json");
    }
    public void EndDayAndSave(bool sleptInBed, Vector3 bedPosition)
    {
        _worldData.currentDay++;//günü artýr

        healthSystem.SleepAndRestore(sleptInBed);//statlarý yenile

        if (sleptInBed)
        {
            _worldData.lastBedPosition = bedPosition;
        }
        else
        {
            ApplyPassedOutDebuffs();
        }

        //DÜNYA VERÝLERÝNÝ TOPLA
        CollectWorldDynamicData();

        //Karakter Dosyasýný Kaydet
        PlayerDataManager.Instance.SaveCharacter();

        //Dünya Dosyasýný Kaydet
        SaveWorldToFile();

        //Uyanýþ ve Iþýnlanma
        HandleWakeUp();
    }
    private void ApplyPassedOutDebuffs()//sýzma senaryosunda
    {
        // Örn: inventorydeki itemler ve kuþanýlan itemlerden rastgele 1-2 item çalýnsýn. bu itemlerin kayý tutlarak karakola gittiðinde bunlarýn geri gelmesi saðlanacak. bunun için sonra kod yazýcam
        //þu an için test amacý ile enerjý yarýda baþlat
        PlayerDataManager.Instance.currentEnergy /= 2;
        Debug.LogWarning("Sýzma debufflarý uygulandý ve kaydediliyor...");
    }
    private void HandleWakeUp()
    {
        if (_worldData.lastBedPosition != Vector3.zero)
            playerTransform.position = _worldData.lastBedPosition;

        healthSystem.NotifyAll(); // UI'ý tazele
    }
    private void CollectWorldDynamicData()
    {
        // Sadece 'Dirty' (deðiþmiþ) bina seviyelerini kontrol eder.

        // Ekinler, kaynaklar ve hava durumu tamamen "Gün Sayýsý" ve "Seed" üzerinden matematiksel olarak hesaplandýðý için burada ek bir tarama yapmýyoruz.

        Debug.Log("Sistem: Gün bazlý veriler (Ekinler/Binalar) senkronize edildi.");
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
        _worldData.lastBedPosition = Vector3.zero;

        currentDifficulty = difficulty;

        SaveWorldToFile();
        Debug.Log($"{worldName} adýnda yeni bir evren yaratýldý.");
    }
    public void LoadWorld()
    {
        if (File.Exists(_worldSavePath))
        {
            string json = File.ReadAllText(_worldSavePath);
            _worldData = JsonUtility.FromJson<WorldSaveData>(json);

            // jsondan gelen deðeri runtime a aktararak her seferinde json dosyasýna eriþme zorunluluðunu ortadan kaldýrýyoruz. oyun zorluðu bir çok hesaplamada kullanýlacaðý için bir çok defa çaðýrýlacak.
            //bu yüzden runtime(ram içerisinde) hazýr bulunmasý daha mantýklý
            currentDifficulty = _worldData.difficultyLevel;

            // Geçici listeyi, diskten gelen verilerle doldur (Hibrit sistem)
            _temporaryPlacedObjects = new List<PlacedObjectSaveData>(_worldData.placedObjects);

            Debug.Log($"Dünya yüklendi. Gün: {_worldData.currentDay}, Zorluk: {currentDifficulty}");
        }
        else
        {
            Debug.LogError("Dünya dosyasý bulunamadý!");
        }
    }
}