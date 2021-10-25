using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    void OnGUI()
    {
        if (GUI.Button(new Rect(270, 70, 100, 30), "Restart"))
        {
            Debug.Log("Restart Button Clicked ");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}