using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImagePoolScript : MonoBehaviour
{
    [SerializeField] private GameObject afterImagePrefab;
    [SerializeField] private int initialPoolSize = 10; // Baþlangýç boyutu

    private Queue<GameObject> availableObjects = new Queue<GameObject>();
    public static AfterImagePoolScript Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GrowPool(initialPoolSize);
        }
    }
    private void GrowPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject instanceToAdd = Instantiate(afterImagePrefab);
            instanceToAdd.transform.SetParent(transform);
            instanceToAdd.SetActive(false);
            AddToPool(instanceToAdd);
        }
    }
    public void AddToPool(GameObject instance)
    {
        if (this == null || gameObject == null)
        {
            // Eðer havuz bir þekilde yok olduysa, objeyi dünyada býrakma, direkt imha et.
            Destroy(instance);
            return;
        }
        instance.transform.SetParent(transform);
        instance.SetActive(false);
        if (!availableObjects.Contains(instance))
        {
            availableObjects.Enqueue(instance);
        }
    }
    public GameObject GetFromPool()
    {
        if (availableObjects.Count == 0)
        {
            GrowPool(initialPoolSize);
        }
        GameObject instance = availableObjects.Dequeue();
        instance.transform.SetParent(null);
        instance.SetActive(true);
        return instance;
    }
}
