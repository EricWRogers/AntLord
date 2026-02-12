using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    public Vector3 pos;
    public Mesh mesh;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();

    private int lastVertex;

    private void Start()
    {
        //Initialise the mesh
        mesh = new Mesh();
        //Create mesh data
        DrawCube();
        //Set the mesh data
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.SetUVs(0, uvs.ToArray());
        //recalculate lighting
        mesh.RecalculateNormals();
        //Set the mesh
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void DrawCube()
    {
        Front_GenerateFace();
        Back_GenerateFace();
        Right_GenerateFace();
        Left_GenerateFace();
        Top_GenerateFace();
        Bottom_GenerateFace();
    }

    void Front_GenerateFace()
    {
        lastVertex = vertices.Count;

        //Declare vertices
        vertices.Add(pos + Vector3.forward);                                    //0
        vertices.Add(pos + Vector3.forward + Vector3.up);                       //1
        vertices.Add(pos + Vector3.forward + Vector3.up + Vector3.right);       //2
        vertices.Add(pos + Vector3.forward + Vector3.right);                    //3

        //first triangle
        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);
    }
    void Back_GenerateFace()
    {
        lastVertex = vertices.Count;

        //Declare vertices
        vertices.Add(pos + Vector3.right);                                    //0
        vertices.Add(pos + Vector3.up + Vector3.right);                       //1
        vertices.Add(pos + Vector3.up);       //2
        vertices.Add(pos);                    //3

        //first triangle
        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);
    }
    void Right_GenerateFace()
    {
        lastVertex = vertices.Count;

        //Declare vertices
        vertices.Add(pos + Vector3.right + Vector3.forward);                                    //0
        vertices.Add(pos + Vector3.one);                       //1
        vertices.Add(pos + Vector3.right + Vector3.up);       //2
        vertices.Add(pos + Vector3.right);                    //3

        //first triangle
        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);
    }
    void Left_GenerateFace()
    {
        lastVertex = vertices.Count;

        //Declare vertices
        vertices.Add(pos);                                    //0
        vertices.Add(pos + Vector3.up);                       //1
        vertices.Add(pos + Vector3.forward + Vector3.up);       //2
        vertices.Add(pos + Vector3.forward);                    //3

        //first triangle
        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);
    }
    void Top_GenerateFace()
    {
        lastVertex = vertices.Count;

        //Declare vertices
        vertices.Add(pos + Vector3.up + Vector3.right);       //0
        vertices.Add(pos + Vector3.one);                      //1
        vertices.Add(pos + Vector3.forward + Vector3.up);       //2
        vertices.Add(pos + Vector3.up);                    //3

        //first triangle
        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);
    }
    void Bottom_GenerateFace()
    {
        lastVertex = vertices.Count;

        //Declare vertices
        vertices.Add(pos);       //0
        vertices.Add(pos + Vector3.forward);                      //1
        vertices.Add(pos + Vector3.forward + Vector3.right);       //2
        vertices.Add(pos + Vector3.right);                    //3

        //first triangle
        triangles.Add(lastVertex + 2);
        triangles.Add(lastVertex + 1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex + 3);
        triangles.Add(lastVertex + 2);
    }
}
