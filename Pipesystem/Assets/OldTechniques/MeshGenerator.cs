using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public Vector3[] baseShape;
    public float zOffset;
    public bool drawBaseShape;

    public int rows;

    [SerializeField]
    private bool showGizmos;

    [SerializeField]
    private float gizmoRadius;

    [SerializeField]
    private bool drawLine;

    [SerializeField]
    private float gizmoLineRadius;

    [SerializeField]
    private Color32 shapeColor;

    [SerializeField]
    private Color32 lineColor;

    void OnDrawGizmos()
    {
        Gizmos.color = shapeColor;
        if (showGizmos && rows==1)
        {
            foreach(Vector3 point in baseShape)
            {
                Gizmos.DrawSphere(point, gizmoRadius);
            }
        }
        

        if (drawBaseShape)
        {
            for (int i = 0; i < baseShape.Length-1; i++)
            {
                Gizmos.DrawLine(baseShape[i], baseShape[i + 1]);
            }
            Gizmos.DrawLine(baseShape[baseShape.Length - 1], baseShape[0]);
        }

        if(drawLine)
        {
            Gizmos.color= lineColor;
            for(int i = 0; i < 19; i++)
            {
                Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y, transform.position.z + (i * zOffset)), new Vector3(transform.position.x, transform.position.y, transform.position.z + ((i + 1) * zOffset)));
            }
            for (int i = 0; i < 20; i++)
            {
                Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y, transform.position.z + (i * zOffset)), gizmoLineRadius);
            }
        }

    }
}
