using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using Zenject;

/// <summary>
/// Odaklanılan <see cref="Interactable"/> üzerinde dünya uzayında E göstergesi (en yakın + imleç önceliği)
/// ve aynı odak için E / Interact ile <see cref="Interactable.OnPlayerInteract"/> tetikler.
/// Adaylar: oyuncu etrafında <see cref="interactionRadius"/> içindeki <b>Interactable</b> katmanı collider'ları
/// (<see cref="Physics2D.OverlapCircle"/>); imleç için <see cref="Physics2D.OverlapPoint"/>.
/// </summary>
[DisallowMultipleComponent]
public class PlayerInteractionPrompt : MonoBehaviour
{
    private static readonly List<Collider2D> OverlapScratch = new List<Collider2D>(32);

    [Inject(Optional = true)] private GameManager _gameManager;

    [Tooltip("Etkileşim yarıçapı (OverlapCircle ile aday daraltma).")]
    [SerializeField] private float interactionRadius = 2.5f;

    [SerializeField] private Vector3 worldPromptOffset = new Vector3(0f, 0.85f, 0f);

    [Tooltip("Boşsa 'Interactable' katmanı kullanılır.")]
    [SerializeField] private LayerMask interactableLayerMask;

    [SerializeField] private TextMeshPro worldLabel;

    private Transform _playerTransform;
    private Camera _mainCamera;
    private InputAction _interactAction;
    private Interactable _currentFocus;
    private ContactFilter2D _interactFilter;
    private bool _filterReady;

    private void Awake()
    {
        _playerTransform = transform;
        if (_gameManager == null)
            _gameManager = FindFirstObjectByType<GameManager>();
        EnsureInteractableLayerMask();
        InitInteractFilter();
        EnsureWorldLabel();
    }

    private void EnsureInteractableLayerMask()
    {
        if (interactableLayerMask.value != 0)
            return;
        int l = LayerMask.NameToLayer(Interactable.InteractableLayerName);
        if (l >= 0)
            interactableLayerMask = 1 << l;
    }

    private void InitInteractFilter()
    {
        if (_filterReady)
            return;
        _interactFilter = ContactFilter2D.noFilter;
        _interactFilter.useLayerMask = true;
        _interactFilter.SetLayerMask(interactableLayerMask);
        _interactFilter.useTriggers = true;
        _filterReady = true;
    }

    private void OnEnable()
    {
        _interactAction = new InputAction(type: InputActionType.Button);
        _interactAction.AddBinding("<Keyboard>/e");
        _interactAction.AddBinding("<Gamepad>/buttonNorth");
        _interactAction.Enable();
    }

    private void OnDisable()
    {
        if (_interactAction != null)
        {
            _interactAction.Disable();
            _interactAction.Dispose();
            _interactAction = null;
        }
    }

    private void EnsureWorldLabel()
    {
        if (worldLabel != null)
            return;

        var go = new GameObject("InteractionPromptLabel");
        go.transform.SetParent(transform, false);
        worldLabel = go.AddComponent<TextMeshPro>();
        worldLabel.text = "E";
        worldLabel.fontSize = 6f;
        worldLabel.alignment = TextAlignmentOptions.Center;
        if (TMP_Settings.defaultFontAsset != null)
            worldLabel.font = TMP_Settings.defaultFontAsset;

        var mr = worldLabel.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerName = "OverPlayer";
            mr.sortingOrder = 100;
        }
    }

    private bool IsGameplayReady()
    {
        return _gameManager != null && _gameManager.MovementController != null;
    }

    private void Update()
    {
        if (!IsGameplayReady())
        {
            _currentFocus = null;
            return;
        }

        if (_gameManager.CurrentState != GameState.Playing)
        {
            _currentFocus = null;
            return;
        }

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        float radiusSq = interactionRadius * interactionRadius;
        Vector3 playerPos = _playerTransform.position;
        _currentFocus = ResolveFocus(playerPos, radiusSq);

        if (_currentFocus == null || _interactAction == null)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!_interactAction.WasPressedThisFrame())
            return;

        _currentFocus.OnPlayerInteract();
    }

    private void LateUpdate()
    {
        if (!IsGameplayReady() || _gameManager.CurrentState != GameState.Playing)
        {
            worldLabel.gameObject.SetActive(false);
            return;
        }

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_currentFocus == null)
        {
            worldLabel.gameObject.SetActive(false);
            return;
        }

        worldLabel.gameObject.SetActive(true);
        worldLabel.text = string.IsNullOrEmpty(_currentFocus.PromptText) ? "E" : _currentFocus.PromptText;
        worldLabel.transform.position = _currentFocus.GetPromptWorldPosition() + worldPromptOffset;
    }

    private Interactable ResolveFocus(Vector3 playerPos, float radiusSq)
    {
        Interactable nearest = PickNearestInteractableInCircle(playerPos, radiusSq);

        bool skipHover = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        Mouse mouse = Mouse.current;
        if (!skipHover && _mainCamera != null && mouse != null)
        {
            Vector2 screen = mouse.position.ReadValue();
            float zDepth = Mathf.Abs(_mainCamera.transform.position.z);
            Vector3 mp = _mainCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, zDepth));
            mp.z = 0f;

            OverlapScratch.Clear();
            int count = Physics2D.OverlapPoint((Vector2)mp, _interactFilter, OverlapScratch);
            Interactable hoverPick = null;
            float hoverBestSq = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider2D col = OverlapScratch[i];
                if (col == null)
                    continue;

                Interactable it = col.GetComponentInParent<Interactable>();
                if (it == null || !it.isActiveAndEnabled)
                    continue;

                float d = ((Vector2)playerPos - (Vector2)it.GetPromptWorldPosition()).sqrMagnitude;
                if (d > radiusSq)
                    continue;

                if (d < hoverBestSq)
                {
                    hoverBestSq = d;
                    hoverPick = it;
                }
            }

            if (hoverPick != null)
                return hoverPick;
        }

        return nearest;
    }

    private Interactable PickNearestInteractableInCircle(Vector3 playerPos, float radiusSq)
    {
        OverlapScratch.Clear();
        int count = Physics2D.OverlapCircle((Vector2)playerPos, interactionRadius, _interactFilter, OverlapScratch);

        Interactable nearest = null;
        float nearestSq = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = OverlapScratch[i];
            if (col == null)
                continue;

            Interactable it = col.GetComponentInParent<Interactable>();
            if (it == null || !it.isActiveAndEnabled)
                continue;

            float d = ((Vector2)playerPos - (Vector2)it.GetPromptWorldPosition()).sqrMagnitude;
            if (d > radiusSq)
                continue;

            if (d < nearestSq)
            {
                nearestSq = d;
                nearest = it;
            }
        }

        return nearest;
    }
}
