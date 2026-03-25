using BattleLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static DungeonBorderAnchor;
using static DungeonBorderAnchorObject;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonBlock defaultStartingRoom;
    public DungeonPalette defaultPalette;

    public float blockWidth = 30;
    public float blockHeight = 15;

    public int depth = 10;

    public Dictionary<DungeonBlock, int> blockAges;
    public Dictionary<Vector3Int, DungeonBlock> blockPositions;

    private IEnumerable<AnchorDirection> directions = new List<AnchorDirection>() { AnchorDirection.North, AnchorDirection.South, AnchorDirection.East, AnchorDirection.West };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blockAges = new Dictionary<DungeonBlock, int>();
        blockPositions = new Dictionary<Vector3Int, DungeonBlock>();
        StartCoroutine(Generate(defaultStartingRoom, defaultPalette));
        Debug.Break();
    }

    IEnumerator Generate(DungeonBlock startingRoomPF, DungeonPalette palette) 
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
                Debug.Log(GetGridPosition(room));
                DungeonBlock block = Instantiate(palette.GetNextBlock(random, out random, this, room, FilterBlocks(GetNextGridPosition(room, anchor.direction), palette).ToList()), transform);
                //DungeonBlock block = Instantiate(startingRoomPF, transform);
                block.ConnectAnchors(block.anchors.First(a => a.direction == anchor.oppositeDirection), anchor);
                //blockPositions[GetGridPosition(block)] = block;
                foreach (Vector3Int position in GetGridPositions(block)) 
                {
                    //Debug.LogError(position);
                    blockPositions[position] = block;
                }
                blockAges[block] = blockAges[room] + 1;
                block.name += " " + blockAges[block];
                block.name += " " + GetGridPosition(block);
                ConnectToNeighbors(block);
                openRooms.Add(block);
                yield return null;
            }
            openRooms.Remove(room);
            closedRooms.Add(room);
        }
        yield break;
    }

    Vector3Int GetGridPosition(Vector3 position)
    {
        return new Vector3Int(Mathf.RoundToInt(position.x / blockWidth), Mathf.RoundToInt(position.y / blockHeight), Mathf.RoundToInt(position.z / blockWidth));
    }

    Vector3 GetPositionFromGrid(Vector3Int gridPosition) 
    {
        return new Vector3(gridPosition.x * blockWidth, gridPosition.y * blockHeight, gridPosition.z * blockWidth) + transform.position;
    }

    Vector3Int GetGridPosition(DungeonBlock block) 
    {
        Vector3 diff = block.transform.position - transform.position;
        return GetGridPosition(diff);
    }

    IEnumerable<Vector3Int> GetGridPositions(DungeonBlock block)
    {
        return GetGridPositions(block, GetGridPosition(block));
    }
    IEnumerable<Vector3Int> GetGridPositions(DungeonBlock block, Vector3Int position)
    {
        List<Vector3Int> positions = new List<Vector3Int>() { position };
        foreach (AnchorDirection direction in directions) 
        {
            int heightOffset = Mathf.RoundToInt((block.GetAnchor(direction).transform.position.y - block.transform.position.y) / blockHeight);
            if (heightOffset != 0) 
            {
                Vector3Int newPosition = position + (Vector3Int.up * heightOffset);
                if (!positions.Contains(newPosition)) 
                {
                    positions.Add(newPosition);
                }
            }
        }
        return positions;
    }

    Vector3Int GetNextGridPosition(DungeonBlock block, AnchorDirection direction, Vector3Int blockPosition)
    {
        Vector3Int offset;
        int heightOffset = Mathf.RoundToInt((block.GetAnchor(direction).transform.position.y - block.transform.position.y) / blockHeight);
        switch (direction)
        {
            case AnchorDirection.North: offset = Vector3Int.forward; break;
            case AnchorDirection.South: offset = Vector3Int.back; break;
            case AnchorDirection.East: offset = Vector3Int.right; break;
            case AnchorDirection.West: offset = Vector3Int.left; break;
            default: throw new System.ArgumentException("You put none in GetNextGridPosition's direction! D:");
        }
        return blockPosition + offset + (Vector3Int.up * heightOffset);
    }
    Vector3Int GetNextGridPosition(DungeonBlock block, AnchorDirection direction) 
    {
        return GetNextGridPosition(block, direction, GetGridPosition(block));
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
        Debug.LogWarning(position);
        IEnumerable<DungeonPalette.BlockMeta> metas = palette.blockMetas;
        //IEnumerable<AnchorDirection> directions = new List<AnchorDirection>{AnchorDirection.North, AnchorDirection.South, AnchorDirection.East, AnchorDirection.West};
        foreach (AnchorDirection direction in directions) 
        {
                metas = metas.Where(m =>
                {
                    foreach (Vector3Int gpos in GetGridPositions(m.block, position))
                    {
                        if (blockPositions.ContainsKey(gpos))
                        {
                            return false;
                        }
                    }
                    Vector3Int key = GetNextGridPosition(m.block, direction, position);
                    if (blockPositions.ContainsKey(key))
                    {
                        return m.block.GetAnchor(direction).anchorObject.type == blockPositions[key].GetAnchorOpp(direction).anchorObject.type;
                    }
                    else 
                    {
                        return true;
                    }
                });
        }
        if (metas.Count() == 0) 
        {
            metas = palette.blockMetas;
            foreach (AnchorDirection direction in directions)
            {
                metas = metas.Where(m =>
                {
                    foreach (Vector3Int gpos in GetGridPositions(m.block, position))
                    {
                        if (blockPositions.ContainsKey(gpos))
                        {
                            return false;
                        }
                    }
                    Vector3Int key = GetNextGridPosition(m.block, direction, position);
                    if (blockPositions.ContainsKey(key))
                    {
                        return m.block.GetAnchor(direction).anchorObject.type == ConnectorType.Wall || m.block.GetAnchor(direction).anchorObject.type == blockPositions[key].GetAnchorOpp(direction).anchorObject.type;
                    }
                    else
                    {
                        return true;
                    }
                });
            }
        }
        //Debug.Log(metas);
        return metas;
    }

    void DrawLocations() 
    {
        foreach (DungeonBlock block in blockPositions.Values.ToHashSet()) 
        {
            Vector3Int gridPos = GetGridPosition(block);
            Color col = new Color((gridPos.x * .1f) + 0.5f, (gridPos.y * .25f) + 0.5f, (gridPos.z * .1f) + 0.5f);
            foreach (Vector3Int gridPoss in GetGridPositions(block)) 
            {
                DrawLocation(col, gridPoss);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) { DrawLocations(); }
    }

    void DrawLocation(Color col, Vector3Int gridPos)
    {
        float a = 0.3f;
        Gizmos.color = new Color(col.r, col.g, col.b, a);
        Gizmos.DrawCube(GetPositionFromGrid(gridPos), Vector3.one * 10);
    }
}
