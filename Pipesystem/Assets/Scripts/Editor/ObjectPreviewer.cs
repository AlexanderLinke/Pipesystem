using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ObjectPreviewer : EditorWindow {

    public GameObject gameObject;

    private Editor editor;

    [MenuItem("Window/Previewer")]
    static void ShowWindow()
    {
        GetWindow<ObjectPreviewer>("Previewer");
    }

    void OnGUI()
    {
        if(editor==null)
            editor = Editor.CreateEditor(gameObject);

        editor.OnPreviewGUI(GUILayoutUtility.GetRect(500,500), EditorStyles.whiteLabel);
    }
}
