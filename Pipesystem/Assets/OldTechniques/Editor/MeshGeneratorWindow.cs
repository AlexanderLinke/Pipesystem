using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshGeneratorWindow : EditorWindow {

    Mesh mesh;
    MeshGenerator meshGenerator;

    private Vector3[] vertices;
    private int[] triangles;

    private int rows = 1;
    private int oldRows;

    private float oldzOffset;

    [MenuItem("Window/Mesh Generator")]
    public static void ShowWindow()
    {
        GetWindow<MeshGeneratorWindow>("MeshGenerator");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Link"))
            LinkMeshGenerator();

        if (meshGenerator == null)
            return;

        if (GUILayout.Button("Trinagle"))
            CalculateTriangle();

        if (rows!=oldRows|| oldzOffset!=meshGenerator.zOffset)
        {
            CalculatePipe();

            if (rows == 1)
                meshGenerator.drawBaseShape = true;
            else
                meshGenerator.drawBaseShape = false;

            oldRows = rows;
            meshGenerator.rows = rows;
            oldzOffset = meshGenerator.zOffset;
        }

        GUILayout.Label("Row Count: " + rows.ToString());
        rows = (int)GUILayout.HorizontalSlider(rows, 1, 20);

        GUILayout.Label("Z Offset: " + meshGenerator.zOffset.ToString());
        meshGenerator.zOffset = GUILayout.HorizontalSlider(meshGenerator.zOffset, 0, 2);


        if (GUILayout.Button("Delete"))
            mesh.Clear();
    }


    void LinkMeshGenerator()
    {
        mesh = new Mesh();

        if (meshGenerator != null)
            meshGenerator.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        else
        {
            meshGenerator = Selection.activeGameObject.GetComponent<MeshGenerator>();
            if(meshGenerator!=null)
                meshGenerator.gameObject.GetComponent<MeshFilter>().mesh = mesh;
        }

    }
	
	void CalculateTriangle()
    {
        vertices = new Vector3[3];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, 1);
        vertices[2] = new Vector3(1, 0, 0);

        triangles = new int[3];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void CalculatePipe()
    {
        int count = meshGenerator.baseShape.Length;

        //vertices
        vertices = new Vector3[rows * count];

        for (int i = 0; i < rows; i++)
        {
            for(int j = 0; j < count; j++)
            {
                vertices[i * count + j] = new Vector3(meshGenerator.baseShape[j].x, meshGenerator.baseShape[j].y, i*meshGenerator.zOffset);
            }
        }

        //triangles
        triangles = new int[(count * rows * 6) - (count * 6)];

        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < count; j++)
            {
                triangles[i * count * 6 + j * 6    ] = i * count + j;
                triangles[i * count * 6 + j * 6 + 1] = (i + 1) * count + j;

                if(j==count-1)
                {
                    triangles[i * count * 6 + j * 6 + 2] = triangles[i * count * 6 + j * 6 + 3] = i * count;
                    triangles[i * count * 6 + j * 6 + 5] = (i + 1) * count ;

                }
                else
                {
                    triangles[i * count * 6 + j * 6 + 2] = triangles[i * count * 6 + j * 6 + 3] = i * count + j + 1;
                    triangles[i * count * 6 + j * 6 + 5] = (i + 1) * count + j + 1;
                }        
                triangles[i * count * 6 + j * 6 + 4] = (i + 1) * count + j;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
