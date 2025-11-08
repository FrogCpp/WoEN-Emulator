using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    [SerializeField] private WheelMotorController[] _odometry;

    public WheelMotorController[] Odometry
    {
        get => _odometry;
        private set => _odometry = value;
    }

    void Start()
    {
    }
}

public class RobotHardware
{
    protected Robot robot;

    public void Init(Robot _robot)
    {
        robot = _robot;
    }
}
