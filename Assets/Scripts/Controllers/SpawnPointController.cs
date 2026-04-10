using UnityEngine;
public class SpawnPointController : MonoBehaviour
{
    [Tooltip("BUNU KOYDUÐUN OBJENÝN ID'sini DEÐÝÞTÝR. KUZEY KÖY KAPISI FLN. DAHA SONRA PORTAL SC'SÝ BUNA GÖRE IÞINLIYCAK")]
    [SerializeField] private string spawnID;
    public string SpawnID => spawnID;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
        // Ýsmi kafasýnýn üzerinde yazdýralým (Sadece Editor'de görünür)
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, spawnID);
    }
#endif
}