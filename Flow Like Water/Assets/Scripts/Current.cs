using System;
using UnityEngine;

public class Current : MonoBehaviour
{
    [SerializeField] private float _straightForce = 5f;
    [SerializeField] private float _turnForce = 2f;
    [SerializeField] private float _hardTurnForce = 2f;
    
    [SerializeField] private Transform _canoe;
    [SerializeField] private Transform _cameraTarget;

    private Rigidbody _playerRB;
    private Vector3 _currentDirection;
    
    
    private void Start()
    {
        _playerRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float force = 0;
        if (CanoeController.Instance.DelayedState == ECanoeState.straight)
            force = _straightForce;
        if (CanoeController.Instance.DelayedState == ECanoeState.turning)
            force = _turnForce;
        if (CanoeController.Instance.DelayedState == ECanoeState.hardTurning)
            force = _hardTurnForce;
        
        _currentDirection = _cameraTarget.position - _canoe.position;
        _playerRB.AddForce(_currentDirection.normalized * force, ForceMode.Force);
        
        Debug.DrawRay(_canoe.position, _cameraTarget.forward * 5, Color.yellow);
    }
}
