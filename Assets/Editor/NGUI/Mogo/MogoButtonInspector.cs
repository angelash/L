using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit MogoButtons.
/// </summary>

[CustomEditor(typeof(MogoButton))]
public class MogoButtonInspector : Editor
{
    MogoButton mMogoButton;

    /// <summary>
    /// Register an Undo command with the Unity editor.
    /// </summary>

    void RegisterUndo() { NGUIEditorTools.RegisterUndo("MogoButton Change", mMogoButton); }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        mMogoButton = target as MogoButton;

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        bool isDragCancel = EditorGUILayout.Toggle("IsDragCancel", mMogoButton.IsDragCancel, GUILayout.Width(100f));
        if (isDragCancel != mMogoButton.IsDragCancel) { RegisterUndo(); mMogoButton.IsDragCancel = isDragCancel; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("一秒变样式", GUILayout.Width(75f));
        //if (GUILayout.Button("底纹", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUINormalButton(mMogoButton.gameObject);
        //if (GUILayout.Button("确定", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIOKButton(mMogoButton.gameObject);
        //if (GUILayout.Button("确定(小)", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIOKSmallButton(mMogoButton.gameObject);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("取消", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUICancelButton(mMogoButton.gameObject);
        //if (GUILayout.Button("关闭", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUICloseButton(mMogoButton.gameObject);
        //if (GUILayout.Button("充值", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIChargeButton(mMogoButton.gameObject);
        GUILayout.EndHorizontal();
    }
}