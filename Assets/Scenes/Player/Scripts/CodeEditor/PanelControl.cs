using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelControl : MonoBehaviour
{
    [SerializeField] private TabScript ConsoleTab;
    [SerializeField] private TabScript DocumentationTab;

    public void CallConsole()
    {
        if (ConsoleTab.Target != 190)
        {
            ConsoleTab.Target = 190;
        }
        else
        {
            ConsoleTab.Target = 0;
        }
        if (DocumentationTab.Target == 190)
        {
            DocumentationTab.Target = 0;
        }
    }

    public void CallDoc()
    {
        if (DocumentationTab.Target != 190)
        {
            DocumentationTab.Target = 190;
        }
        else
        {
            DocumentationTab.Target = 0;
        }
        if (ConsoleTab.Target == 190)
        {
            ConsoleTab.Target = 0;
        }
    }
}
