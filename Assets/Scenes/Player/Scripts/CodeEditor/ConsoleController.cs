using TMPro;
using UnityEngine;

public class ConsoleController : MonoBehaviour
{
    [SerializeField] private TMP_InputField textField;

    private void Start()
    {
        textField.onSubmit.AddListener(ExecutePlayerCommand);
        textField.ActivateInputField();
    }

    private void ExecutePlayerCommand(string command)
    {
        if (string.IsNullOrEmpty(command)) return;

        if (command == "clear")
        {
            textField.text = "";
        }
        else
        {
            textField.text += "\n<color=orange>sorry, but this is not a command</color>";
        }

            textField.ActivateInputField();
    }

    public void Error(object Log)
    {
        textField.text += $"\n<color=red>{Log.ToString()}</color>";
    }

    public void msg(object Log)
    {
        textField.text += $"\n<color=white>{Log.ToString()}</color>";
    }

    public void Log(object msg)
    {
        textField.text += $"\n<color=green>{msg.ToString()}</color>";
    }
}
