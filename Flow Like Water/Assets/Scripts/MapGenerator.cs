using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    public List<GameObject> prefabsToSpawn = new List<GameObject>();
    
    [Header("Spawn Zone Settings")]
    public Vector3 zoneCenter = Vector3.zero;
    public Vector3 zoneSize = new Vector3(10f, 1f, 10f);
    
    [Header("Population Settings")]
    public int objectCount = 20;
    public float minSpacing = 2f;
    public bool avoidOverlap = true;
    public LayerMask groundLayer = 1; // Default layer
    
    [Header("Visual Settings")]
    public Color zoneColor = new Color(0f, 1f, 0f, 0.3f);
    public Color wireframeColor = Color.green;
    
    [Header("Random Color Settings")]
    public bool useRandomColors = true;
    public bool useRandomIntensity = true;
    public float minIntensity = 0.5f;
    public float maxIntensity = 2f;
    public List<Color> colorPalette = new List<Color> 
    { 
        Color.red, Color.green, Color.blue, Color.yellow, 
        Color.magenta, Color.cyan, Color.white 
    };
    
    [Header("Orientation Settings")]
    public bool alignToGround = true;
    public bool randomYRotation = true;
    public Vector3 upDirection = Vector3.up;
    
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    public void PopulateZone()
    {
        if (prefabsToSpawn.Count == 0)
        {
            Debug.LogWarning("No prefabs assigned to spawn!");
            return;
        }
        
        ClearSpawnedObjects();
        
        for (int i = 0; i < objectCount; i++)
        {
            Vector3 spawnPosition = GetRandomPositionInZone();
            
            if (avoidOverlap && IsPositionTooClose(spawnPosition))
            {
                // Try a few more times to find a good position
                bool foundGoodPosition = false;
                for (int attempts = 0; attempts < 10; attempts++)
                {
                    spawnPosition = GetRandomPositionInZone();
                    if (!IsPositionTooClose(spawnPosition))
                    {
                        foundGoodPosition = true;
                        break;
                    }
                }
                
                if (!foundGoodPosition) continue; // Skip this spawn
            }
            
            // Raycast down to place on ground
            Vector3 finalPosition = spawnPosition;
            Vector3 surfaceNormal = Vector3.up;
            
            if (Physics.Raycast(spawnPosition + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, groundLayer))
            {
                finalPosition = hit.point;
                surfaceNormal = hit.normal;
            }
            
            GameObject prefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];
            Quaternion rotation = CalculateRotation(surfaceNormal, prefab);
            GameObject spawnedObject = Instantiate(prefab, spawnPosition, rotation, transform);
            
            // Apply random color
            if (useRandomColors)
            {
                ApplyRandomColor(spawnedObject);
            }
            
            spawnedObjects.Add(spawnedObject);
        }
        
        Debug.Log($"Spawned {spawnedObjects.Count} objects in the zone");
    }
    
    private Quaternion CalculateRotation(Vector3 surfaceNormal, GameObject prefab)
    {
        // Start with the prefab's original rotation
        Quaternion prefabRotation = prefab.transform.rotation;
    
        Quaternion finalRotation = prefabRotation;
    
        if (alignToGround)
        {
            // Calculate the rotation needed to align to surface
            Quaternion groundAlignment = Quaternion.FromToRotation(upDirection, surfaceNormal);
        
            // Combine prefab rotation with ground alignment
            finalRotation = groundAlignment * prefabRotation;
        }
    
        if (randomYRotation)
        {
            // Add random Y rotation while preserving the alignment
            float randomY = Random.Range(0f, 360f);
            Quaternion yRotation = Quaternion.AngleAxis(randomY, surfaceNormal);
            finalRotation = yRotation * finalRotation;
        }
    
        return finalRotation;
    }
    
    private void ApplyRandomColor(GameObject obj)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer == null) return;
    
        // Create a new material instance (so we don't modify the original)
        Material newMaterial = new Material(renderer.material);
    
        // Choose random color
        Color randomColor;
        if (colorPalette.Count > 0)
        {
            randomColor = colorPalette[Random.Range(0, colorPalette.Count)];
        }
        else
        {
            randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
    
        // Apply random intensity if enabled
        if (useRandomIntensity)
        {
            float intensity = Random.Range(minIntensity, maxIntensity);
            randomColor *= intensity;
        }
    
        // Set the color (works for both URP and Built-in materials)
        if (newMaterial.HasProperty("_BaseColor"))
        {
            newMaterial.SetColor("_BaseColor", randomColor);
        }
        else if (newMaterial.HasProperty("_Color"))
        {
            newMaterial.SetColor("_Color", randomColor);
        }
    
        // Assign the new material
        renderer.material = newMaterial;
    }

    
    public void ClearSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                DestroyImmediate(spawnedObjects[i]);
            }
        }
        spawnedObjects.Clear();
    }
    
    private Vector3 GetRandomPositionInZone()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-zoneSize.x * 0.5f, zoneSize.x * 0.5f),
            Random.Range(-zoneSize.y * 0.5f, zoneSize.y * 0.5f),
            Random.Range(-zoneSize.z * 0.5f, zoneSize.z * 0.5f)
        );
        
        return transform.position + zoneCenter + randomOffset;
    }
    
    private bool IsPositionTooClose(Vector3 position)
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null && Vector3.Distance(position, obj.transform.position) < minSpacing)
            {
                return true;
            }
        }
        return false;
    }
    
    public Bounds GetZoneBounds()
    {
        return new Bounds(transform.position + zoneCenter, zoneSize);
    }
}
