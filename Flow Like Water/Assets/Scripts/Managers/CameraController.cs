using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public CanoeController canoe;
    public Vector3 offset = new Vector3(0, 5, -8);
    
    [Header("Smoothing")]
    public float positionSmoothTime = 0.3f;
    public float rotationSmoothTime = 0.2f;
    public float maxSpeed = Mathf.Infinity;
    
    [Header("Dynamic Offset")]
    public bool adjustOffsetByVelocity = false;
    public float velocityInfluence = 2f;
    
    private Vector3 positionVelocity = Vector3.zero;
    private Vector3 rotationVelocity = Vector3.zero;

    void FixedUpdate()
    {
        if (canoe == null) return;
        
        Vector3 currentOffset = offset;
        
        // Adjust offset based on target velocity (optional)
        if (adjustOffsetByVelocity)
        {
            Rigidbody targetRb = canoe.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 velocityOffset = targetRb.linearVelocity * velocityInfluence;
                currentOffset += new Vector3(0, 0, -velocityOffset.magnitude);
            }
        }

        
        // Smooth position
        //Vector3 desiredPosition = canoe.transform.position + currentOffset;
        Vector3 rotatedOffset = canoe.transform.TransformDirection(currentOffset);
        Vector3 desiredPosition = canoe.transform.position + rotatedOffset;
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref positionVelocity, 
            positionSmoothTime, 
            maxSpeed
        );
        
        // Smooth rotation to look at target
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            Time.deltaTime / rotationSmoothTime
        );

    }
}