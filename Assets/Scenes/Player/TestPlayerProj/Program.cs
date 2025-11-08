using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class main : RobotHardware
{
    public void Start()
    {
        foreach (var wheel in robot.Odometry)
        {
            wheel.Force(1.0f, 0.98f);
        }
    }
}