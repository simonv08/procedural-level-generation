using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Size")]
    public int dungeonWidth = 50;
    public int dungeonHeight = 50;

    [Header("Room Size")]
    public int minRoomSize = 6;
    public int maxRoomSize = 15;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;

    private List<RectInt> rooms;
    private List<Vector2Int> corridorTiles;

    private int[,] dungeonMap; // 0 = empty, 1 = floor, 2 = wall

    private void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        dungeonMap = new int[dungeonWidth, dungeonHeight];
        rooms = new List<RectInt>();
        corridorTiles = new List<Vector2Int>();

        BSPPartition(new RectInt(0, 0, dungeonWidth, dungeonHeight));
        ConnectRooms();
        BuildWalls();
        VisualizeDungeon();
    }

    void BSPPartition(RectInt space)
    {
        if (space.width <= maxRoomSize && space.height <= maxRoomSize && Random.value > 0.5f)
        {
            int roomWidth = Random.Range(minRoomSize, space.width);
            int roomHeight = Random.Range(minRoomSize, space.height);
            int roomX = Random.Range(space.xMin, space.xMax - roomWidth);
            int roomY = Random.Range(space.yMin, space.yMax - roomHeight);
            RectInt newRoom = new RectInt(roomX, roomY, roomWidth, roomHeight);
            rooms.Add(newRoom);
            
            for (int x = newRoom.xMin; x < newRoom.xMax; x++)
                for (int y = newRoom.yMin; y < newRoom.yMax; y++)
                    dungeonMap[x, y] = 1;
            return;
        }

        bool splitHorizontally = Random.value > 0.5f;

        if (splitHorizontally && space.height >= 2 * minRoomSize)
        {
            int split = Random.Range(minRoomSize, space.height - minRoomSize);
            BSPPartition(new RectInt(space.xMin, space.yMin, space.width, split));
            BSPPartition(new RectInt(space.xMin, space.yMin + split, space.width, space.height - split));
        }
        else if (!splitHorizontally && space.width >= 2 * minRoomSize)
        {
            int split = Random.Range(minRoomSize, space.width - minRoomSize);
            BSPPartition(new RectInt(space.xMin, space.yMin, split, space.height));
            BSPPartition(new RectInt(space.xMin + split, space.yMin, space.width - split, space.height));
        }
        else
        {
            int roomWidth = Mathf.Min(space.width, maxRoomSize);
            int roomHeight = Mathf.Min(space.height, maxRoomSize);
            RectInt newRoom = new RectInt(space.xMin, space.yMin, roomWidth, roomHeight);
            rooms.Add(newRoom);

            for (int x = newRoom.xMin; x < newRoom.xMax; x++)
                for (int y = newRoom.yMin; y < newRoom.yMax; y++)
                    dungeonMap[x, y] = 1;
        }
    }

    void ConnectRooms()
    {
        if (rooms.Count <= 1)
            return;

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int roomCenterA = GetRoomCenter(rooms[i]);
            Vector2Int roomCenterB = GetRoomCenter(rooms[i + 1]);
            CreateCorridor(roomCenterA, roomCenterB);
        }
    }

    Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(room.xMin + room.width / 2, room.yMin + room.height / 2);
    }

    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;

        while (current.x != end.x)
        {
            dungeonMap[current.x, current.y] = 1;
            corridorTiles.Add(current);
            current.x += (end.x > current.x) ? 1 : -1;
        }

        while (current.y != end.y)
        {
            dungeonMap[current.x, current.y] = 1;
            corridorTiles.Add(current);
            current.y += (end.y > current.y) ? 1 : -1;
        }
    }

    void BuildWalls()
    {
        for (int x = 1; x < dungeonWidth - 1; x++)
        {
            for (int y = 1; y < dungeonHeight - 1; y++)
            {
                if (dungeonMap[x, y] == 1)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dungeonMap[x + dx, y + dy] == 0)
                            {
                                dungeonMap[x + dx, y + dy] = 2; // Wall
                            }
                        }
                    }
                }
            }
        }
    }

    void VisualizeDungeon()
    {
        float tileSize = 1f;

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);

                if (dungeonMap[x, y] == 1)
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                }
                else if (dungeonMap[x, y] == 2)
                {
                    Instantiate(wallPrefab, pos + Vector3.up * 0.5f, Quaternion.identity, transform);
                }
            }
        }
    }
}
