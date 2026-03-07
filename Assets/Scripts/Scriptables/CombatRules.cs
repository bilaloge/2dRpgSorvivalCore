using UnityEngine;

[CreateAssetMenu(fileName = "CombatRules", menuName = "PlayerStats/CombatRules")]
public class CombatRules : ScriptableObject
{
    // ÞU an Hasardan zýrhý çýkar ve en az 1 vur. kuralý
    //AMA BU ARMOR % VE ÝVMELÝ OLARAK ETKÝ EDECEK. 1 ARMOR 1 HASARI ENGELLEMÝYCEK.
    public int CalculateDamage(float rawDamage, int totalArmor)
    {
        int damage = Mathf.RoundToInt(rawDamage);
        int finalDamage = damage - totalArmor;

        // Kural: Hasar asla 1'in altýna düþemez (veya 0)
        return Mathf.Max(1, finalDamage);
    }

    // Ýleride buraya eklenebilecek örnek kural:
    //critik vurma sistemi
}