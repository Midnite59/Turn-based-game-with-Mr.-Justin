using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExitDoor))]
public class ExitDoorEditor : Editor
{
    protected ExitDoor door;
    private void OnEnable()
    {
        door = (ExitDoor)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Open"))
        {
            Debug.Log("I... Have... OPENED");
            door.Open();
        }
    }
}
