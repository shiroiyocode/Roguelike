using UnityEngine;
using System.Collections.Generic;

public class RoomData
{
    public RoomTemplateSO template;
    public Vector2Int position;
    public int width;
    public int height;
    public RoomType roomType;
    public List<Vector2Int> doorPositions =
        new List<Vector2Int>();
    public bool isConnected = false;

    public RoomData(RoomTemplateSO template,
                    Vector2Int position)
    {
        this.template = template;
        this.position = position;
        this.width = template.width;
        this.height = template.height;
        this.roomType = template.roomType;
    }

    // Returns the center of this room in world space
    public Vector2 GetCenter()
    {
        return new Vector2(
            position.x + width * 0.5f,
            position.y + height * 0.5f
        );
    }

    // Returns world position of a door
    public Vector2Int GetWorldDoorPosition(Vector2Int localDoor)
    {
        return new Vector2Int(
            position.x + localDoor.x,
            position.y + localDoor.y
        );
    }
}