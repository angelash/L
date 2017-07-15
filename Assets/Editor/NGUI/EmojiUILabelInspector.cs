﻿//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Inspector class used to edit UILabels.
/// </summary>

[CustomEditor(typeof(EmojiUILabel))]
public class EmojiUILabelInspector : UIWidgetInspector
{
    EmojiUILabel mLabel;

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
        mLabel = mWidget as EmojiUILabel;

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
            EmojiUILabel.Effect effect = (EmojiUILabel.Effect)EditorGUILayout.EnumPopup("Effect", mLabel.effectStyle, GUILayout.Width(170f));
            if (effect != mLabel.effectStyle) { RegisterUndo(); mLabel.effectStyle = effect; }

            if (effect != EmojiUILabel.Effect.None)
            {
                Color c = EditorGUILayout.ColorField(mLabel.effectColor);
                if (mLabel.effectColor != c) { RegisterUndo(); mLabel.effectColor = c; }
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (mLabel.effectStyle != EmojiUILabel.Effect.None)
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

        return true;
    }
}