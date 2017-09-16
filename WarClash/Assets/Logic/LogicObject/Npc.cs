﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Brainiac;
using Config;
using Logic.Config;
using UnityEngine;

namespace Logic.LogicObject
{
    public class Npc : Character
    {
        public ArmyConf Conf;
        internal override void OnInit(CreateInfo createInfo)
        {
            base.OnInit(createInfo);
            var info  = createInfo as NpcCreateInfo;
            Conf = ConfigMap<ArmyConf>.Get(info.NpcId);
#if UNITY_EDITOR
            var bt = UnityEditor.AssetDatabase.LoadAssetAtPath<BTAsset>("Assets/RequiredResources/BT/" + Conf.BT);
            OnBtLoad(Conf.BT, bt);
#else
            Resource.LoadAsset(Conf.BT, OnBtLoad);
#endif
        }
        internal override void ListenEvents()
        {
            base.ListenEvents();
        }

        internal override void OnFixedUpdate(long deltaTime)
        {
            base.OnFixedUpdate(deltaTime);
            if (!IsDeath() && AiAgent!=null)
            {
                AiAgent.Tick();
            }
        }

        internal override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
          
        }

        private void OnBtLoad(string name, Object obj)
        {
            BTAsset bt = Object.Instantiate(obj) as BTAsset;
            AiAgent = new AIAgent(this, bt);
            AiAgent.Start();
        }
    }
}
