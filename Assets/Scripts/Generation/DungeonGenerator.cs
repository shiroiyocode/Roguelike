using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance { get; private set; }

    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap objectTilemap;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Room Templates")]
    public RoomTemplateSO[] standardRooms;
    public RoomTemplateSO entranceRoom;
    public RoomTemplateSO exitRoom;

    [Header("Generation Settings")]
    public int minRooms = 5;
    public int maxRooms = 8;
    public int roomSpacing = 2;

    [Header("Player")]
    public GameObject playerPrefab;

    private List<RoomData> generatedRooms =
        new List<RoomData>();
    private GameObject currentPlayer;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        GenerateFloor();
    }

    public void GenerateFloor()
    {
        // Clear existing floor
        ClearFloor();

        generatedRooms.Clear();

        // Place entrance room first
        PlaceEntranceRoom();

        // Place standard rooms
        int roomCount = Random.Range(minRooms, maxRooms + 1);
        for (int i = 0; i < roomCount; i++)
            TryPlaceRoom();

        // Place exit room
        PlaceExitRoom();

        // Connect all rooms with corridors
        ConnectRooms();

        // Paint walls around all floor tiles
        PaintWalls();

        // Spawn player at entrance
        SpawnPlayer();

        // Populate rooms with mobs
        PopulateRooms();

        Debug.Log("Floor generated with " +
                  generatedRooms.Count + " rooms.");
    }

    void ClearFloor()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        objectTilemap.ClearAllTiles();

        // Destroy all existing mobs
        GameObject[] mobs =
            GameObject.FindGameObjectsWithTag("Mob");
        foreach (GameObject mob in mobs)
            Destroy(mob);
    }

    void PlaceEntranceRoom()
    {
        RoomData room = new RoomData(
            entranceRoom,
            Vector2Int.zero
        );
        generatedRooms.Add(room);
        PaintRoom(room);
    }

    void PlaceExitRoom()
    {
        // Place exit room furthest from entrance
        RoomData lastRoom = generatedRooms[
            generatedRooms.Count - 1
        ];

        Vector2Int exitPos = new Vector2Int(
            lastRoom.position.x + lastRoom.width + roomSpacing,
            lastRoom.position.y
        );

        RoomData exitRoom = new RoomData(
            this.exitRoom,
            exitPos
        );

        generatedRooms.Add(exitRoom);
        PaintRoom(exitRoom);
    }

    void TryPlaceRoom()
    {
        if (standardRooms.Length == 0) return;

        // Pick a random template weighted by spawnWeight
        RoomTemplateSO template = GetWeightedRoom();

        // Try to place next to an existing room
        RoomData anchorRoom = generatedRooms[
            Random.Range(0, generatedRooms.Count)
        ];

        Vector2Int newPos = GetAdjacentPosition(anchorRoom);

        // Check if position overlaps existing rooms
        if (IsPositionOverlapping(newPos,
            template.width, template.height))
            return;

        RoomData newRoom = new RoomData(template, newPos);
        generatedRooms.Add(newRoom);
        PaintRoom(newRoom);
    }

    RoomTemplateSO GetWeightedRoom()
    {
        int totalWeight = 0;
        foreach (RoomTemplateSO room in standardRooms)
            totalWeight += room.spawnWeight;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (RoomTemplateSO room in standardRooms)
        {
            cumulative += room.spawnWeight;
            if (roll < cumulative)
                return room;
        }

        return standardRooms[0];
    }

    Vector2Int GetAdjacentPosition(RoomData anchor)
    {
        // Pick a random side to place the new room
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // Right
                return new Vector2Int(
                    anchor.position.x + anchor.width + roomSpacing,
                    anchor.position.y
                );
            case 1: // Left
                return new Vector2Int(
                    anchor.position.x - 10 - roomSpacing,
                    anchor.position.y
                );
            case 2: // Up
                return new Vector2Int(
                    anchor.position.x,
                    anchor.position.y + anchor.height + roomSpacing
                );
            default: // Down
                return new Vector2Int(
                    anchor.position.x,
                    anchor.position.y - 10 - roomSpacing
                );
        }
    }

    bool IsPositionOverlapping(Vector2Int pos,
                                int width, int height)
    {
        foreach (RoomData existing in generatedRooms)
        {
            bool overlapX = pos.x < existing.position.x +
                            existing.width + roomSpacing &&
                            pos.x + width + roomSpacing >
                            existing.position.x;

            bool overlapY = pos.y < existing.position.y +
                            existing.height + roomSpacing &&
                            pos.y + height + roomSpacing >
                            existing.position.y;

            if (overlapX && overlapY)
                return true;
        }
        return false;
    }

    void PaintRoom(RoomData room)
    {
        // Paint floor tiles for the room interior
        for (int x = 1; x < room.width - 1; x++)
        {
            for (int y = 1; y < room.height - 1; y++)
            {
                Vector3Int tilePos = new Vector3Int(
                    room.position.x + x,
                    room.position.y + y,
                    0
                );
                floorTilemap.SetTile(tilePos, floorTile);
            }
        }
    }

    void ConnectRooms()
    {
        // Connect each room to the next one in the list
        for (int i = 0; i < generatedRooms.Count - 1; i++)
        {
            RoomData roomA = generatedRooms[i];
            RoomData roomB = generatedRooms[i + 1];
            CarveCorridorBetween(roomA, roomB);
        }
    }

    void CarveCorridorBetween(RoomData a, RoomData b)
    {
        // Get center points of both rooms
        Vector2Int centerA = new Vector2Int(
            a.position.x + a.width / 2,
            a.position.y + a.height / 2
        );

        Vector2Int centerB = new Vector2Int(
            b.position.x + b.width / 2,
            b.position.y + b.height / 2
        );

        // Carve L-shaped corridor
        // First go horizontal then vertical
        Vector2Int current = centerA;

        // Horizontal segment
        while (current.x != centerB.x)
        {
            floorTilemap.SetTile(
                new Vector3Int(current.x, current.y, 0),
                floorTile
            );
            current.x += (centerB.x > current.x) ? 1 : -1;
        }

        // Vertical segment
        while (current.y != centerB.y)
        {
            floorTilemap.SetTile(
                new Vector3Int(current.x, current.y, 0),
                floorTile
            );
            current.y += (centerB.y > current.y) ? 1 : -1;
        }

        // Paint final tile
        floorTilemap.SetTile(
            new Vector3Int(current.x, current.y, 0),
            floorTile
        );
    }

    void PaintWalls()
    {
        // For every floor tile check all 8 neighbors
        // If a neighbor has no floor tile paint a wall there
        BoundsInt bounds = floorTilemap.cellBounds;

        for (int x = bounds.xMin - 1; x <= bounds.xMax + 1; x++)
        {
            for (int y = bounds.yMin - 1;
                 y <= bounds.yMax + 1; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Skip if already a floor tile
                if (floorTilemap.HasTile(pos)) continue;

                // Check if any neighbor is a floor tile
                if (HasFloorNeighbor(x, y))
                    wallTilemap.SetTile(pos, wallTile);
            }
        }
    }

    bool HasFloorNeighbor(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector3Int neighbor = new Vector3Int(
                    x + dx, y + dy, 0
                );

                if (floorTilemap.HasTile(neighbor))
                    return true;
            }
        }
        return false;
    }

    void SpawnPlayer()
    {
        if (generatedRooms.Count == 0) return;

        // Spawn at entrance room center
        RoomData entrance = generatedRooms[0];
        Vector2 spawnPos = entrance.GetCenter();

        // Snap to grid
        spawnPos = new Vector2(
            Mathf.Floor(spawnPos.x) + 0.5f,
            Mathf.Floor(spawnPos.y) + 0.5f
        );

        if (currentPlayer == null)
        {
            currentPlayer = Instantiate(
                playerPrefab,
                new Vector3(spawnPos.x, spawnPos.y, 0),
                Quaternion.identity
            );
        }
        else
        {
            currentPlayer.transform.position = new Vector3(
                spawnPos.x, spawnPos.y, 0
            );
        }
    }

    void PopulateRooms()
    {
        // Skip entrance room at index 0
        // Skip exit room at last index
        for (int i = 1; i < generatedRooms.Count - 1; i++)
        {
            RoomData room = generatedRooms[i];
            PopulateRoom(room);
        }
    }

    void PopulateRoom(RoomData room)
    {
        if (room.template.mobSpawns == null ||
            room.template.mobSpawns.Length == 0)
            return;

        int mobCount = Random.Range(
            room.template.minMobs,
            room.template.maxMobs + 1
        );

        for (int i = 0; i < mobCount; i++)
        {
            // Pick a random mob spawn rule
            MobSpawnRule rule = room.template.mobSpawns[
                Random.Range(0, room.template.mobSpawns.Length)
            ];

            // Find a random walkable position in the room
            Vector2? spawnPos = GetRandomRoomPosition(room);

            if (spawnPos == null) continue;

            GameObject mob = Instantiate(
                rule.mobPrefab,
                new Vector3(
                    spawnPos.Value.x,
                    spawnPos.Value.y,
                    0
                ),
                Quaternion.identity
            );

            mob.tag = "Mob";
        }
    }

    Vector2? GetRandomRoomPosition(RoomData room)
    {
        // Try up to 10 times to find a valid position
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int x = Random.Range(
                room.position.x + 1,
                room.position.x + room.width - 1
            );
            int y = Random.Range(
                room.position.y + 1,
                room.position.y + room.height - 1
            );

            Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);

            Collider2D hit = Physics2D.OverlapBox(
                pos,
                Vector2.one * 0.9f,
                0f,
                LayerMask.GetMask("Walls", "Mobs", "Player")
            );

            if (hit == null)
                return pos;
        }

        return null;
    }
}