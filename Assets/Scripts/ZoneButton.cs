using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneButton : MonoBehaviour
{
    public GameObject LinePrefab;
    void OnGUI()
    {
        if (GUI.Button(new Rect(140, 70, 100, 30), "Create Zone"))
        {
            Debug.Log("Clicked the button with text");
            GameObject LineController = Instantiate(LinePrefab, new Vector3(0f, 0f), Quaternion.identity);
            LineController.GetComponent<LineController>().enabled = true;
        }
    }
}
