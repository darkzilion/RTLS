using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTest : MonoBehaviour
{
    public Material material;

    void Start()
    {
        Vector3[] verticles = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        Mesh mesh = new Mesh();
        
        verticles[0] = new Vector3(0, 20);
        verticles[1] = new Vector3(20, 20);
        verticles[2] = new Vector3(0, 0);
        verticles[3] = new Vector3(20, 0);

        uv[0] = new Vector2(0,20);
        uv[1] = new Vector2(20,20);
        uv[2] = new Vector2(0,0);
        uv[3] = new Vector2(20,0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;

        mesh.vertices = verticles;
        mesh.uv = uv;
        mesh.triangles = triangles;

        GameObject gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material = material;
    }

}