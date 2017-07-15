//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's color.
/// </summary>

[AddComponentMenu("NGUI/Tween/GroupColor")]
public class TweenGroupColor : UITweener
{
	public Color from = Color.white;
	public Color to = Color.white;


    public BetterList<Color> mColors = new BetterList<Color>();


	Transform mTrans;
	UIWidget mWidget;
	Material mMat;
	Light mLight;

	/// <summary>
	/// Current color.
	/// </summary>

	public Color color
	{
		get
		{
			if (mWidget != null) return mWidget.color;
			if (mLight != null) return mLight.color;
			if (mMat != null) return mMat.color;
			return Color.black;
		}
		set
		{
			if (mWidget != null) mWidget.color = value;
			if (mMat != null) mMat.color = value;

			if (mLight != null)
			{
				mLight.color = value;
				mLight.enabled = (value.r + value.g + value.b) > 0.01f;
			}
		}
	}

	/// <summary>
	/// Find all needed components.
	/// </summary>

	void Awake ()
	{
		mWidget = GetComponentInChildren<UIWidget>();
		Renderer ren = renderer;
		if (ren != null) mMat = ren.material;
		mLight = light;
	}

	/// <summary>
	/// Interpolate and update the color.
	/// </summary>

	override protected void OnUpdate(float factor, bool isFinished) { 

        //查看当前属于第几个时间片
        int count = mColors.size;
        int iSeq = (int)(count * factor)%count;
        //Debug.Log("iSeq = " + iSeq.ToString() + ", " + (count * factor).ToString());
        from = mColors.buffer[iSeq];
        to = mColors.buffer[(iSeq + 1) % count];
        /*
        if (to.r == 0 && to.b == 0 && to.g == 0)
        {
            Debug.Log("iSeq = " + iSeq.ToString() + ", " + (count * factor).ToString()+", to="+to.ToString());
        }
         * */
        color = Color.Lerp(from, to, factor); 
    }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

    static public TweenGroupColor Begin(GameObject go, float duration, Color color)
	{
        TweenGroupColor comp = UITweener.Begin<TweenGroupColor>(go, duration);
		comp.from = comp.color;
		comp.to = color;

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}
}