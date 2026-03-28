using UnityEngine;

public class SpawnPointController : MonoBehaviour
{
    [Tooltip("BUNU KOYDUÐUN OBJENÝN ID'sini DEÐÝÞTÝR. KUZEY KÖY KAPISI FLN. DAHA SONRA PORTAL SC'SÝ BUNA GÖRE IÞINLIYCAK")]
    [SerializeField] private string spawnID;

    private void OnEnable()
    {
        // Obje aktifleþtiðinde yöneticiye haber ver
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.RegisterSpawnPoint(spawnID, transform);
        }
    }

    private void OnDisable()
    {
        // Sahne deðiþirken yönetici listesinden kendini sil
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.UnregisterSpawnPoint(spawnID);
        }
    }

    private void OnDrawGizmos()
    {
        // Editörde noktayý yeþil bir çember olarak göster
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}