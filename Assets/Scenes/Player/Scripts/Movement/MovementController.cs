using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private float speed, sens;
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


        if (Input.GetMouseButton(1))
        {

            Vector3 Move = transform.forward * Vert + transform.right * Hori;
            Move *= speed;

            _rb.velocity = Move;

            transform.rotation = Quaternion.AngleAxis(MouseHori * sens, Vector3.up) * transform.rotation;
            transform.rotation *= Quaternion.AngleAxis(MouseVert * sens * -1, Vector3.right);
        }
    }
}
