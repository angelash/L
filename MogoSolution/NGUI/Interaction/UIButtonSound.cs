//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using Mogo.Util;

/// <summary>
/// Plays the specified sound.
/// </summary>

[AddComponentMenu("NGUI/Interaction/Button Sound")]
public class UIButtonSound : MonoBehaviour
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
	}

	public AudioClip audioClip;
	public Trigger trigger = Trigger.OnClick;
	public float volume = 1f;
	public float pitch = 1f;
    /// <summary>
    /// 按钮点击音效类型
    /// 1：弱
    /// 2：中
    /// 3：强
    /// 4：打开
    /// 5：关闭
    /// </summary>
    public ButtonClickSoundType buttonType = ButtonClickSoundType.Weak;

	void OnHover (bool isOver)
	{
		if (enabled && ((isOver && trigger == Trigger.OnMouseOver) || (!isOver && trigger == Trigger.OnMouseOut)))
		{
			NGUITools.PlaySound(audioClip, volume, pitch);
		}
	}

	void OnPress (bool isPressed)
	{
		if (enabled && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
		{
			NGUITools.PlaySound(audioClip, volume, pitch);
		}
	}

	void OnClick ()
	{
		if (enabled && trigger == Trigger.OnClick)
		{
            EventDispatcher.TriggerEvent("Click_Button", buttonType);
			NGUITools.PlaySound(audioClip, volume, pitch);
		}
	}
}