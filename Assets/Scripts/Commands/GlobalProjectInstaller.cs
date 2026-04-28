using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class GlobalProjectInstaller : MonoInstaller
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject timeManagerPrefab;
    [SerializeField] private GameObject sceneLoadManagerPrefab;
    [SerializeField] private GameObject gameDataManagerPrefab;
    [SerializeField] private GameObject playerDataManagerPrefab;
    [SerializeField] private GameObject playerUIPrefab;
    [SerializeField] private GameObject eventSystemPrefab;

    [Header("ScriptableObject Referanslarý")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private ItemDatabase itemDatabase;
    public override void InstallBindings()
    {
        Container.BindInstance(playerStats).AsSingle();

        itemDatabase.Initialize();// ID lookup sözlüðünü hazýrla
        Container.BindInstance(itemDatabase).AsSingle();

        Container.BindInterfacesAndSelfTo<InventoryManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<EquipmentManager>().AsSingle().NonLazy();

        // MonoBehaviour manager'lar
        Container.Bind<GameObject>().WithId("Player").FromInstance(playerPrefab);
        BindGlobalManager<GameManager>(gameManagerPrefab);
        BindGlobalManager<TimeManager>(timeManagerPrefab);
        BindGlobalManager<SceneLoadManager>(sceneLoadManagerPrefab);
        BindGlobalManager<GameDataManager>(gameDataManagerPrefab);
        BindGlobalManager<PlayerDataManager>(playerDataManagerPrefab);
        BindGlobalManager<EventSystem>(eventSystemPrefab);

        // Her sahneye UI spawn et, tüm child component'ler otomatik inject edilir
        BindGlobalManager<PlayerUIController>(playerUIPrefab);
    }
    private void BindGlobalManager<T>(GameObject prefab) where T : Component
    {
        Container.Bind<T>()
            .FromComponentInNewPrefab(prefab)
            .AsSingle()
            .NonLazy(); // Oyun açýlýr açýlmaz belleðe yükle
    }
}