using UnityEngine;

public class UniversalPersistentObject : MonoBehaviour
{
    //Eðer bu objeden yeni sahnede bir tane daha varsa, yenisini siler
    [SerializeField] private bool useSingleton = true;
    private static System.Collections.Generic.Dictionary<string, UniversalPersistentObject> instances = new();

    private void Awake()
    {
        if (useSingleton)
        {
            string objectName = gameObject.name;

            if (instances.ContainsKey(objectName) && instances[objectName] != this)
            {
                Destroy(gameObject);
                return;
            }

            instances[objectName] = this;
        }

        transform.SetParent(null); // Eðer bir child ise root'a çýkar (DDOL kuralý)
        DontDestroyOnLoad(gameObject);
    }
}