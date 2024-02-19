using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellProjectile : MonoBehaviour
{
    [SerializeField]
    private float projectileSpeed = 10.0f;

    private Rigidbody _rigidBody;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 movementDirection = transform.rotation * Vector3.forward;
        _rigidBody.MovePosition(_rigidBody.position + movementDirection * projectileSpeed * Time.deltaTime);
    }
}
