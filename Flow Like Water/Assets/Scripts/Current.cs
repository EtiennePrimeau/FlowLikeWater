using System;
using UnityEngine;

public class Current : MonoBehaviour
{
    [SerializeField] private float _currentForce = 5f;
    
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
        _currentDirection = _cameraTarget.position - _canoe.position;
        _playerRB.AddForce(_currentDirection.normalized * _currentForce, ForceMode.Force);
        
        Debug.DrawRay(_canoe.position, _cameraTarget.forward * 5, Color.yellow);
    }
}
