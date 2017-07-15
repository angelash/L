using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit MogoSingleButton.
/// </summary>

[CustomEditor(typeof(MogoSingleButton))]
public class MogoSingleButtonInspector : Editor
{
    MogoSingleButton mMogoButton;

    /// <summary>
    /// Register an Undo command with the Unity editor.
    /// </summary>

    void RegisterUndo() { NGUIEditorTools.RegisterUndo("MogoSingleButton Change", mMogoButton); }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        mMogoButton = target as MogoSingleButton;

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        var isImage = EditorGUILayout.Toggle("isImage", mMogoButton.isImage, GUILayout.Width(250f));
        if (isImage != mMogoButton.isImage) { RegisterUndo(); mMogoButton.isImage = isImage; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var BGDown = EditorGUILayout.ObjectField("BGDown", mMogoButton.BGDown, typeof(GameObject)) as GameObject;
        if (BGDown != mMogoButton.BGDown) { RegisterUndo(); mMogoButton.BGDown = BGDown; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var BGUp = EditorGUILayout.ObjectField("BGUp", mMogoButton.BGUp, typeof(GameObject)) as GameObject;
        if (BGUp != mMogoButton.BGUp) { RegisterUndo(); mMogoButton.BGUp = BGUp; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var TextFG = EditorGUILayout.ObjectField("TextFG", mMogoButton.TextFG, typeof(GameObject)) as GameObject;
        if (TextFG != mMogoButton.TextFG) { RegisterUndo(); mMogoButton.TextFG = TextFG; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var m_lblName = EditorGUILayout.ObjectField("m_lblName", mMogoButton.m_lblName, typeof(UILabel)) as UILabel;
        if (m_lblName != mMogoButton.m_lblName) { RegisterUndo(); mMogoButton.m_lblName = m_lblName; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var buttonType = (ButtonClickSoundType)EditorGUILayout.EnumPopup("buttonType", mMogoButton.buttonType, GUILayout.Width(170f));
        if (buttonType != mMogoButton.buttonType) { RegisterUndo(); mMogoButton.buttonType = buttonType; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("一秒变样式", GUILayout.Width(75f));
        //if (GUILayout.Button("A横上选中", GUILayout.Width(100f)))
        //    ExportScenesManager.SetUIASingleButton(mMogoButton.gameObject, true);
        //if (GUILayout.Button("A横上普通", GUILayout.Width(100f)))
        //    ExportScenesManager.SetUIASingleButton(mMogoButton.gameObject, false);
        //GUILayout.EndHorizontal();
      
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("B竖左选中", GUILayout.Width(100f)))
        //    ExportScenesManager.SetUIBSingleButton(mMogoButton.gameObject, true);
        //if (GUILayout.Button("B竖左普通", GUILayout.Width(100f)))
        //    ExportScenesManager.SetUIBSingleButton(mMogoButton.gameObject, false);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("C横左选中", GUILayout.Width(100f)))
        //    ExportScenesManager.SetUICSingleButton(mMogoButton.gameObject, true);
        //if (GUILayout.Button("C横左普通", GUILayout.Width(100f)))
        //    ExportScenesManager.SetUICSingleButton(mMogoButton.gameObject, false);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("D子页签选中", GUILayout.Width(120f)))
        //    ExportScenesManager.SetUIDSingleButton(mMogoButton.gameObject, true);
        //if (GUILayout.Button("D子页签普通", GUILayout.Width(120f)))
        //    ExportScenesManager.SetUIDSingleButton(mMogoButton.gameObject, false);
        GUILayout.EndHorizontal();

    }
}