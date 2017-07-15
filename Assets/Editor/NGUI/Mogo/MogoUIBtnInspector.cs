using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit MogoUIBtn.
/// </summary>

[CustomEditor(typeof(MogoUIBtn))]
public class MogoUIBtnInspector : Editor
{
    MogoUIBtn mMogoButton;

    /// <summary>
    /// Register an Undo command with the Unity editor.
    /// </summary>

    void RegisterUndo() { NGUIEditorTools.RegisterUndo("MogoUIBtn Change", mMogoButton); }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        mMogoButton = target as MogoUIBtn;

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        var m_imgNormal = EditorGUILayout.ObjectField("m_imgNormal", mMogoButton.m_imgNormal, typeof(UISprite), GUILayout.Width(250f)) as UISprite;
        if (m_imgNormal != mMogoButton.m_imgNormal) { RegisterUndo(); mMogoButton.m_imgNormal = m_imgNormal; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var m_imgPressed = EditorGUILayout.ObjectField("m_imgPressed", mMogoButton.m_imgPressed, typeof(UISprite), GUILayout.Width(250f)) as UISprite;
        if (m_imgPressed != mMogoButton.m_imgPressed) { RegisterUndo(); mMogoButton.m_imgPressed = m_imgPressed; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var m_lblText = EditorGUILayout.ObjectField("m_lblText", mMogoButton.m_lblText, typeof(UILabel), GUILayout.Width(250f)) as UILabel;
        if (m_lblText != mMogoButton.m_lblText) { RegisterUndo(); mMogoButton.m_lblText = m_lblText; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var m_boxCollider = EditorGUILayout.ObjectField("m_boxCollider", mMogoButton.m_boxCollider, typeof(BoxCollider), GUILayout.Width(250f)) as BoxCollider;
        if (m_boxCollider != mMogoButton.m_boxCollider) { RegisterUndo(); mMogoButton.m_boxCollider = m_boxCollider; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var buttonType = (ButtonClickSoundType)EditorGUILayout.EnumPopup("buttonType", mMogoButton.buttonType, GUILayout.Width(170f));
        if (buttonType != mMogoButton.buttonType) { RegisterUndo(); mMogoButton.buttonType = buttonType; }
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