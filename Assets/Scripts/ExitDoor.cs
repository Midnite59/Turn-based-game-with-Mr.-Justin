using UnityEngine;

public class ExitDoor : MonoBehaviour, IInteractable
{
    bool locked { get { return GetComponentInParent<DungeonBorderAnchorObject>().isWall; } }
    bool hasOpened = false;
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetRotation(DungeonBorderAnchor.AnchorDirection direction)
    {
        switch (direction)
        {
            case DungeonBorderAnchor.AnchorDirection.North: transform.rotation = Quaternion.identity; break;
            case DungeonBorderAnchor.AnchorDirection.South: transform.rotation = Quaternion.Euler(0, 180, 0); break;
            case DungeonBorderAnchor.AnchorDirection.East: transform.rotation = Quaternion.Euler(0, 90, 0); break;
            case DungeonBorderAnchor.AnchorDirection.West: transform.rotation = Quaternion.Euler(0, -90, 0); break;
            default: break;
        }
    }

    public void Open()
    {
        DungeonGenerator gen = GetComponentInParent<DungeonGenerator>();
        if (gen != null)
        {
            gen.GenerateFrom(this);
            hasOpened = true;
        }
        else 
        {
            Debug.LogWarning("Yo I can't find the generator");
        }
    }

    public bool Interact()
    {
        if (IsActive())
        {
            animator.SetTrigger("Open");
            Open();
            return true;
        }
        return false;
    }

    public bool IsActive()
    {
        return !(locked || hasOpened);
        //throw new System.NotImplementedException("idk");
    }
}
