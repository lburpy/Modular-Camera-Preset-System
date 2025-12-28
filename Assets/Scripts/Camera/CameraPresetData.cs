using UnityEngine;

[System.Serializable]
public class CameraPresetData
{
    [Header("Transform")]
    public Vector3 position; // Kameranýn gideceði pozisyon
    public Vector3 rotation; // Euler rotation

    [Header("Komþular (-1 = Yok demek)")]
    public int up = -1;     // Yukarý geçiþ
    public int down = -1;   // Aþaðý  geçiþ
    public int left = -1;   // Sola geçiþ
    public int right = -1;  // Saða geçiþ
}