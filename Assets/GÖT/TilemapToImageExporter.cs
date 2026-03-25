using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class TilemapToImageExporter : MonoBehaviour
{
    [Header("Ayarlar")]
    public Tilemap targetTilemap;
    public int pixelsPerUnit = 16; // Tile'larýnýzýn PPU deðeri (genelde 16, 32 veya 64 olur)
    public string fileName = "OyunHaritasi.png";

    [ContextMenu("Haritayý Resim Olarak Kaydet")]
    public void ExportTilemap()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("Lütfen bir Tilemap atayýn!");
            return;
        }

        // 1. Haritanýn sýnýrlarýný belirle
        BoundsInt bounds = targetTilemap.cellBounds;
        int width = bounds.size.x * pixelsPerUnit;
        int height = bounds.size.y * pixelsPerUnit;

        // 2. Boþ ve þeffaf bir Texture oluþtur
        Texture2D mapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Arkaplaný tamamen þeffaf yapalým
        Color[] clearColors = new Color[width * height];
        for (int i = 0; i < clearColors.Length; i++) clearColors[i] = Color.clear;
        mapTexture.SetPixels(clearColors);

        // 3. Her bir hücreyi tara
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = targetTilemap.GetTile(pos);

                if (tile != null)
                {
                    // Tile'ýn içindeki Sprite'ý bul
                    Sprite sprite = targetTilemap.GetSprite(pos);
                    if (sprite != null)
                    {
                        DrawSpriteOnTexture(mapTexture, sprite, x - bounds.xMin, y - bounds.yMin);
                    }
                }
            }
        }

        mapTexture.Apply();

        // 4. PNG olarak kaydet
        byte[] bytes = mapTexture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);

        Debug.Log($"Harita baþarýyla kaydedildi: {path}");

        // Hafýzayý temizle
        DestroyImmediate(mapTexture);
    }

    private void DrawSpriteOnTexture(Texture2D mainTex, Sprite sprite, int gridX, int gridY)
    {
        Texture2D sourceTex = sprite.texture;
        Rect r = sprite.textureRect;

        // Sprite'ýn piksellerini al
        Color[] pixels = sourceTex.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);

        // Ana resimdeki koordinatlarý hesapla
        int startX = gridX * pixelsPerUnit;
        int startY = gridY * pixelsPerUnit;

        // Pikselleri yerleþtir
        mainTex.SetPixels(startX, startY, (int)r.width, (int)r.height, pixels);
    }
}