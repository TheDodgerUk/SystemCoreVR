using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MonitorTrainer.MonitorTrainerConsts;

namespace MonitorTrainer
{
    public class MultiPlayerData
    {

        public List<string> m_UnlockedItems = new List<string>();
        public List<VrInteraction> m_VrInteractions = new List<VrInteraction>();
        public List<ContentMaterialChangeMetaData> m_ColourChangeDataPrimary = new List<ContentMaterialChangeMetaData>();
        public List<ContentMaterialChangeMetaData> m_ColourChangeDataSecondary = new List<ContentMaterialChangeMetaData>();
        
        public List<LocalPlayerData.SaveDataClass.SendData> m_MultiPlayerPrefsSelected = new List<LocalPlayerData.SaveDataClass.SendData>();




        public MultiPlayerData()
        {
            ClearAll();
        }
        public void ClearAll()
        {
            m_MultiPlayerPrefsSelected.Clear();
        }


        private void CollectVrInteractionsPlayArea(MonitorTrainerConsts.PlayersEnum player)
        {
            var Interative_Player = Core.Scene.GetSpawnedVrInteractionsConatainingScene($"Interactive_{player}", "PlayerSpace");
            var Static_Player = Core.Scene.GetSpawnedVrInteractionsConatainingScene($"Static_{player}", "PlayerSpace");

            List <VrInteraction> allVrInteraction = Interative_Player;
            allVrInteraction.AddRange(Static_Player);
        }

        private void CollectVrInteractionsMenu()
        {
            var Interative_Player = Core.Scene.GetSpawnedVrInteractionsConatainingScene($"Interactive_{MonitorTrainerConsts.PlayersEnum.Player1}", "PlayerSpace");
            var Static_Player = Core.Scene.GetSpawnedVrInteractionsConatainingScene($"Static_{MonitorTrainerConsts.PlayersEnum.Player1}", "PlayerSpace");

            List<VrInteraction> allVrInteraction = Interative_Player;
            allVrInteraction.AddRange(Static_Player);
        }


    }
}
