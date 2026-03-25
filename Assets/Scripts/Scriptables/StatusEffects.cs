using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Status Effect/Create Status Effect")]
public class StatusEffects : ScriptableObject
{
    public string effectName;
    public Sprite icon;

    [Header("Süre Ayarlarý")]
    public float durationInSeconds; // Etki ne kadar sürecek?
    public bool isPermanent;        // Gün bitene kadar kalýcý mý?

    [Header("Etki Deðerleri")]
    public int healthChange;        // Toplam can deðiþimi
    public float speedModifier = 1f; // Hareket hýzý çarpaný (1 = normal)
    public int infectionChange;     // Enfeksiyon artýþ/azalýþý

    [Header("Görsel Efekt")]
    public GameObject vfxPrefab;    // Karakterin üzerinde çýkacak efekt
}