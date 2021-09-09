using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    private enum Direction
    {
        DirX,
        DirY,
        DirZ,
    }

    public GameObject GO;
    //public enum GridColor_
    //{
    //    Black,
    //    Blue,
    //    Red,
    //}

    //public GridColor_ GridColor = GridColor_.Black;


    private int Row = 100;

    private int Col = 100;

    private Color LineColor = Color.black;

    private Direction MyDirection;

    private float OneMeterOpacity = 0.2f;
    private float TenMeterOpacity = 0.3f;

    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            //Shader shader = Shader.Find("Particles/Standard Unlit");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 1);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw Scene
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        DrawGrid(Row, Col);
        GL.PopMatrix();
    }

    private void OnRenderObject()
    {
        if (Camera.current.name != "Main Camera")
            return;

        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        DrawGrid(Row, Col);
        GL.PopMatrix();
    }

    //private Color GetAlphaDistance(int i)
    //{
    //    double alpha = 1f;
    //    if (i < 0)
    //        i = i * -1;
    //
    //    if (i == 0)
    //        alpha = 1f;
    //
    //    double temp = (double)i / (double)Row;
    //    alpha = temp * 100f;
    //    alpha = 1 - (alpha / 100);
    //
    //    if (alpha > 0.8)
    //        alpha = 0.8f;
    //
    //    return new Color(LineColor.r, LineColor.g, LineColor.b, (float)alpha);
    //}

    private Color GetColor(float Opacity)
    {
        return new Color(LineColor.r, LineColor.g, LineColor.b, Opacity);
    }

    

    void DrawGrid(int row, int col)
    {
        GL.Begin(GL.LINES);
        GL.Color(LineColor);
        // 1M row
        for (int i = -row; i <= row; i++)
        {
            GL.Color(GetColor(OneMeterOpacity));
            GL.Vertex3((float)-row, (float)i, 0);
            GL.Vertex3((float)row, (float)i, 0);
        }

        // 1M col
        for (int i = -col; i <= col; i++)
        {
            GL.Color(GetColor(OneMeterOpacity));
            GL.Vertex3((float)i, (float)-col, 0);
            GL.Vertex3((float)i, (float)col, 0);
        }

        // 10M row
        for (int i = -row; i <= row; i++)
        {
            if (i % 10 != 0)
            {
                continue;
            }
            GL.Color(GetColor(TenMeterOpacity));
            GL.Vertex3((float)-row, (float)i, 0);
            GL.Vertex3((float)row, (float)i, 0);
        }

        // 10M col
        for (int i = -col; i <= col; i++)
        {
            if (i % 10 != 0)
            {
                continue;
            }
            GL.Color(GetColor(TenMeterOpacity));
            GL.Vertex3((float)i, (float)-col, 0);
            GL.Vertex3((float)i, (float)col, 0);
        }


        GL.End();
    }

    public void ColorSelect(int index)
    {
        if (index == 0)
        {
            LineColor = Color.black;
        }
        if (index == 1)
        {
            LineColor = Color.red;
        }
        if (index == 2)
        {
            LineColor = Color.blue;
        }
    }

    public void OnOffGrid(bool index)
    {
        if (index == true)
        {
            GO.SetActive(true);
        }
        if (index == false)
        {
            GO.SetActive(false);
        }
    }
}