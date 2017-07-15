using Mogo.FSM;
using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mogo.Game
{
    public class EntityMyself : EntityPlayer
    {
        private void CreateCamera(Action callback)
        {
        }

        public override void CreateModel()
        {
        }

        public void Process()
        {
        }

        public override void OnDeath()
        {
            base.OnDeath();
        }

        public override void OnReborn()
        {
            base.OnReborn();
        }

        #region RPC

        public void StartMatch(int teamCount, int clientCount)
        {
            LoggerHelper.Debug(teamCount + " " + clientCount);

            MogoWorld.EnterMainEufloria(MapId);
        }

        public void AddBuilding(uint id, int starid, int buildingid, int finishPercentage)
        {
            //LoggerHelper.Error("rec AddBuilding id"+id);
            MogoWorld.m_dataMapManager.addBuildingToStar(id, starid, buildingid, finishPercentage);
        }

        public void AddSoldier(uint id, int starid, int soldier, int energy, int isSelfProduce)
        {
            MogoWorld.m_dataMapManager.addSoldierToStar(id, starid, soldier, energy, isSelfProduce);
        }

        public void OccupyStar(uint id, int starid)
        {
            MogoWorld.m_dataMapManager.StarBelong(id, starid);
        }

        public void RemoveSoldier(uint id, int starid, int soldier, int num,int type)
        {
            MogoWorld.m_dataMapManager.AttackSoldier(id, starid, soldier, num,type);
        }

        public void RemoveBuilding(uint id, int starid, int buildingPosition, int num)
        {
            MogoWorld.m_dataMapManager.AttackBuilding(id, starid, buildingPosition, num);
        }

        public void ArrayedSoldier(int starid, int fightingType)
        {
            MogoWorld.m_dataMapManager.ArrayedSoldier(starid, fightingType);
        }

        public void UpdateStarBelongTo(uint playerId, int starId)
        {
            MogoWorld.m_dataMapManager.UpdateStarBelongTo(playerId, starId);
        }

        //public void MoveSoldier(uint playerId, int startStarId, int endStarId, int isSelfProduce, List<int> soldierTypeList, int percent)
        public void MoveSoldier(LuaTable lt)
        {
            MogoWorld.m_dataMapManager.SendSoldier(lt);
        }

        public void AttackStar(uint attackPlayerId, uint starBelongPlayerId, int StarId, int energy)
        {
            MogoWorld.m_dataMapManager.PlayerAttackStar(attackPlayerId, starBelongPlayerId, StarId, energy);
        }

        public void GameOver(int sign)
        {
            if (MogoWorld.IsInGame)
            {
                MogoWorld.IsInGame = false;
                MogoWorld.m_dataMapManager.CulPlayerScroe();
                UIManager.I.ShowUI<BalanceUIMgr>();
            }
        }

        #endregion RPC
    }
}