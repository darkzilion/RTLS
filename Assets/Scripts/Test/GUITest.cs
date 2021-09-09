using UnityEngine;
using System.Collections;

public class GUITest : MonoBehaviour
{
    public Texture2D icon;

    void OnGUI()
    {
        GUI.Button(new Rect(10, 10, 100, 50), new GUIContent("This is A", "A"));
        GUI.Button(new Rect(10, 110, 100, 50), new GUIContent("This is B", "B"));
        GUI.Button(new Rect(10, 210, 100, 50), new GUIContent("This is C", "C"));

        if (GUI.tooltip == "A")
        {
            GUI.Label(new Rect(10, 40, 100, 50), GUI.tooltip);
        }

        if (GUI.tooltip == "B")
        {
            GUI.Label(new Rect(10, 140, 100, 50), GUI.tooltip);
        }

        if (GUI.tooltip == "C")
        {
            GUI.Label(new Rect(10, 240, 200, 50), GUI.tooltip);
        }
    }
}
