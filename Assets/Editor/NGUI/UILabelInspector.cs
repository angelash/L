//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(UILabel))]
public class UILabelInspector : UIWidgetInspector
{
    UILabel mLabel;

    /// <summary>
    /// Register an Undo command with the Unity editor.
    /// </summary>

    void RegisterUndo() { NGUIEditorTools.RegisterUndo("Label Change", mLabel); }

    /// <summary>
    /// Font selection callback.
    /// </summary>

    void OnSelectFont(MonoBehaviour obj)
    {
        if (mLabel != null)
        {
            NGUIEditorTools.RegisterUndo("Font Selection", mLabel);
            bool resize = (mLabel.font == null);
            mLabel.font = obj as UIFont;
            if (resize) mLabel.MakePixelPerfect();
        }
    }

    override protected bool DrawProperties()
    {
        mLabel = mWidget as UILabel;

        ComponentSelector.Draw<UIFont>(mLabel.font, OnSelectFont);
        if (mLabel.font == null) return false;

        GUI.skin.textArea.wordWrap = true;
        string text = string.IsNullOrEmpty(mLabel.text) ? "" : mLabel.text;
        text = EditorGUILayout.TextArea(mLabel.text, GUI.skin.textArea, GUILayout.Height(100f));
        if (!text.Equals(mLabel.text)) { RegisterUndo(); mLabel.text = text; }
        EditorGUILayout.LabelField("*Language Id代替text序列化");
        EditorGUILayout.LabelField("*固定内容直接把中文表id填到Language Id");

        GUILayout.BeginHorizontal();
        int languageId = EditorGUILayout.IntField("Language Id", mLabel.LanguageId, GUILayout.Width(220f));
        if (languageId != mLabel.LanguageId)
        {
            SystemSwitch.ReleaseMode = false;
            SystemSwitch.UseHmf = false;
            UILabel.GetContent = Mogo.GameData.LanguageData.GetContent;
            RegisterUndo(); mLabel.LanguageId = languageId;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            int len = EditorGUILayout.IntField("Line Width", mLabel.lineWidth, GUILayout.Width(120f));
            if (len != mLabel.lineWidth) { RegisterUndo(); mLabel.lineWidth = len; }

            int count = EditorGUILayout.IntField("Line Count", mLabel.maxLineCount, GUILayout.Width(120f));
            if (count != mLabel.maxLineCount) { RegisterUndo(); mLabel.maxLineCount = count; }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            int len = EditorGUILayout.IntField("Spacing X", mLabel.spacingX, GUILayout.Width(120f));
            if (len != mLabel.spacingX) { RegisterUndo(); mLabel.spacingX = len; }

            int count = EditorGUILayout.IntField("Spacing Y", mLabel.spacingY, GUILayout.Width(120f));
            if (count != mLabel.spacingY) { RegisterUndo(); mLabel.spacingY = count; }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        bool password = EditorGUILayout.Toggle("Password", mLabel.password, GUILayout.Width(120f));
        if (password != mLabel.password) { RegisterUndo(); mLabel.password = password; }

        bool encoding = EditorGUILayout.Toggle("Encoding", mLabel.supportEncoding, GUILayout.Width(100f));
        if (encoding != mLabel.supportEncoding) { RegisterUndo(); mLabel.supportEncoding = encoding; }

        bool tranlateReturn = EditorGUILayout.Toggle("TranslateReturn", mLabel.TranslateReturn, GUILayout.Width(120f));
        if (tranlateReturn != mLabel.TranslateReturn) { RegisterUndo(); mLabel.TranslateReturn = tranlateReturn; }

        GUILayout.EndHorizontal();

        if (encoding && mLabel.font.hasSymbols)
        {
            UIFont.SymbolStyle sym = (UIFont.SymbolStyle)EditorGUILayout.EnumPopup("Symbols", mLabel.symbolStyle, GUILayout.Width(170f));
            if (sym != mLabel.symbolStyle) { RegisterUndo(); mLabel.symbolStyle = sym; }
        }

        GUILayout.BeginHorizontal();
        {
            UILabel.Effect effect = (UILabel.Effect)EditorGUILayout.EnumPopup("Effect", mLabel.effectStyle, GUILayout.Width(170f));
            if (effect != mLabel.effectStyle) { RegisterUndo(); mLabel.effectStyle = effect; }

            if (effect != UILabel.Effect.None)
            {
                Color c = EditorGUILayout.ColorField(mLabel.effectColor);
                if (mLabel.effectColor != c) { RegisterUndo(); mLabel.effectColor = c; }
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (mLabel.effectStyle != UILabel.Effect.None)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Distance", GUILayout.Width(70f));
            Vector2 offset = EditorGUILayout.Vector2Field("", mLabel.effectDistance);
            GUILayout.Space(20f);

            if (offset != mLabel.effectDistance)
            {
                RegisterUndo();
                mLabel.effectDistance = offset;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            int len = EditorGUILayout.IntField("TextChangeWidth", mLabel.ChangeTextWidth, GUILayout.Width(120f));
            if (len != mLabel.ChangeTextWidth) { RegisterUndo(); mLabel.ChangeTextWidth = len; }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("一秒变样式", GUILayout.Width(75f));
        //if (GUILayout.Button("36标题", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIHeadText(mLabel.gameObject);
        //if (GUILayout.Button("30标题", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUI30TitleText(mLabel.gameObject);
        //if (GUILayout.Button("28标题", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUI28TitleText(mLabel.gameObject);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("26标题", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUI26TitleText(mLabel.gameObject);
        //if (GUILayout.Button("通用", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUICommonText(mLabel.gameObject);
        //if (GUILayout.Button("A标签", GUILayout.Width(50f)))
        //    ExportScenesManager.SetUIAHorizontalUpButtonText(mLabel.gameObject);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("绿色", GUILayout.Width(60f)))
        //    ExportScenesManager.SetUIGreenText(mLabel.gameObject);
        //if (GUILayout.Button("红色", GUILayout.Width(60f)))
        //    ExportScenesManager.SetUIRedText(mLabel.gameObject);
        //if (GUILayout.Button("icon", GUILayout.Width(60f)))
        //    ExportScenesManager.SetUIIconNumText(mLabel.gameObject);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("底纹按钮", GUILayout.Width(60f)))
        //    ExportScenesManager.SetUINormalButtonText(mLabel.gameObject);
        //if (GUILayout.Button("主右上", GUILayout.Width(60f)))
        //    ExportScenesManager.SetUIMainRightUpText(mLabel.gameObject);
        //if (GUILayout.Button("主右", GUILayout.Width(60f)))
        //    ExportScenesManager.SetUIMainRightText(mLabel.gameObject);
        //GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(75f));
        //if (GUILayout.Button("底纹按钮", GUILayout.Width(60f)))
        //    ExportScenesManager.SetUINormalButtonText(mLabel.gameObject);
        //if (GUILayout.Button("双字加空格", GUILayout.Width(75f)))
        //{
        //    RegisterUndo();
        //    mLabel.ChangeTextWidth = 100;
        //    mLabel.text = mLabel.text;
        //}
        GUILayout.EndHorizontal();

        return true;
    }
}