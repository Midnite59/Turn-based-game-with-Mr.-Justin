using UnityEngine;

public class DungeonBorderAnchorObject : AnchorObject
{
    public enum ConnectorType 
    {
        Wall, Door, None
    }
    public ConnectorType type;
    public bool isDoor { get { return type == ConnectorType.Door; } }
    public bool isOpen { get { return type == ConnectorType.None; } }
    public bool isWall { get { return type == ConnectorType.Wall; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Lock() 
    {
        if (type == ConnectorType.Wall) 
        {
            Debug.LogError("Your trying to lock... a wall... What?");
        }
        type = ConnectorType.Wall;
    }
}
