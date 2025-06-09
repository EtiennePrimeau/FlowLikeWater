using System;
using UnityEngine;

public class Current : MonoBehaviour
{
    [SerializeField] private float _currentForce = 5f;
    [SerializeField] private Vector3 _currentDirection = new Vector3(0, 0, 1);

    private Rigidbody _playerRB;

    private void Start()
    {
        _playerRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _playerRB.AddForce(_currentDirection.normalized * _currentForce, ForceMode.Force);
    }
}
