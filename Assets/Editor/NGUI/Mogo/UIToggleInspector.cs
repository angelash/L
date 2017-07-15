using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit MogoSingleButton.
/// </summary>

[CustomEditor(typeof(UIToggle))]
public class UIToggleInspector : Editor
{
    UIToggle mMogoButton;

    /// <summary>
    /// Register an Undo command with the Unity editor.
    /// </summary>

    void RegisterUndo() { NGUIEditorTools.RegisterUndo("UIToggle Change", mMogoButton); }

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80f);
        mMogoButton = target as UIToggle;

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        var group = EditorGUILayout.IntField("group", mMogoButton.group, GUILayout.Width(250f));
        if (group != mMogoButton.group) { RegisterUndo(); mMogoButton.group = group; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var startsActive = EditorGUILayout.Toggle("startsActive", mMogoButton.startsActive, GUILayout.Width(250f));
        if (startsActive != mMogoButton.startsActive) { RegisterUndo(); mMogoButton.startsActive = startsActive; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var activeSprite = EditorGUILayout.ObjectField("activeSprite", mMogoButton.activeSprite, typeof(GameObject), GUILayout.Width(250f)) as GameObject;
        if (activeSprite != mMogoButton.activeSprite) { RegisterUndo(); mMogoButton.activeSprite = activeSprite; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var disableSprite = EditorGUILayout.ObjectField("disableSprite", mMogoButton.disableSprite, typeof(GameObject), GUILayout.Width(250f)) as GameObject;
        if (disableSprite != mMogoButton.disableSprite) { RegisterUndo(); mMogoButton.disableSprite = disableSprite; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var activeLabel = EditorGUILayout.ObjectField("activeLabel", mMogoButton.activeLabel, typeof(GameObject), GUILayout.Width(250f)) as GameObject;
        if (activeLabel != mMogoButton.activeLabel) { RegisterUndo(); mMogoButton.activeLabel = activeLabel; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var disableLabel = EditorGUILayout.ObjectField("disableLabel", mMogoButton.disableLabel, typeof(GameObject), GUILayout.Width(250f)) as GameObject;
        if (disableLabel != mMogoButton.disableLabel) { RegisterUndo(); mMogoButton.disableLabel = disableLabel; }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        //GUILayout.Label("一秒变样式", GUILayout.Width(75f));
        //if (GUILayout.Button("A选中", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIAUIToggle(mMogoButton.gameObject, true);
        //if (GUILayout.Button("A普通", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIAUIToggle(mMogoButton.gameObject, false);
        //GUILayout.EndHorizontal();
      
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("B选中", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIBUIToggle(mMogoButton.gameObject, true);
        //if (GUILayout.Button("B普通", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIBUIToggle(mMogoButton.gameObject, false);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("C选中", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUICUIToggle(mMogoButton.gameObject, true);
        //if (GUILayout.Button("C普通", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUICUIToggle(mMogoButton.gameObject, false);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("D选中", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIDUIToggle(mMogoButton.gameObject, true);
        //if (GUILayout.Button("D普通", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIDUIToggle(mMogoButton.gameObject, false);
        GUILayout.EndHorizontal();

    }
}