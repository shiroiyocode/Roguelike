using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomTemplate",
                 menuName = "ScriptableObjects/RoomTemplate")]
public class RoomTemplateSO : ScriptableObject
{
    [Header("Room Identity")]
    public string roomName;
    public RoomType roomType;
    public int spawnWeight = 10;

    [Header("Room Dimensions")]
    public int width;
    public int height;

    [Header("Tile Layout")]
    [TextArea(10, 20)]
    public string tileLayout;

    [Header("Spawn Rules")]
    public MobSpawnRule[] mobSpawns;
    public int minMobs = 1;
    public int maxMobs = 3;

    [Header("Connections")]
    public Vector2Int[] doorPositions;
}

public enum RoomType
{
    Standard,
    Entrance,
    Exit,
    Shop,
    Special,
    Boss
}

[System.Serializable]
public class MobSpawnRule
{
    public GameObject mobPrefab;
    public int spawnWeight = 10;
    public int minCount = 1;
    public int maxCount = 2;
}