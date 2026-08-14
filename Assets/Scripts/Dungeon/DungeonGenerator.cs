using BattleLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DungeonBorderAnchor;
using static DungeonBorderAnchorObject;

public class DungeonGenerator : MonoBehaviour
{
    public DungeonBlock defaultStartingRoom;
    public DungeonPalette defaultPalette;

    public float blockWidth = 30;
    public float blockHeight = 15;

    public int depth = 10;

    public int exitsAtEnd = 3;

    public int extraExits = 3;

    public int segment = 0; // 0 base

    public List<Vector3Int> exitsStopped;
    public List<DungeonBorderAnchor> exitAnchors;

    public Dictionary<DungeonBlock, int> blockAges;
    public Dictionary<Vector3Int, DungeonBlock> blockPositions;

    private IEnumerable<AnchorDirection> directions = new List<AnchorDirection>() { AnchorDirection.North, AnchorDirection.South, AnchorDirection.East, AnchorDirection.West };
    private IEnumerable<AnchorDirection> directions6 = new List<AnchorDirection>() { AnchorDirection.North, AnchorDirection.South, AnchorDirection.East, AnchorDirection.West, AnchorDirection.Up, AnchorDirection.Down };

    public ExitDoor exitPrefab;

    public DungeonBorderAnchorObject NSDoor;
    public DungeonBorderAnchorObject EWDoor;

    public DungeonBlockAnchor prefabAnchor;

    public EncounterTable encounters;

    public EncountersOhNo encounterPrefab;

    public BattleRandom random = new BattleRandom();

    [Serializable]
    public class blockObjectPair 
    {
        public string key;
        public DungeonBlockAnchor.PointFlags flags;
        public Vector3 pos;
    }

    public List<blockObjectPair> blockObjects;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log((-100)%(3));

