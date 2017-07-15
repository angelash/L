using Mogo.GameData;
using Mogo.Util;
using UnityEngine;

public class UnitBase
{
    public int UnitId;
    public GameObject UnitGO;
    public Transform UnitParent;
    public float PositionX;
    public float PositionY;

    //当前能量
    public int CurEnergy;

    //当前阵营
    public GroupData MyGroup;//

    private bool IsShow = true;

    protected virtual string prefabName
    {
        get { return "StarUI.prefab"; }
    }

    public void InitUnit()
    {
        if (UnitGO == null)
        {
            AssetCacheMgr.GetUIInstance(prefabName, (prefab, guid, go) =>
            {
                UnitGO = go as GameObject;
                InitUnitData();
                ShowUnit(IsShow);
                if (MyGroup != null)
                    SetGroupData();
            });
        }
        else
        {
            InitUnitData();
            ShowUnit(IsShow);
        }
    }

    public void SetGroup(GroupData data)
    {
        MyGroup = data;
        if (UnitGO != null && MyGroup != null)
            SetGroupData();
    }

    protected virtual void InitUnitData()
    {
    }

    protected virtual void SetGroupData()
    {
    }

    public void ClearUnit()
    {
        if (UnitGO != null)
        {
            UnitGO.transform.parent = null;
            UnitGO.transform.localPosition = new Vector3(5000, 1000, 1);
            UnitGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        UnitId = -1;
        CurEnergy = 0;
        ClearUnitData();
    }

    protected virtual void ClearUnitData()
    {
    }

    public void ShowUnit(bool _show)
    {
        IsShow = _show;
        if (UnitGO != null)
        {
            if (_show)
            {
                UnitGO.layer = (int)LayerMaskMofloria.Default;
            }
            else
            {
                UnitGO.layer = (int)LayerMaskMofloria.Monster;
            }
        }
    }

    public enum LayerMaskMofloria
    {
        Default = 0,
        Monster = 11,
    }
}