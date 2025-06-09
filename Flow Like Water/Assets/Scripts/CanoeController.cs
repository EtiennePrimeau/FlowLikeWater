using UnityEngine;
using System.Collections.Generic;

public class CanoeController : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> points = new List<Transform>();
    
    [Header("Settings")]
    public float rotationSpeed = 50f;
    public float arrivalDistance = 2f;
    
    [Header("Debug")]
    public bool showDebug = true;
    
    private int currentPointIndex = 0;
    
    void Update()
    {
        if (points.Count == 0) return;
        
        Vector3 currentTarget = points[currentPointIndex].position;
        
        // Rotate toward current point
        RotateToward(currentTarget);
        
        // Check if close enough to switch to next point
        float distance = Vector3.Distance(transform.position, currentTarget);
        if (distance <= arrivalDistance)
        {
            GoToNextPoint();
        }
    }
    
    void RotateToward(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f; // Keep horizontal
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }
    
    void GoToNextPoint()
    {
        currentPointIndex++;
        if (currentPointIndex >= points.Count)
        {
            currentPointIndex = 0; // Loop back to start
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebug || points.Count == 0) return;
        
        // Draw all points
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.color = (i == currentPointIndex) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(points[i].position, 0.5f);
        }
        
        // Draw arrival radius around current target
        if (Application.isPlaying && points.Count > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(points[currentPointIndex].position, arrivalDistance);
            
            // Draw line to current target
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, points[currentPointIndex].position);
        }
    }
}
