//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// If you don't have or don't wish to create an atlas, you can simply use this script to draw a texture.
/// Keep in mind though that this will create an extra draw call with each UITexture present, so it's
/// best to use it only for backgrounds or temporary visible widgets.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/MaskTexture")]
public class UIMaskTexture : UITexture
{
    [HideInInspector][SerializeField] protected Texture mMaskTexture;

    public override Shader shader
    {
        get
        {
            if (mShader == null)
            {
                Material mat = material;
                if (mat != null) mShader = mat.shader;
                if (mShader == null) mShader = Shader.Find("Mogo/UIETC");
            }
            return mShader;
        }
        set
        {
            if (mShader != value)
            {
                mShader = value;
                Material mat = material;
                if (mat != null) mat.shader = value;
                mPMA = -1;
            }
        }
    }


	public override Material material
	{
		get
		{
			if (!mCreatingMat && mMat == null)
			{
				mCreatingMat = true;

                if (mainTexture != null)
                {
                    if (mShader == null) mShader = Shader.Find("Mogo/UIETC");
                    if (Application.isEditor)
                    {
                        mShader = Shader.Find(mShader.name);
                    }

                    mDynamicMat = new Material(mShader);
                    mDynamicMat.hideFlags = HideFlags.DontSave;
                    mDynamicMat.mainTexture = mainTexture;
                    mMat.SetTexture("_AlphaTex", mMaskTexture);
                    base.material = mDynamicMat;
                    mPMA = 0;
                }
                mCreatingMat = false;
			}
			return mMat;
		}
		set
		{
			if (mDynamicMat != value && mDynamicMat != null)
			{
				NGUITools.Destroy(mDynamicMat);
				mDynamicMat = null;
			}
			base.material = value;
			mPMA = -1;
		}
	}

    public override Texture mainTexture
    {
        get
        {
            return (mTexture != null) ? mTexture : base.mainTexture;
        }
        set
        {
            if (mPanel != null && mMat != null) mPanel.RemoveWidget(this);

            if (mMat == null)
            {
                if (Application.isEditor)
                {
                    shader = Shader.Find(shader.name);
                }
                mDynamicMat = new Material(shader);
                mDynamicMat.hideFlags = HideFlags.DontSave;
                mMat = mDynamicMat;
            }

            mPanel = null;
            mTex = value;
            mTexture = value;
            mMat.mainTexture = value;
            mMat.SetTexture("_AlphaTex", mMaskTexture);

            if (enabled) CreatePanel();
        }
    }

    public Texture maskTexture
    {
        get
        {
            return mMaskTexture;
        }
        set
        {
            if (mPanel != null && mMat != null) mPanel.RemoveWidget(this);

            if (mMat == null)
            {
                if (Application.isEditor)
                {
                    shader = Shader.Find(shader.name);
                }
                mDynamicMat = new Material(shader);
                mDynamicMat.hideFlags = HideFlags.DontSave;
                mMat = mDynamicMat;
            }

            mPanel = null;
            mMaskTexture = value;
            mMat.mainTexture = mTexture;
            mMat.SetTexture("_AlphaTex", mMaskTexture);

            if (enabled) CreatePanel();
        }
    }
}
