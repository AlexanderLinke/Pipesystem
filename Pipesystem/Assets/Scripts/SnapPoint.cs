using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SnapPoint : MonoBehaviour {

    public ControlPoint connectedControlPoint;

    public bool isSelected;

    public List<PipeStyle> connectablePipestyles;

    public float gizmoSize = 1;

    public bool isAnchord;
    public Vector3 position;

    private Color32 hiddenColor = new Color32(0, 0, 0, 0);


    void OnDrawGizmos()
    {
        if(connectedControlPoint == null) 
        {
            if (PipesystemManager.activePipesystem != null)
            {
                if (connectablePipestyles.Contains(PipesystemManager.activePipesystem.pipeStyle))
                {
                    if (isSelected)
                        Gizmos.color = PipesystemManager.activePipesystem.gizmoColors[7];
                    else
                        Gizmos.color = PipesystemManager.activePipesystem.gizmoColors[6];
                }
                else
                    Gizmos.color = PipesystemManager.activePipesystem.gizmoColors[8];
            }
            else
                Gizmos.color = hiddenColor;

            Gizmos.DrawSphere(transform.position, gizmoSize);
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
        }
    }
}
