using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Robot : MonoBehaviour
{
    public struct OdometryType
    {
        public WheelMotorController LeftUp;
        public WheelMotorController RightUp;
        public WheelMotorController LeftDown;
        public WheelMotorController RightDown;

        public Enumerator GetEnumerator() => new Enumerator(this);

        public struct Enumerator
        {
            private readonly OdometryType _odometry;
            private int _index;

            public Enumerator(OdometryType odometry)
            {
                _odometry = odometry;
                _index = -1;
            }

            public WheelMotorController Current
            {
                get
                {
                    return _index switch
                    {
                        0 => _odometry.LeftUp,
                        1 => _odometry.RightUp,
                        2 => _odometry.LeftDown,
                        3 => _odometry.RightDown,
                        _ => throw new InvalidOperationException()
                    };
                }
            }

            public bool MoveNext()
            {
                _index++;
                return _index < 4;
            }
        }
    }


    [SerializeField] private WheelMotorController[] _odometry;
    private ConsoleController _console;
    public OdometryType Odometry;

    private void Start()
    {
        Odometry.LeftUp = _odometry[0];
        Odometry.RightUp = _odometry[1];
        Odometry.LeftDown = _odometry[2];
        Odometry.RightDown = _odometry[3];

        _console = GameObject.FindWithTag("Console").GetComponent<ConsoleController>();
    }

    public void print(string text)
    {
        _console.Log(text);
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
