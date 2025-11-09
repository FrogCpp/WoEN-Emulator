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
    void Start()
    {
    #if UNITY_EDITOR
        UnityEngine.Debug.Log("Пропуск проверки в редакторе");
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
                UnityEngine.Debug.Log("Требуются права администратора!");

                RestartAsAdmin();

                Application.Quit();
                return;
            }

            UnityEngine.Debug.Log("Права администратора подтверждены!");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Ошибка проверки прав: {ex.Message}");
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
            UnityEngine.Debug.Log("Пользователь отклонил запрос прав администратора");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Ошибка перезапуска: {ex.Message}");
        }
    }
}
