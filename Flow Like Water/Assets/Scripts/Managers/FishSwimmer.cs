using UnityEngine;

public class FishSwimmer : MonoBehaviour
{
    private FishManager manager;
    private Transform player;
    private Vector3 randomDirection;
    private float sineOffset;
    private float changeDirectionTimer;
    private float baseY;
    
    [Header("Individual Fish Settings")]
    public float individualSpeed = 1f;
    public float directionChangeInterval = 3f;
    
    public void Initialize(FishManager fishManager, Transform playerTransform, int fishIndex)
    {
        manager = fishManager;
        player = playerTransform;
        
        // Give each fish unique characteristics
        sineOffset = fishIndex * 0.5f; // Offset sine waves
        individualSpeed = Random.Range(0.8f, 1.2f); // Vary speed
        directionChangeInterval = Random.Range(2f, 5f);
        
        baseY = transform.position.y;
        GetNewRandomDirection();
    }
    
    void Update()
    {
        if (player == null || manager == null) return;
        
        MoveRandomly();
        ApplySwimmingMotion();
        FaceMovementDirection();
        
        // Check distance periodically
        manager.TeleportFishIfTooFar(this);
        
        // Change direction periodically
        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0)
        {
            GetNewRandomDirection();
            changeDirectionTimer = directionChangeInterval;
        }
    }
    
    void MoveRandomly()
    {
        // Move in random direction with some tendency to stay near player
        Vector3 toPlayer = (player.position + 
            CanoeController.Instance.transform.forward * manager.forwardDistance -
            transform.position).normalized;
        Vector3 moveDirection = Vector3.Lerp(randomDirection, toPlayer, 0.3f);
        
        transform.position += moveDirection * manager.swimSpeed * individualSpeed * Time.deltaTime;
    }
    
    void ApplySwimmingMotion()
    {
        // Create swimming motion with sine wave
        float sineValue = Mathf.Sin((Time.time + sineOffset) * manager.sineFrequency) * manager.sineAmplitude;
        
        Vector3 pos = transform.position;
        pos.y = baseY + sineValue;
        transform.position = pos;
        
        // Update base Y occasionally to follow terrain/water level
        if (Random.Range(0, 100) < 1) // 1% chance per frame
        {
            baseY = Mathf.Lerp(baseY, player.position.y - Random.Range(0.5f, 3f), 0.1f);
        }
    }
    
    void FaceMovementDirection()
    {
        // Make fish face the direction they're swimming
        if (randomDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
        }
    }
    
    void GetNewRandomDirection()
    {
        // Get new random direction, but bias it towards staying near player
        randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        ).normalized;
        
        // Add some randomness to Y direction occasionally
        if (Random.Range(0, 4) == 0) // 25% chance
        {
            randomDirection.y = Random.Range(-0.3f, 0.3f);
        }
    }
}
