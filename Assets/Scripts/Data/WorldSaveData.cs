using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BuildingSaveData
{
    public string buildingID;
    public int currentLevel;
}

[System.Serializable]
public class PlacedObjectSaveData
{
    public string objectPrefabID; // Hangi obje üretilecek? (Örn: "Chest_Wood", "Crop_Tomato")
    public Vector3 position;      // Nereye konulmuþ?

    // Objenin kendine has verileri (Sandýk envanteri, tarlanýn sulanma durumu vb.)
    // Ýleride bu string'in içine özel bir JSON gömebiliriz.
    public string customStateData;
}

[System.Serializable]
public class WorldSaveData
{
    public string worldName;
    public int currentDay;
    public int difficultyLevel;

    [Header("Konum Bilgisi")]
    public Vector3 lastBedPosition;

    [Header("Dünya Geliþimi (Binalar)")]
    public List<BuildingSaveData> upgradedBuildings = new List<BuildingSaveData>();

    [Header("Oyuncu Tarafýndan Yerleþtirilenler")]
    public List<PlacedObjectSaveData> placedObjects = new List<PlacedObjectSaveData>();
}