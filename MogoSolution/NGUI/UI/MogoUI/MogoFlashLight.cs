using UnityEngine;
using System.Collections;
using Mogo.Util;

public class MogoFlashLight : MonoBehaviour
{
    private UITexture tex;
    float m_fOffset = -1;
    public int m_fSpeed = 2;
    bool m_bStartFlashing = true;

    public string MainTexName;
    public string MainAlphaTexName;
    public string LightTexName;

    public Vector3 MainTexScale;

    void Awake()
    {
        tex = GetComponent<UITexture>();
        SetTheTextureOffset(m_fOffset);

    }

    public void SetTheTextureOffset(float offset)
    {
        if (tex && tex.material)
            tex.material.SetTextureOffset("_LightTex", new Vector2(offset, 0));
    }

    public void SetLightFactor(float factor)
    {
        if (tex && tex.material)
            tex.material.SetFloat("_LightFactor", factor);
    }

    void OnEnable()
    {
        AssetCacheMgr.GetResourceAutoRelease(MainTexName, (obj) =>
        {
            AssetCacheMgr.GetResourceAutoRelease(MainAlphaTexName, (obj1) =>
            {
                AssetCacheMgr.GetResourceAutoRelease(LightTexName, (obj2) =>
                {
                    if (tex && tex.material)
                    {
                        tex.material.SetTexture("_LightTex", (Texture)obj2);
                        tex.material.SetTexture("_AlphaTex", (Texture)obj1);
                        tex.material.SetTexture("_MainTex", (Texture)obj);
                        tex.transform.localScale = MainTexScale;
                        tex.material.SetFloat("_AlphaFactor", 1);
                    }
                });
            });


        });





        if (!SystemSwitch.DestroyResource)
            return;

        if (MainTexName == "" || LightTexName == "")
            return;

        if (tex.material.GetTexture("_MainTex") == null)
        {
            AssetCacheMgr.GetResourceAutoRelease(MainTexName, (obj) =>
            {
                if (!tex)
                    return;
                tex.material.SetTexture("_MainTex", (Texture)obj);
            });
        }

        if (tex.material.GetTexture("_LightTex") == null)
        {
            AssetCacheMgr.GetResourceAutoRelease(LightTexName, (obj) =>
            {
                if (!tex)
                    return;
                tex.material.SetTexture("_LightTex", (Texture)obj);
            });
        }


    }

    void OnDisable()
    {
        if (tex && tex.material)
        {
            tex.material.SetTexture("_MainTex", null);
            tex.material.SetTexture("_AlphaTex", null);
            tex.material.SetTexture("_LightTex", null);
            tex.material.SetFloat("_AlphaFactor", 0);
            tex.material.SetFloat("_LightFactor", 0);
            tex.material.SetTextureOffset("_LightTex", Vector2.zero);
        }

        //if (!SystemSwitch.DestroyAllUI)
        //    return;

        //不能在编辑器下生效以下代码，否则会将资源文件修改了
        if (Application.isEditor)
            return;

        if (MainTexName == "" || LightTexName == "")
            return;


        AssetCacheMgr.ReleaseResourceImmediate(MainTexName);
        AssetCacheMgr.ReleaseResourceImmediate(LightTexName);
        AssetCacheMgr.ReleaseResourceImmediate(MainAlphaTexName);
    }
    void Update()
    {
        if (m_bStartFlashing)
        {
            SetLightFactor(1);
            SetTheTextureOffset(m_fOffset += m_fSpeed * 0.01f);

            if (m_fOffset >= 1f)
            {
                m_fOffset = -1f;

                SetLightFactor(0);

                m_bStartFlashing = false;

                TimerHeap.AddTimer(3000, 0, () => { m_bStartFlashing = true; });
            }
        }
    }
}
