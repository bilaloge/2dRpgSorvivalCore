using UnityEngine;
using Zenject;

/// <summary>
/// Sahne geçişi için kapı: yakında E göstergesi; geçiş yalnızca etkileşim tuşu ile olur (temasta otomatik değil).
/// </summary>
public class DoorController : Interactable
{
    [Inject] private SceneLoadManager _sceneLoadManager;

    [Header("Hedef")]
    [Tooltip("Gidilecek sahnenin adı (Build Settings'te ekli olmalı)")]
    [SerializeField] private string targetSceneName;

    [Tooltip("Gidilecek sahnedeki SpawnPoint ID'si")]
    [SerializeField] private string targetSpawnID;

    public override void OnPlayerInteract()
    {
        _sceneLoadManager.LoadNewScene(targetSceneName, targetSpawnID);
    }
}
