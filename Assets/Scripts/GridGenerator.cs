using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject line;
    public int lineNum = 10;

    private LineRenderer[] lines;
    private LineRenderer renderLine;
    private Vector3 pos = new Vector3(0, 0, 1);
    private Color LineColor = Color.black;
    private float OneMeterOpacity = 0.2f;
    private float TenMeterOpacity = 0.3f;

    // Start is called before the first frame update
    void Awake()
    {
        DrawLine();
    }

    private Color GetColor(float Opacity)
    {
        return new Color(LineColor.r, LineColor.g, LineColor.b, Opacity);
    }

    void DrawLine()
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = pos;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        //Shader shader = Shader.Find("Hidden/Internal-Colored");
        Shader shader = Shader.Find("Particles/Standard Unlit");
        var color = GetColor(OneMeterOpacity);
        lr.material = new Material(shader);
        lr.material.SetFloat("_Mode", 2f);
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = (0.03f);
        lr.endWidth = (0.03f);
        lr.SetPosition(0, new Vector3(-10.0f, 0));
        lr.SetPosition(1, new Vector3(10.0f, 0));
        //for (int i = lineNum/2 * -1; i <= lineNum; i++)
        //{
        //    Debug.Log(i);
        //}
    }
}
