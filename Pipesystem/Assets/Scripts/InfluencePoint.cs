using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluencePoint : MonoBehaviour
{
    public Pipesystem correspondingPipesystem;

    public bool isSelected;

    //gizmos
    public float gizmoSize = 1f;

    void OnDrawGizmos()
    {

        if (correspondingPipesystem != null && correspondingPipesystem.isLinked)
        {
            //if (!isSelected)
            //{
                Gizmos.color = correspondingPipesystem.gizmoColors[9];
                Gizmos.DrawWireSphere(transform.position, gizmoSize);

                Gizmos.color = correspondingPipesystem.gizmoColors[10];
                Gizmos.DrawSphere(transform.position, gizmoSize);
            //}
            //else
            //{
            //    Gizmos.color = correspondingPipesystem.gizmoColors[11];
            //    Gizmos.DrawWireSphere(transform.position, gizmoSize);

            //    Gizmos.color = correspondingPipesystem.gizmoColors[11];
            //    Gizmos.DrawSphere(transform.position, gizmoSize);
            //}
        }
    }
}
