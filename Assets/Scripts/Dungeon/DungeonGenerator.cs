using BattleLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static DungeonBorderAnchor;
using static DungeonBorderAnchorObject;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonBlock defaultStartingRoom;
    public DungeonPalette defaultPalette;

    public float blockWidth = 30;
    public float blockHeight = 15;

    public int depth = 10;

    public int exitsAtEnd = 3;

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

        int exitsStopped = 0;
        int exitsWorking = 0;

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
                exitsWorking = blockAges.Aggregate(0, (sum, ba) => { return ba.Value < depth ? sum + ba.Key.exits : sum; });
                exitsStopped = blockAges.Aggregate(0, (sum, ba) => { return ba.Value >= depth ? sum + ba.Key.exits : sum; });
                int minExits = 0;
                int maxExits = 0;

                int MaxMinExits = ((exitsWorking - 1) * 3);

                if (blockAges[room] + 1 >= depth)
                {
                    maxExits = exitsAtEnd - exitsStopped;
                    minExits = Math.Max(0, maxExits - MaxMinExits);

                }
                else 
                {
                    //TODO
                }
                    Debug.Log(GetGridPosition(room));
                DungeonBlock block = Instantiate(palette.GetNextBlock(random, out random, this, room, FilterBlocks(GetNextGridPosition(room, anchor.direction), palette).ToList()), transform);
                List<DungeonBlock.Neighbor> neighbors = new List<DungeonBlock.Neighbor>(block.neighbors);
                block.ConnectAnchors(block.anchors.First(a => a.direction == anchor.oppositeDirection), anchor);
                while (neighbors.Count > 0) 
                {
                    DungeonBlock neighborBlock = Instantiate(neighbors[0].prefab, transform);
                    Debug.Log(neighborBlock.name);
                    DungeonBorderAnchor neighborAnchor = block.anchors.First(a => a.direction == neighbors[0].direction);
                    neighborBlock.ConnectAnchors(neighborBlock.anchors.First(a => a.direction == neighborAnchor.oppositeDirection), neighborAnchor);
                    neighbors.RemoveAt(0);
                    blockPositions[GetGridPosition(neighborBlock)] = neighborBlock;
                    blockAges[neighborBlock] = blockAges[room] + 1;
                    neighborBlock.name += " " + blockAges[neighborBlock];
                    neighborBlock.name += " " + GetGridPosition(neighborBlock);
                    ConnectToNeighbors(neighborBlock);
                    openRooms.Add(neighborBlock);
                }
                blockPositions[GetGridPosition(block)] = block;
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
        var open = block.neighbors.Select(n => new { neighbor = n, position }).ToList();
        List<DungeonBlock.Neighbor> closed = new() { new DungeonBlock.Neighbor(block, AnchorDirection.None) };

        List<Vector3Int> gridPositions = new List<Vector3Int>() {position};
        while (open.Count() > 0) 
        {
            var nPos = open[0];
            Vector3Int gPos = GetNextGridPosition(nPos.neighbor.direction, nPos.position);
            open = open.Concat(nPos.neighbor.prefab.neighbors.Where(n => !closed.Any(c => c.prefab == n.prefab)).Select(n => new { neighbor = n, position = gPos})).ToList();
            open.RemoveAt(0);
            closed.Add(nPos.neighbor);
            if (gridPositions.Contains(gPos)) 
            {
                //Debug.LogWarning("Oh no!");
                continue;
            }
            gridPositions.Add(gPos);
        }

        return gridPositions;
    }

    Vector3Int GetNextGridPosition(AnchorDirection direction, Vector3Int blockPosition)
    {
        Vector3Int offset;
        switch (direction)
        {
            case AnchorDirection.North: offset = Vector3Int.forward; break;
            case AnchorDirection.South: offset = Vector3Int.back; break;
            case AnchorDirection.East: offset = Vector3Int.right; break;
            case AnchorDirection.West: offset = Vector3Int.left; break;
            case AnchorDirection.Up: offset = Vector3Int.up; break;
            case AnchorDirection.Down: offset = Vector3Int.down; break;
            default: throw new System.ArgumentException("You put none in GetNextGridPosition's direction! D:");
        }
        return blockPosition + offset;
    }
    Vector3Int GetNextGridPosition(DungeonBlock block, AnchorDirection direction) 
    {
        return GetNextGridPosition(direction, GetGridPosition(block));
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
        //Debug.LogWarning(position);
        IEnumerable<DungeonPalette.BlockMeta> metas = palette.blockMetas;
        //IEnumerable<AnchorDirection> directions = new List<AnchorDirection>{AnchorDirection.North, AnchorDirection.South, AnchorDirection.East, AnchorDirection.West};
        metas = GetValidBlocks(metas, position);
        if (metas.Count() == 0) 
        {
            metas = palette.blockMetas;
            metas = GetValidBlocks(metas, position, true);
        }
        //Debug.Log(metas);
        return metas;
    }

    IEnumerable<DungeonPalette.BlockMeta> GetValidBlocks(IEnumerable<DungeonPalette.BlockMeta> metas, Vector3Int position, bool force = false) 
    {
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
                Vector3Int key = GetNextGridPosition(direction, position);
                if (blockPositions.ContainsKey(key))
                {
                    return (force ? !m.block.GetAnchor(direction).anchorObject.isWall : false) || m.block.GetAnchor(direction).anchorObject.type == blockPositions[key].GetAnchorOpp(direction).anchorObject.type;
                }
                else
                {
                    return true;
                }
            });
        }
        return metas;
    }

    void DrawLocations() 
    {
        foreach (DungeonBlock block in blockPositions.Values) 
        {
            Vector3Int gridPos = GetGridPosition(block);
            Color col = new Color((gridPos.x * .1f) + 0.5f, (gridPos.y * .25f) + 0.5f, (gridPos.z * .1f) + 0.5f);
            DrawLocation(col, GetGridPosition(block));
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
