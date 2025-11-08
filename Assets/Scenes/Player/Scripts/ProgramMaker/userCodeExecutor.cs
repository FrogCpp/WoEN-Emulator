using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class userCodeExecutor : MonoBehaviour
{
    [SerializeField] private Robot robotLink;

    private string way = "C:\\Users\\kessokuBand\\Documents\\projects\\WoEN-Emulator\\Assets\\Scenes\\Player\\TestPlayerProj\\Program.cs";

    private PlayerCodeBuilder Builder;
    private bool _runing = false, collected = false;
    private Events ActualEvent;
    void Start()
    {
        Builder = new PlayerCodeBuilder();
    }
    void Update()
    {
        if (_runing && ActualEvent.Update != null)
        {
            try
            {
                ActualEvent.Update.Invoke(ActualEvent.userInstance, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Ошибка в пользовательском Update: {e}");
                _runing = false;
            }
        }
    }

    public void Build()
    {
        Debug.Log("trying to build");
        string code = File.ReadAllText(way);
        collected = Builder.CompileAndLoad(code, robotLink, out ActualEvent);

        Debug.Log("complite");
    }

    public void Run()
    {
        if (collected)
        {
            _runing = true;
            Debug.Log("Before invoking Start");
            Debug.Log(ActualEvent.Start);
            ActualEvent.Start?.Invoke(ActualEvent.userInstance, null);
            Debug.Log("After invoking Start");
        }
    }

    public void BuildAndRun()
    {
        Build();
        Run();
    }

    public void Stop()
    {
        if (_runing)
        {
            _runing = false;

            foreach (var wheel in robotLink.Odometry) // потом переделать в функцию резета в роботе
            {
                wheel.Force(0.0f, 0.0f);
            }
            Debug.Log("End!");
        }
    }
}
