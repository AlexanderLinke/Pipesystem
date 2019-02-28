using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class SnapPoint : MonoBehaviour {

    public PipesystemManager manager;

    public ControlPoint connectedControlPoint;

    public bool isSelected;

    public List<PipeStyle> connectablePipestyles;

    public float gizmoSize = 1;

    public bool isAnchord;
    public Vector3 position;

    private Color32 hiddenColor = new Color32(0, 0, 0, 0);


    void OnDrawGizmos()
    {
        if(manager == null)
            manager = FindObjectOfType<PipesystemManager>();

        if (connectedControlPoint == null) 
        {
            if (manager != null)
            {
                if (connectablePipestyles.Contains(manager.activePipesystem.pipeStyle))
                {
                    if (isSelected)
                        Gizmos.color = manager.activePipesystem.gizmoColors[7];
                    else
                        Gizmos.color = manager.activePipesystem.gizmoColors[6];
                }
                else
                    Gizmos.color = manager.activePipesystem.gizmoColors[8];
            }
            else
                Gizmos.color = hiddenColor;

            Gizmos.DrawSphere(transform.position, gizmoSize);
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
        }
    }
}