        blockAges = new Dictionary<DungeonBlock, int>();
        blockPositions = new Dictionary<Vector3Int, DungeonBlock>();
        StartGenerate(defaultStartingRoom, defaultPalette);
        Debug.Break();
    }

    public void GenerateFrom(ExitDoor exitDoor, DungeonPalette palette = null) 
    {
        segment++;
        palette = palette != null ? palette : defaultPalette;
        blockAges.Clear();
        //nop
        foreach (DungeonBorderAnchor anchor in exitAnchors) 
        {
            if (anchor.anchorObject.GetComponentInChildren<ExitDoor>() != exitDoor) 
            {
                anchor.anchorObject.Lock();
            }

        }
        DungeonBlock startingRoom = exitDoor.GetComponentInParent<DungeonBlock>();
        StartCoroutine(Generate(startingRoom, palette));
    }
    //TOD Take off the top of the generate function and put the prefab part into its own function

    void StartGenerate(DungeonBlock startingRoomPF, DungeonPalette palette)
    {
        DungeonBlock startingRoom = Instantiate(startingRoomPF, transform);
        blockPositions[GetGridPosition(startingRoom)] = startingRoom;
        //blockAges[startingRoom] = 0;
        StartCoroutine(Generate(startingRoom, palette));
    }

    IEnumerator Generate(DungeonBlock startingRoom, DungeonPalette palette)
    {
        List<DungeonBlock> closedRooms = new List<DungeonBlock>();
        List<DungeonBlock> openRooms = new List<DungeonBlock>();

        exitAnchors = new List<DungeonBorderAnchor>();

        exitsStopped = new List<Vector3Int>();
        List<Vector3Int> exitsWorking = new List<Vector3Int>();

        //BattleRandom random = new BattleRandom();

        blockAges[startingRoom] = 0;
        openRooms.Add(startingRoom);
        while (openRooms.Any(r => blockAges[r] < depth))
        {
            DungeonBlock room = openRooms.First(r => blockAges[r] < depth);
            foreach (var anchor in room.anchors)
            {
                if (!room.HasDoor(anchor.direction)) continue;
                Vector3Int nextRoomPos = GetNextGridPosition(room, anchor.direction);
                if (blockPositions.ContainsKey(nextRoomPos)) continue;
                exitsWorking = blockAges.Aggregate(new List<Vector3Int>(), (set, ba) => { return ba.Value < depth ? set.Concat(GetFreeAdjacentGridPositions(ba.Key, GetGridPosition(ba.Key)).Where(gp => !set.Contains(gp))).ToList() : set; });
                exitsStopped = blockAges.Aggregate(exitsStopped, (set, ba) => { return ba.Value >= depth ? set.Concat(GetFreeAdjacentGridPositions(ba.Key, GetGridPosition(ba.Key)).Where(gp => !set.Contains(gp))).ToList() : set; });
                int minExits = 0;
                int maxExits = 0;

                int ageOffset = 1;

                //int MaxMinExits = ((exitsWorking.Count - 1) * 3);
                int MaxMinExits = exitsWorking.Aggregate(new List<Vector3Int>(), (set, pos) =>
                {
                    if (pos == nextRoomPos)
                    {
                        return set;
                    }
                    return set.Concat(GetFreeAdjacentGridPositions(pos)).ToList();
                }, set => set.Count);


                if (blockAges[room] + 1 >= depth)
                {
                    maxExits = Math.Max(0, exitsAtEnd - exitsStopped.Count);
                    minExits = Math.Max(0, maxExits - MaxMinExits);

                }
                else
                {
                    int exitsRemaining = exitsAtEnd - exitsWorking.Count;
                    minExits = exitsRemaining < 0 ? 0 : 1;
                    maxExits = exitsRemaining < 0 ? 1 : Math.Max(exitsRemaining + extraExits, 1);

                }
                Debug.LogWarning("exitsWorking: [" + String.Join(',', exitsWorking) + "], exitsStopped: [" + String.Join(',', exitsStopped) + "], exitsRemaining: " + (exitsAtEnd - exitsWorking.Count) + ", minExits: " + minExits + ", maxExits: " + maxExits);
                List<DungeonPalette.BlockMeta> filteredBlocks;
                try
                {
                    filteredBlocks = FilterBlocks(GetNextGridPosition(room, anchor.direction), palette, minExits, maxExits).ToList();
                }
                catch (InvalidOperationException)
                {

                    //Debug.Log("Hi");
                    ageOffset = 0;
                    filteredBlocks = FilterBlocks(GetNextGridPosition(room, anchor.direction), palette, GetFreeAdjacentGridPositions(nextRoomPos).Count(), maxExits).ToList();
                }
                DungeonBlock block = Instantiate(palette.GetNextBlock(random, out random, this, room, filteredBlocks), transform);
                List<DungeonBlock.Neighbor> neighbors = new List<DungeonBlock.Neighbor>(block.neighbors);
                block.ConnectAnchors(block.anchors.First(a => a.direction == anchor.oppositeDirection), anchor);
                while (neighbors.Count > 0)
                {
                    DungeonBlock neighborBlock = Instantiate(neighbors[0].prefab, transform);
                    //Debug.Log(neighborBlock.name);
                    DungeonBorderAnchor neighborAnchor = block.anchors.First(a => a.direction == neighbors[0].direction);
                    neighborBlock.ConnectAnchors(neighborBlock.anchors.First(a => a.direction == neighborAnchor.oppositeDirection), neighborAnchor);
                    neighbors.RemoveAt(0);
                    blockPositions[GetGridPosition(neighborBlock)] = neighborBlock;
                    blockAges[neighborBlock] = blockAges[room] + ageOffset;
                    neighborBlock.name += " " + blockAges[neighborBlock];
                    neighborBlock.name += " " + GetGridPosition(neighborBlock);
                    ConnectToNeighbors(neighborBlock);
                    openRooms.Add(neighborBlock);
                }
                blockPositions[GetGridPosition(block)] = block;
                blockAges[block] = blockAges[room] + ageOffset;
                block.name += " " + blockAges[block];
                block.name += " " + GetGridPosition(block);
                ConnectToNeighbors(block);
                openRooms.Add(block);
                yield return null;
            }
            openRooms.Remove(room);
            closedRooms.Add(room);
        }
        exitAnchors = openRooms.Aggregate(new List<DungeonBorderAnchor>(), (l, r) => { return l.Concat(r.anchors.Where(a => directions.Contains(a.direction) && a.connection == null && !a.anchorObject.isWall)).ToList(); });
        BuildExits(exitAnchors);
        Debug.Log(String.Join(", ", exitAnchors));
        yield return StartCoroutine(FillSegment());
    }

    IEnumerator FillSegment() 
    {
        List<DungeonBlock> segment = GetCurrentSegment().ToList();
        foreach (DungeonBlock block in segment) 
        {
            DungeonBlockAnchor point = Instantiate(prefabAnchor, block.transform);
            string key = block.GetKey();
            Debug.LogWarning("Key = " + key);
            if (key != "0")
            {
                blockObjectPair bop = blockObjects.First(bo => bo.key == key);
                point.transform.localPosition = bop.pos;
                point.accTypes = bop.flags;
                block.points.Add(point);
            }
            foreach (DungeonBlockAnchor anchor in block.points) 
            {
                anchor.anchorObject = Instantiate(encounterPrefab, anchor.transform, false);
                (anchor.anchorObject as EncountersOhNo).Init(encounters.GetNextEncounter(random, out random, this));
                yield return null;
            }
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

    IEnumerable<Vector3Int> GetAdjacentGridPositions(DungeonBlock block, Vector3Int position)
    {
        var open = block.neighbors.Select(n => new { neighbor = n, position }).ToList();
        var closed = new[] { new { neighbor = new DungeonBlock.Neighbor(block, AnchorDirection.None), position } }.ToList();
        //var closed = new List<new() {DungeonBlock, Vector3Int}>
        //List<DungeonBlock.Neighbor> closed = new() { new DungeonBlock.Neighbor(block, AnchorDirection.None) };

        List<Vector3Int> gridPositions = new List<Vector3Int>() { position };
        List<Vector3Int> nextGridPositions = new List<Vector3Int>() { };
        while (open.Count() > 0)
        {
            var nPos = open[0];
            Vector3Int gPos = GetNextGridPosition(nPos.neighbor.direction, nPos.position);
            open = open.Concat(nPos.neighbor.prefab.neighbors.Where(n => !closed.Any(c => c.neighbor.prefab == n.prefab)).Select(n => new { neighbor = n, position = gPos })).ToList();
            open.RemoveAt(0);
            if (gridPositions.Contains(gPos))
            {
                //Debug.LogWarning("Oh no!");
                continue;
            }
            closed.Add(new { nPos.neighbor, position = gPos });
            gridPositions.Add(gPos);
        }
        foreach (var neighborPos in closed)
        {
            //Debug.Log(neighborPos.neighbor.prefab.name);
            foreach (AnchorDirection direction in directions6)
            {
                if (neighborPos.neighbor.prefab.HasDoor(direction))
                {
                    Vector3Int nextGridPosition = GetNextGridPosition(direction, neighborPos.position);
                    if (!gridPositions.Contains(nextGridPosition))
                    {
                        nextGridPositions.Add(nextGridPosition);
                    }
                }
            }
        }
        //Debug.LogWarning("GRIDPOSITIONS: " + nextGridPositions.ToList().Count + ", " + string.Join(", ", nextGridPositions));
        return nextGridPositions;
    }
    IEnumerable<Vector3Int> GetFreeAdjacentGridPositions(Vector3Int position) 
    {
        List<Vector3Int> adjGP = new List<Vector3Int>();
        foreach (AnchorDirection direction in directions) 
        {
            adjGP.Add(GetNextGridPosition(direction, position));
        }
        return adjGP.WhereFree(this);
    }

    IEnumerable<Vector3Int> GetFreeAdjacentGridPositions(DungeonBlock block, Vector3Int position) 
    {
        return GetAdjacentGridPositions(block, position).WhereFree(this);
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
            default: throw new System.ArgumentException("You put none in GetNextGridPosition's direction! D: (asked for " + (int)direction + "??)");
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

    public IEnumerable<DungeonBlock> GetCurrentSegment() 
    {
        if (segment == 0)
        {
            return blockAges.Select(a => a.Key);
        }
        else 
        {
            return blockAges.Where(a => a.Value != 0).Select(a => a.Key);
        }
    }

    IEnumerable<DungeonPalette.BlockMeta> FilterBlocks(Vector3Int position, DungeonPalette palette, int minExits, int maxExits) 
    {
        IEnumerable<DungeonPalette.BlockMeta> metas = palette.blockMetas.Where(m => { var fe = GetFreeExits(m.block, position); return fe >= minExits && fe <= maxExits; }).ToList();
        if (metas.Count() == 0)
        {
            throw new InvalidOperationException("We can't meet the exit requirement!!");
        }
        IEnumerable<DungeonPalette.BlockMeta> newMetas = GetValidBlocks(metas, position);
        if (newMetas.Count() == 0) 
        {
            return GetValidBlocks(metas, position, true);
        }
        return newMetas;
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

    int GetFreeExits(DungeonBlock block, Vector3Int position) 
    {
        int freeExits = GetAdjacentGridPositions(block, position).WhereFree(this).Count();
        //Debug.LogError(freeExits + ", " + block.name);
        return freeExits;
    }
    /*
    void DrawLocations() 
    {
        foreach (DungeonBlock block in blockPositions.Values) 
        {
            Vector3Int gridPos = GetGridPosition(block);
            Color col = new Color((gridPos.x * .1f) + 0.5f, (gridPos.y * .25f) + 0.5f, (gridPos.z * .1f) + 0.5f);
            DrawLocation(col, GetGridPosition(block));
        }
    }
    */
    void DrawLocations()
    {
        foreach (DungeonBlock block in blockPositions.Values)
        {
            Vector3Int gridPos = GetGridPosition(block);
            Color col = blockAges.ContainsKey(block) ? new Color(0, 0.5f + (blockAges[block] * 0.1f), 0) : Color.gray;
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

    void BuildExits(IEnumerable<DungeonBorderAnchor> anchors) 
    {
        foreach (DungeonBorderAnchor anchor in anchors) 
        {
            BuildExit(anchor);
        }
    }

    void BuildExit(DungeonBorderAnchor anchor) 
    {
        // idk exit goes here
        switch (anchor.direction) 
        {
            case AnchorDirection.North:
            case AnchorDirection.South:
                anchor.BuildObject(NSDoor);
                break;
            case AnchorDirection.East:
            case AnchorDirection.West:
                anchor.BuildObject(EWDoor);
                break;
            default:
                throw new NotImplementedException("Why are we building an exit on " + anchor.direction + "???");
        }
        ExitDoor exit = Instantiate(exitPrefab, anchor.anchorObject.transform);
        exit.transform.localPosition = Vector3.zero;
        exit.SetRotation(anchor.direction);
    }

}
public static class DungeonGeneratorHelper
{
    public static IEnumerable<Vector3Int> WhereFree(this IEnumerable<Vector3Int> list, DungeonGenerator gen)
    {
        return list.Where(gp => !gen.blockPositions.ContainsKey(gp) && !gen.exitsStopped.Contains(gp));
    }
}
