using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    private MapGenerator mapGen;
    private bool showZoneHandles = true;
    
    private void OnEnable()
    {
        mapGen = (MapGenerator)target;
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Generation Tools", EditorStyles.boldLabel);
        
        // Toggle zone handles
        showZoneHandles = EditorGUILayout.Toggle("Show Zone Handles", showZoneHandles);
        
        EditorGUILayout.Space();
        
        // Populate button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Populate Zone", GUILayout.Height(40)))
        {
            Undo.RecordObject(mapGen, "Populate Zone");
            mapGen.PopulateZone();
            EditorUtility.SetDirty(mapGen);
        }
        
        // Clear button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear Zone", GUILayout.Height(30)))
        {
            Undo.RecordObject(mapGen, "Clear Zone");
            mapGen.ClearSpawnedObjects();
            EditorUtility.SetDirty(mapGen);
        }
        
        GUI.backgroundColor = Color.white;
        
        // Info
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "• Drag prefabs into the 'Prefabs to Spawn' list\n" +
            "• Use the scene view handles to resize the zone\n" +
            "• Click 'Populate Zone' to spawn objects\n" +
            "• Objects will be placed on the ground layer", 
            MessageType.Info);
    }
    
    private void OnSceneGUI()
    {
        if (mapGen == null) return;
        
        // Draw the spawn zone
        DrawSpawnZone();
        
        // Draw zone handles for resizing
        if (showZoneHandles)
        {
            DrawZoneHandles();
        }
        
        // Handle zone center movement
        DrawCenterHandle();
    }
    
    private void DrawSpawnZone()
    {
        Bounds bounds = mapGen.GetZoneBounds();
    
        // Draw wireframe only - this definitely works
        Handles.color = mapGen.wireframeColor;
        Handles.DrawWireCube(bounds.center, bounds.size);
    
        // Optional: Draw some lines to make it more visible
        Handles.color = mapGen.zoneColor;
    
        // Draw cross lines for better visibility
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;
    
        // Draw X across the bottom
        Handles.DrawLine(
            center + new Vector3(-size.x/2, -size.y/2, -size.z/2),
            center + new Vector3(size.x/2, -size.y/2, size.z/2)
        );
        Handles.DrawLine(
            center + new Vector3(size.x/2, -size.y/2, -size.z/2),
            center + new Vector3(-size.x/2, -size.y/2, size.z/2)
        );
    
        // Draw label
        Handles.color = Color.white;
        Handles.Label(bounds.center + Vector3.up * (bounds.size.y * 0.5f + 1f), 
            $"Spawn Zone\n{mapGen.objectCount} objects");
    }

    private void DrawZoneHandles()
    {
        EditorGUI.BeginChangeCheck();
        
        Vector3 center = mapGen.transform.position + mapGen.zoneCenter;
        Vector3 size = mapGen.zoneSize;
        
        // Size handles
        Handles.color = Color.yellow;
        
        // X-axis handles
        Vector3 rightHandle = center + Vector3.right * size.x * 0.5f;
        Vector3 leftHandle = center - Vector3.right * size.x * 0.5f;
        
        rightHandle = Handles.Slider(rightHandle, Vector3.right);
        leftHandle = Handles.Slider(leftHandle, -Vector3.right);
        
        // Z-axis handles
        Vector3 forwardHandle = center + Vector3.forward * size.z * 0.5f;
        Vector3 backHandle = center - Vector3.forward * size.z * 0.5f;
        
        forwardHandle = Handles.Slider(forwardHandle, Vector3.forward);
        backHandle = Handles.Slider(backHandle, -Vector3.forward);
        
        // Y-axis handles
        Vector3 upHandle = center + Vector3.up * size.y * 0.5f;
        Vector3 downHandle = center - Vector3.up * size.y * 0.5f;
        
        upHandle = Handles.Slider(upHandle, Vector3.up);
        downHandle = Handles.Slider(downHandle, -Vector3.up);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(mapGen, "Resize Spawn Zone");
            
            // Calculate new size based on handle positions
            mapGen.zoneSize = new Vector3(
                Vector3.Distance(leftHandle, rightHandle),
                Vector3.Distance(downHandle, upHandle),
                Vector3.Distance(backHandle, forwardHandle)
            );
            
            EditorUtility.SetDirty(mapGen);
        }
    }
    
    private void DrawCenterHandle()
    {
        EditorGUI.BeginChangeCheck();
        
        Vector3 worldCenter = mapGen.transform.position + mapGen.zoneCenter;
        
        // Center position handle
        Handles.color = Color.cyan;
        Vector3 newWorldCenter = Handles.PositionHandle(worldCenter, Quaternion.identity);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(mapGen, "Move Spawn Zone Center");
            mapGen.zoneCenter = newWorldCenter - mapGen.transform.position;
            EditorUtility.SetDirty(mapGen);
        }
    }
}
