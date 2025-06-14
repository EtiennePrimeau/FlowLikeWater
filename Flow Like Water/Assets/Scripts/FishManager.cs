using UnityEngine;
using System.Collections.Generic;

public class FishManager : MonoBehaviour
{
    [Header("Fish Settings")]
    public GameObject fishPrefab; // Your MandarinFish prefab
    public int fishCount = 20;
    public float spawnRadius = 15f;
    public float maxDistanceFromPlayer = 25f;
    
    [Header("Movement Settings")]
    public float swimSpeed = 2f;
    public float randomMoveStrength = 1f;
    public float sineFrequency = 1f;
    public float sineAmplitude = 0.5f;
    public float forwardDistance = 10f;
    
    private Transform player; // The Canoe
    private List<FishSwimmer> fishList = new List<FishSwimmer>();
    
    void Start()
    {
        player = CanoeController.Instance.transform;
        
        SpawnFish();
    }
    
    void SpawnFish()
    {
        for (int i = 0; i < fishCount; i++)
        {
            Vector3 spawnPos = GetRandomPositionAroundPlayer();
            GameObject newFish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);
            
            FishSwimmer swimmer = newFish.GetComponent<FishSwimmer>();
            if (swimmer == null)
                swimmer = newFish.AddComponent<FishSwimmer>();
            
            swimmer.Initialize(this, player, i);
            fishList.Add(swimmer);
        }
    }
    
    Vector3 GetRandomPositionAroundPlayer()
    {
        if (player == null) return Vector3.zero;
        
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = player.position + 
                                CanoeController.Instance.transform.forward * forwardDistance 
                                + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // Randomize Y position slightly
        spawnPosition.y = player.position.y + Random.Range(-2f, 0.5f);
        
        return spawnPosition;
    }
    
    public void TeleportFishIfTooFar(FishSwimmer fish)
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(fish.transform.position, player.position);
        if (distance > maxDistanceFromPlayer)
        {
            Vector3 newPos = GetRandomPositionAroundPlayer();
            fish.transform.position = newPos;
        }
    }
    
    public Vector3 GetPlayerPosition()
    {
        return player != null ? player.position : Vector3.zero;
    }
}
