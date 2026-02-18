using BattleLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DungeonBorderAnchor;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonBlock defaultStartingRoom;
    public DungeonPalette defaultPalette;

    public float blockWidth = 30;
    public float blockHeight = 15;

    public int depth = 10;

    public Dictionary<DungeonBlock, int> blockAges;
    public Dictionary<Vector3Int, DungeonBlock> blockPositions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blockAges = new Dictionary<DungeonBlock, int>();
        blockPositions = new Dictionary<Vector3Int, DungeonBlock>();
        Generate(defaultStartingRoom, defaultPalette);
    }

    void Generate(DungeonBlock startingRoomPF, DungeonPalette palette) 
    {
        List<DungeonBlock> closedRooms = new List<DungeonBlock>();
        List<DungeonBlock> openRooms = new List<DungeonBlock>();

        BattleRandom random = new BattleRandom();
        DungeonBlock startingRoom = Instantiate(startingRoomPF, transform);
        blockPositions[GetGridPosition(startingRoom)] = startingRoom;
        blockAges[startingRoom] = 0;
        openRooms.Add(startingRoom);
        while (openRooms.Any(r => blockAges[r] < depth))
        {
            DungeonBlock room = openRooms.First(r => blockAges[r] < depth);
            foreach (var anchor in room.anchors)
            {   
                if (!room.HasDoor(anchor.direction)) continue;
                if (blockPositions.ContainsKey(GetNextGridPosition(room, anchor.direction))) continue;
                DungeonBlock block = Instantiate(palette.GetNextBlock(random, out random, this, room, FilterBlocks(GetNextGridPosition(room, anchor.direction), palette).ToList()), transform);
                //DungeonBlock block = Instantiate(startingRoomPF, transform);
                block.ConnectAnchors(block.anchors.First(a => a.direction == anchor.oppositeDirection), anchor);
                blockPositions[GetGridPosition(block)] = block;
                blockAges[block] = blockAges[room] + 1;
                ConnectToNeighbors(block);
                openRooms.Add(block);
            }
            openRooms.Remove(room);
            closedRooms.Add(room);
        }
    }

    Vector3Int GetGridPosition(DungeonBlock block) 
    {
        Vector3 diff = block.transform.position - transform.position;
        return new Vector3Int(Mathf.RoundToInt(diff.x/blockWidth), Mathf.RoundToInt(diff.y/blockHeight), Mathf.RoundToInt(diff.z/blockWidth));
    }
    Vector3Int GetNextGridPosition(DungeonBlock block, AnchorDirection direction)
    {
        return GetNextGridPosition(GetGridPosition(block), direction);
    }
    Vector3Int GetNextGridPosition(Vector3Int blockPosition, AnchorDirection direction)
    {
        Vector3Int offset;
        switch (direction)
        {
            case AnchorDirection.North: offset = Vector3Int.forward; break;
            case AnchorDirection.South: offset = Vector3Int.back; break;
            case AnchorDirection.East: offset = Vector3Int.right; break;
            case AnchorDirection.West: offset = Vector3Int.left; break;
            default: throw new System.ArgumentException("You put none in GetNextGridPosition's direction! D:");
        }
        return blockPosition + offset;
    }
    void ConnectToNeighbors(DungeonBlock block) 
    {
        foreach(DungeonBorderAnchor anchor in block.anchors) 
        {
            Vector3Int nextPos = GetNextGridPosition(block, anchor.direction);
            if (anchor.connection == null && blockPositions.ContainsKey(nextPos)) 
            {
                anchor.Connect(blockPositions[nextPos].anchors.First(a => a.oppositeDirection == anchor.direction));
            }
        }
    }
    IEnumerable<DungeonPalette.BlockMeta> FilterBlocks(Vector3Int position, DungeonPalette palette) 
    {
        IEnumerable<DungeonPalette.BlockMeta> metas = palette.blockMetas;
        IEnumerable<AnchorDirection> directions = new List<AnchorDirection>{AnchorDirection.North, AnchorDirection.South, AnchorDirection.East, AnchorDirection.West};
        foreach (AnchorDirection direction in directions) 
        {
            Vector3Int key = GetNextGridPosition(position, direction);
            if (blockPositions.ContainsKey(key)) 
            {
                    metas = metas.Where(m => m.block.anchors.First(a => a.direction == direction).anchorObject.type == blockPositions[key].anchors.First(a => a.oppositeDirection == direction).anchorObject.type);
            }
        }
        return metas;
    }
}
