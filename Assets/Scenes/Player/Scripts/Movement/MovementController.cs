using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private float speed;
    private float Hori, Vert;
    private float MouseHori, MouseVert;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Hori = Input.GetAxis("Horizontal");
        Vert = Input.GetAxis("Vertical");

        MouseHori = Input.GetAxis("Mouse X");
        MouseVert = Input.GetAxis("Mouse Y");

        Vector3 Move = Vector3.forward * Hori + Vector3.right * Vert;
        Move *= speed;

        _rb.velocity = Move;

        transform.rotation = Quaternion.Euler(MouseHori * speed, MouseVert * speed, 0f);
    }
}
