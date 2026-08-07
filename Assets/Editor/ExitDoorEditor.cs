using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonPalette))]
public class DungeonPaletteEditor : Editor
{
    protected DungeonPalette palette;
    private void OnEnable()
    {
        palette = (DungeonPalette)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Bake Blocks"))
        {
            palette.BakeBlocks();
            AssetDatabase.ForceReserializeAssets();
        }
    }
}
