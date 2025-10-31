using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelMotorController : MonoBehaviour
{
    [SerializeField] private GameObject WheelMesh;
    [SerializeField] private float Power;

    private float UpMotor, DownMotor, forwardSpeed, angularVelocity;
    private WheelCollider _I;


    void Start()
    {
        _I = GetComponent<WheelCollider>();
    }
    void FixedUpdate()
    {
        WheelMesh.transform.SetPositionAndRotation(transform.position, transform.rotation);

        _I.motorTorque = forwardSpeed * Power;
        _I.rotationSpeed = angularVelocity * Power;
    }

    public void Force(float motorUp, float motorDown)
    {
        UpMotor = Mathf.Lerp(motorUp, -1, 1);
        DownMotor = Mathf.Lerp(motorDown, -1, 1);

        forwardSpeed = (UpMotor + DownMotor) / 2;
        angularVelocity = (UpMotor - DownMotor) / 2;
    }
}
