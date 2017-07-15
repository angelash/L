using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ScenesManager
{
    private GameObject m_currentScene;
    public Transform MogoLogicPanel;

    public void LoadLoginScene()
    {
        UnloadScene();
        SoundManager.GameObjectPlaySound(Init.goDiver, "S1_se_city.ogg", true);
        LoadScene("MogoLogicUI.prefab");
        PreloadResource();
    }

    public void LoadBattleField(Action callback = null)
    {
        UnloadScene();
        SoundManager.SoundVolume = 0.2f;
        LoadScene("35002.prefab", delegate()
        {
            LoadLightMap("35002_Lightmap.exr", delegate()
            {
                LoadSkyBox("DawnDuskSkybox.mat", delegate()
                {
                    callback.SafeInvoke();
                });
            });
        });
    }

    public void LoadMapScene(int mapId)
    {
        //UnloadScene();
        SoundManager.GameObjectPlaySound(Init.goDiver, "S2_pvp.ogg", true);
        InitLogicPanel(mapId);
        //LoadScene("MogoLogicUI.prefab", () => { InitLogicPanel(mapId); });
        PreloadResource();

    }

    private void InitLogicPanel(int mapId)
    {
        MogoLogicPanel = m_currentScene.transform.FindChild("MogoLogicUIPanel");
        UIManager.I.ShowUI<MapUIMgr>(mapId);
    }

    public void PreloadResource(Action callback = null)
    {
        callback.SafeInvoke();

        //List<string> preloadList = new List<string>();
        //foreach (var item in SkillData.dataMap.Values)
        //{
        //    if (!string.IsNullOrEmpty(item.skillPrefab))
        //        preloadList.Add(item.skillPrefab);
        //    if (string.IsNullOrEmpty(item.explodeFx))
        //        continue;
        //    if (item.explodeFx.Contains("{0}"))
        //    {
        //        for (int i = 1; i <= 3; i++)
        //        {
        //            preloadList.Add(string.Format(item.explodeFx, i));
        //        }
        //    }
        //    else
        //    {
        //        preloadList.Add(item.explodeFx);
        //    }
        //}
        //preloadList.Add("SpeedUp.prefab");
        //preloadList.Add("SkillXuli.prefab");
        //preloadList.Add("BodyCharge.prefab");
        //preloadList.Add("RecoverBlood.prefab");
        //preloadList.Add("fx_buff_cure.prefab");
        //preloadList.Add("SkillXuli.prefab");

        ////LoggerHelper.Debug(preloadList.PackList());
        //AssetCacheMgr.GetResources(preloadList.ToArray(), (objs) =>
        //{
        //    callback.SafeInvoke();
        //});
    }

    public void LoadScene(string sceneName, Action callback = null)
    {
        AssetCacheMgr.GetNoCacheInstance(sceneName, (prefab, guid, obj) =>
        {
            m_currentScene = obj as GameObject;
            StaticBatchingUtility.Combine(m_currentScene);
            if (callback != null)
            {
                callback();
            }
        });
    }

    public void LoadLightMap(string lightMap, Action callback = null)
    {
        AssetCacheMgr.GetNoCacheResourceAutoRelease(lightMap, lm =>
        {
            var lmData = new LightmapData();
            lmData.lightmapFar = lm as Texture2D;
            LightmapSettings.lightmaps = new LightmapData[1] { lmData };
            if (callback != null)
            {
                callback();
            }
        });
    }

    public void LoadSkyBox(string skybox, Action callback = null)
    {
        AssetCacheMgr.GetNoCacheResourceAutoRelease(skybox, sb =>
        {
            RenderSettings.skybox = sb as Material;
            if (callback != null)
            {
                callback();
            }
        });
    }

    private void UnloadScene()
    {
        if (m_currentScene)
            GameObject.Destroy(m_currentScene);
    }
}
