using UnityEngine;

public class SimpleBuoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public float waterLevel = 0f;           // Y position of water surface
    public float buoyancyForce = 10f;       // How strong the upward force is
    public float waterDrag = 0.5f;          // Resistance when in water
    public float waterAngularDrag = 0.8f;   // Rotational resistance
    
    [Header("Wobble Settings")]
    public float wobbleStrength = 0.3f;     // How much the water level varies
    public float wobbleSpeed = 1.5f;        // How fast the wobble moves
    public float buoyancyVariation = 0.2f;  // How much buoyancy force varies
    
    [Header("Buoyancy Points")]
    public Transform[] buoyancyPoints;      // Points where buoyancy is calculated
    
    private Rigidbody rb;
    private bool isInWater = false;
    private float wobbleTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // If no buoyancy points set, create them automatically
        if (buoyancyPoints.Length == 0)
        {
            CreateBuoyancyPoints();
        }
        
        // Start with random timer so multiple objects don't sync
        wobbleTimer = Random.Range(0f, Mathf.PI * 2f);
    }

    void FixedUpdate()
    {
        // Update wobble timer
        wobbleTimer += Time.fixedDeltaTime * wobbleSpeed;
        if (wobbleTimer > Mathf.PI * 2f)
        {
            wobbleTimer -= Mathf.PI * 2f;
        }
        
        // Check each buoyancy point
        isInWater = false;
        
        for (int i = 0; i < buoyancyPoints.Length; i++)
        {
            Transform point = buoyancyPoints[i];
            
            // Calculate local water level with wobble for this point
            float localWaterLevel = GetLocalWaterLevel(point.position, i);
            
            if (point.position.y < localWaterLevel)
            {
                isInWater = true;
                
                // Calculate how deep this point is underwater
                float depthMultiplier = (localWaterLevel - point.position.y) / 2f;
                
                // Add some variation to buoyancy force for natural feel
                float wobbledBuoyancyForce = buoyancyForce + (Mathf.Sin(wobbleTimer + i) * buoyancyVariation * buoyancyForce);
                
                // Apply upward buoyancy force at this point
                Vector3 buoyancyForceVector = Vector3.up * wobbledBuoyancyForce * depthMultiplier;
                rb.AddForceAtPosition(buoyancyForceVector, point.position);
            }
        }
        
        // Apply water resistance when in water
        if (isInWater)
        {
            rb.linearDamping = waterDrag;
            rb.angularDamping = waterAngularDrag;
        }
        else
        {
            rb.linearDamping = 0.1f;          // Air resistance
            rb.angularDamping = 0.05f;        // Minimal angular drag in air
        }
    }

    float GetLocalWaterLevel(Vector3 position, int pointIndex)
    {
        // Create slightly different wobble for each buoyancy point
        float baseWobble = Mathf.Sin(wobbleTimer + position.z * 0.1f) * wobbleStrength;
        float secondaryWobble = Mathf.Cos(wobbleTimer * 0.7f + position.x * 0.1f + pointIndex) * wobbleStrength * 0.5f;
        
        return waterLevel + baseWobble + secondaryWobble;
    }

    void CreateBuoyancyPoints()
    {
        // Automatically create 4 buoyancy points for canoe corners
        buoyancyPoints = new Transform[4];
        
        float length = 1.5f;  // Adjust based on your canoe size
        float width = 0.6f;
        
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-width, 0, length),   // Front left
            new Vector3(width, 0, length),    // Front right
            new Vector3(-width, 0, -length),  // Back left
            new Vector3(width, 0, -length)    // Back right
        };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject point = new GameObject($"BuoyancyPoint_{i}");
            point.transform.parent = transform;
            point.transform.localPosition = positions[i];
            buoyancyPoints[i] = point.transform;
        }
    }

    // Optional: Visualize buoyancy points in Scene view
    void OnDrawGizmosSelected()
    {
        if (buoyancyPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in buoyancyPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.1f);
                    
                    // Show local water level for this point
                    if (Application.isPlaying)
                    {
                        float localLevel = GetLocalWaterLevel(point.position, System.Array.IndexOf(buoyancyPoints, point));
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireCube(new Vector3(point.position.x, localLevel, point.position.z), new Vector3(0.2f, 0.02f, 0.2f));
                        Gizmos.color = Color.cyan;
                    }
                }
            }
        }
        
        // Draw base water level
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        //Gizmos.DrawCube(new Vector3(0, waterLevel, 0), new Vector3(50, 0.1f, 50));
    }
}
