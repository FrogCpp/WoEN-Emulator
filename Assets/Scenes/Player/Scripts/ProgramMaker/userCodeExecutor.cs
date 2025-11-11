using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class userCodeExecutor : MonoBehaviour
{
    [SerializeField] private Robot robotLink;

    private ConsoleController _console;

    private string way = "";

    private PlayerCodeBuilder Builder;
    private bool _runing = false, collected = false;
    private Events ActualEvent;
    void Start()
    {
        _console = GameObject.FindWithTag("Console").GetComponent<ConsoleController>();
        Builder = new PlayerCodeBuilder(_console);
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
                _console.Error($"❌ Ошибка в пользовательском Update: {e}");
                _runing = false;
            }
        }
    }

    public void Build()
    {
        _console.msg($"🔨 Компилируем проект: {way}");
        collected = Builder.CompileProject(way, robotLink, out ActualEvent);

        if (collected)
        {
            _console.Log($"✅ Проект скомпилирован успешно!");
            _console.msg($"   Start метод: {ActualEvent.Start != null}");
            _console.msg($"   Update метод: {ActualEvent.Update != null}");
        }
        else
        {
            _console.Error("❌ Ошибка компиляции проекта");
        }
    }

    public void Run()
    {
        if (collected)
        {
            _runing = true;
            ActualEvent.Start?.Invoke(ActualEvent.userInstance, null);
            _console.Log("✅ Начало исполнения!");
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

            _console.Error("❌ Остановка выполнения кода!");
            foreach (var wheel in robotLink.Odometry) // сделано здесь, тк если делать в роботе, то это костыли
            {
                wheel.ResetPower();
            }
        }
    }

    [MenuItem("Tools/Select Project Folder")]
    public void SelectProjFolder()
    {
        string path = EditorUtility.OpenFolderPanel("Choose project folder", "C:/", "");

        if (!string.IsNullOrEmpty(path))
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path += Path.DirectorySeparatorChar;

            _console.msg("Выбран файл: " + path);
            way = path;
        }
    }
}
