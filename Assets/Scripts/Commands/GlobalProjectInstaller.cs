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
    [SerializeField] private GameObject eventSystemPrefab;
    [SerializeField] private PlayerStats playerStats;
    public override void InstallBindings()
    {
        Container.Bind<GameObject>().WithId("Player").FromInstance(playerPrefab);
        Container.BindInstance(playerStats).AsSingle();
        // Bu komutlar, GameManager ve TimeManager'ý hiyerarþide bulur, 
        // sisteme tanýtýr ve onlara 'Sen bir Singletonsýn' der.
        BindGlobalManager<GameManager>(gameManagerPrefab);
        BindGlobalManager<TimeManager>(timeManagerPrefab);
        BindGlobalManager<SceneLoadManager>(sceneLoadManagerPrefab);
        BindGlobalManager<GameDataManager>(gameDataManagerPrefab);
        BindGlobalManager<PlayerDataManager>(playerDataManagerPrefab);
        BindGlobalManager<EventSystem>(eventSystemPrefab);
    }
    private void BindGlobalManager<T>(GameObject prefab) where T : Component
    {
        Container.Bind<T>()
            .FromComponentInNewPrefab(prefab)
            .AsSingle()
            .NonLazy(); // Oyun açýlýr açýlmaz belleðe yükle
    }
}