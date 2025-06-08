using UnityEngine;

public class SimpleBuoyancy : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    public float waterLevel = 0f;           // Y position of water surface
    public float buoyancyForce = 10f;       // How strong the upward force is
    public float waterDrag = 0.5f;          // Resistance when in water
    public float waterAngularDrag = 0.8f;   // Rotational resistance
    
    [Header("Buoyancy Points")]
    public Transform[] buoyancyPoints;      // Points where buoyancy is calculated
    
    private Rigidbody rb;
    private bool isInWater = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // If no buoyancy points set, create them automatically
        if (buoyancyPoints.Length == 0)
        {
            CreateBuoyancyPoints();
        }
    }

    void FixedUpdate()
    {
        // Check each buoyancy point
        isInWater = false;
        
        foreach (Transform point in buoyancyPoints)
        {
            if (point.position.y < waterLevel)
            {
                isInWater = true;
                
                // Calculate how deep this point is underwater
                float depthMultiplier = (waterLevel - point.position.y) / 2f;
                
                // Apply upward buoyancy force at this point
                Vector3 buoyancyForceVector = Vector3.up * buoyancyForce * depthMultiplier;
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
            rb.angularDamping = 0.05f;  // Minimal angular drag in air
        }
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
                }
            }
        }
        
        // Draw water level
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        //Gizmos.DrawCube(new Vector3(0, waterLevel, 0), new Vector3(50, 0.1f, 50));
    }
}
