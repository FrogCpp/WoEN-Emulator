using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    
    public void SelectProjFolder()
    {
        var path = GetPath();
        if (path == "") _console.msg("файл не выбран");
        _console.msg("Выбран файл: " + path);
    }

    private static string GetPath()
    {
        string powerShellScript = @"
        Add-Type -AssemblyName System.Windows.Forms
        $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
        $folderBrowser.Description = 'Choose project folder'
        $folderBrowser.RootFolder = 'MyComputer'
        if ($folderBrowser.ShowDialog() -eq 'OK') {
        return $folderBrowser.SelectedPath
        }";

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "powershell",
                Arguments = $"-Command \"{powerShellScript}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                string result = process.StandardOutput.ReadToEnd().Trim();
                return string.IsNullOrEmpty(result) ? "" : result + Path.DirectorySeparatorChar;
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Ошибка PowerShell: {e}");
            return "";
        }
    }
}
