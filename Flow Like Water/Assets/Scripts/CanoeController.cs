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
    
    [Header("Rotation Alignment (Read-Only)")]
    public float rotationDirection; // -1 = opposite direction, 0 = perpendicular, 1 = perfect alignment
    public bool isRotatingRight;    // Which way we're turning
    
    void Update()
    {
        if (points.Count == 0) return;
        
        Vector3 currentTarget = points[currentPointIndex].position;
        
        // Calculate how aligned we are with target direction
        CalculateRotationAlignment(currentTarget);
        
        // Rotate toward current point
        RotateToward(currentTarget);
        
        // Check if close enough to switch to next point
        float distance = Vector3.Distance(transform.position, currentTarget);
        if (distance <= arrivalDistance)
        {
            GoToNextPoint();
        }
        
        GuiDebug.Instance.PrintFloat("rotationDirection", rotationDirection);
    }
    
    void CalculateRotationAlignment(Vector3 target)
    {
        Vector3 directionToTarget = target - transform.position;
        directionToTarget.y = 0f; // Keep horizontal
        
        if (directionToTarget.magnitude > 0.1f)
        {
            // Normalize both vectors for dot product
            Vector3 forwardDirection = transform.forward;
            forwardDirection.y = 0f;
            
            Vector3 targetDirection = directionToTarget.normalized;
            
            // Dot product gives us alignment: 1 = same direction, -1 = opposite, 0 = perpendicular
            rotationDirection = Vector3.Dot(forwardDirection.normalized, targetDirection);
            
            // Still calculate which way we're turning for other systems that might need it
            float signedAngle = Vector3.SignedAngle(forwardDirection, targetDirection, Vector3.up);
            isRotatingRight = signedAngle < 0f;
        }
        else
        {
            rotationDirection = 1f; // We're at the target, so we're "aligned"
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
            
            // Visual indicator of alignment using color intensity
            float alignmentStrength = Mathf.Abs(rotationDirection);
            Color alignmentColor = Color.Lerp(Color.red, Color.green, (rotationDirection + 1f) / 2f);
            alignmentColor.a = alignmentStrength;
            
            Gizmos.color = alignmentColor;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * alignmentStrength);
        }
    }
}
