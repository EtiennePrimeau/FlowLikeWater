using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public enum ECanoeState { straight, turning, hardTurning }

public class CanoeController : MonoBehaviour
{
    public static CanoeController Instance;
    
    public RhythmInputSystem rhythmInputSystem;
    public Transform perfectSpot;
    
    [Header("Waypoints")]
    public List<Transform> points = new List<Transform>();
    
    [Header("Settings")]
    public CanoeParams canoeParams;
    public float arrivalDistance = 2f;
    public float turnThreshold = .4f;
    public float straightThreshold = .9f;
    public float stateDelayTime = 1f;
    
    
    [Header("Debug")]
    public bool showDebug = true;
    
    private int currentPointIndex = 0;
    
    [Header("Rotation Alignment (Read-Only)")]
    public float rotationDirection; // -1 = opposite direction, 0 = perpendicular, 1 = perfect alignment
    public bool isRotatingRight;    // Which way we're turning
    public bool isRotatingRightDelayed;    
    
    private ECanoeState canoeState = ECanoeState.straight;
    private ECanoeState delayedState = ECanoeState.straight;
    private float rotationSpeed;
    private Vector3 _currentTarget;
    private Vector3 delayedTarget;
    private Coroutine delayedStateCoroutine;
    
    public Vector3 CurrentTarget => _currentTarget;
    
    public ECanoeState CurrentState => canoeState;
    public ECanoeState DelayedState => delayedState;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        stateDelayTime = rhythmInputSystem.spawnDistance / PromptObject.FallSpeed - 0.5f;
        ChangeState(ECanoeState.straight);
        _currentTarget = points[0].position;
    }

    void Update()
    {
        if (points.Count == 0) return;
        
        _currentTarget = points[currentPointIndex].position;
        
        // Calculate how aligned we are with target direction
        CalculateRotationAlignment(_currentTarget);
        
        // Rotate toward current point
        RotateToward(_currentTarget);
        
        // Check if close enough to switch to next point
        float distance = Vector3.Distance(transform.position, _currentTarget);
        if (distance <= arrivalDistance)
        {
            GoToNextPoint();
        }
        
        //GuiDebug.Instance.PrintFloat("rotationDirection", rotationDirection);
        //GuiDebug.Instance.PrintString("canoe state", canoeState.ToString());
        //GuiDebug.Instance.PrintString("delayed state", delayedState.ToString());
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
            isRotatingRight = signedAngle > 0f;
        }
        else
        {
            rotationDirection = 1f; // We're at the target, so we're "aligned"
        }

        SetCurrentState();
    }

    void SetCurrentState()
    {
        //GuiDebug.Instance.PrintString("state", canoeState.ToString());
        //GuiDebug.Instance.PrintFloat("rot dir", rotationDirection);
        
        if (rotationDirection > straightThreshold)
        {
            ChangeState(ECanoeState.straight);
            return;
        }

        if (rotationDirection > turnThreshold)
        {
            ChangeState(ECanoeState.turning);
            return;
        }
        
        ChangeState(ECanoeState.hardTurning);
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
            GameManager.Instance.LevelCompleted();
        }
    }

    void ChangeState(ECanoeState newState)
    {
        if (newState != canoeState)
        {
            //Debug.Log("state changed from : " + canoeState + " to : " + newState);
            //State Change
            canoeState = newState;
            
            StartCoroutine(ChangeDelayedState());
            //GuiDebug.Instance.PrintFloat("rot speed", rotationSpeed);
                        
        }
    }
    
    IEnumerator ChangeDelayedState()
    {
        Vector3 newTarget = _currentTarget;
        ECanoeState newDelayedState = canoeState;
        bool isRotatinRight = isRotatingRight;
        yield return new WaitForSeconds(stateDelayTime);
    
        delayedState = newDelayedState;
        isRotatingRightDelayed = isRotatinRight;
        delayedTarget = newTarget;
        //Debug.Log($"Coroutine :: State changed to: {newDelayedState}");
        
        if (delayedState == ECanoeState.hardTurning)
            rotationSpeed = canoeParams.turnRotationSpeed;
        else if (delayedState == ECanoeState.turning)
            rotationSpeed = canoeParams.regRotationSpeed;
        else
            rotationSpeed = 1;

        Material material = perfectSpot.gameObject.GetComponent<Renderer>().material;
        switch (delayedState)
        {
            case ECanoeState.hardTurning:
                material.color = Color.red;
                break;
            case ECanoeState.turning:
                material.color = Color.yellow;
                break;
            case ECanoeState.straight:
                material.color = Color.blue;
                break;
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebug || points.Count == 0) return;

        if (delayedTarget != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(delayedTarget, 1f);
        }
        
        // Draw all points
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.color = (i == currentPointIndex) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(points[i].position, 0.5f);

            
            if (i == 0)
                continue;
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(points[i].position, points[i - 1].position);
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
