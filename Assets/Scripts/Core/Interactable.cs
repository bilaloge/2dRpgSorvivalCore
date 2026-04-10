using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Yakınlık / imleç ile odaklanılabilen dünya etkileşimleri için taban sınıf.
/// <see cref="PlayerInteractionPrompt"/> adayları <b>Interactable</b> katmanındaki 2D collider'lar üzerinden toplar.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    public const string InteractableLayerName = "Interactable";

    [SerializeField] private Transform promptAnchor;
    [SerializeField] private string promptText = "E";

    public string PromptText => promptText;

    public Vector3 GetPromptWorldPosition() =>
        (promptAnchor != null ? promptAnchor.position : transform.position) + new Vector3(0.5f, 0, 0);

    // Oyuncu, bu nesne odaktayken E / Interact bastığında çağrılır.
    public abstract void OnPlayerInteract();

    protected virtual void Awake()
    {
        int layer = LayerMask.NameToLayer(InteractableLayerName);
        if (layer >= 0 && gameObject.layer == 0)
            gameObject.layer = layer;
    }
}
