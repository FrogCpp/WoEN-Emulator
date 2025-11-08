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
        Vector3 pos;
        Quaternion rot;
        _I.GetWorldPose(out pos, out rot);
        WheelMesh.transform.position = pos;
        WheelMesh.transform.rotation = rot;

        _I.motorTorque = forwardSpeed * Power;
        _I.steerAngle += angularVelocity * Power;

        if (_I.steerAngle == 360) { _I.steerAngle = 0.1f; }

        if (_I.steerAngle == 0) { _I.steerAngle = 359.9f; }
    }

    public void Force(float motorUp, float motorDown)
    {
        Debug.Log("call!");

        UpMotor = Mathf.Lerp(motorUp, 1f, -1f);
        DownMotor = Mathf.Lerp(motorDown, 1f, -1f);

        forwardSpeed = (UpMotor + DownMotor) / 2f;
        angularVelocity = (UpMotor - DownMotor) / 2f;
    }
}
