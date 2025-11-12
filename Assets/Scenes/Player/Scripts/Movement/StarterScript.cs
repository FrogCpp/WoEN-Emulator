using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using UnityEngine;

public class StarterScript : MonoBehaviour
{
    private ConsoleController _console;
    void Start()
    {
        _console = GameObject.FindWithTag("Console").GetComponent<ConsoleController>();
    #if UNITY_EDITOR
        _console.Log("Пропуск проверки в редакторе");
    #else
        CheckAdminRights();
    #endif
    }

    private void CheckAdminRights()
    {
        try
        {
            if (!IsRunningAsAdmin())
            {
                _console.Error("Требуются права администратора!");

                RestartAsAdmin();

                Application.Quit();
                return;
            }

            _console.Log("Права администратора подтверждены!");

            adjustingSettings();
        }
        catch (Exception ex)
        {
            _console.Error($"Ошибка проверки прав: {ex.Message}");
            Application.Quit();
        }
    }

    private bool IsRunningAsAdmin()
    {
        try
        {
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string testFile = Path.Combine(systemPath, "test_admin.tmp");

            try
            {
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void RestartAsAdmin()
    {
        try
        {
            var currentProcess = Process.GetCurrentProcess();
            var processStartInfo = new ProcessStartInfo
            {
                FileName = currentProcess.MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(processStartInfo);
        }
        catch (Win32Exception)
        {
            _console.Error("Пользователь отклонил запрос прав администратора");
        }
        catch (Exception ex)
        {
            _console.Error($"Ошибка перезапуска: {ex.Message}");
        }
    }

    private void adjustingSettings()
    {
        string batDir = Path.Combine(Application.dataPath, "../ref");
        string batPath = Path.GetFullPath(Path.Combine(batDir, "config.bat"));

        if (File.Exists(batPath))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C \"{batPath}\"";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;

            Process.Start(startInfo);
        }
        else
        {
            UnityEngine.Debug.LogError($"Bat file not found: {batPath}");
        }
    }
}
