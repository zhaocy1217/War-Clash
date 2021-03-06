﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lockstep;


namespace Logic.Map
{
    [Serializable]
    public abstract class MapItem
    {
        public int Id;
        public Vector3d Position;
        public Vector3d Forward;
        public virtual string GetResPath()
        {
            return string.Empty;
        }
    }
    [Serializable]
    public class MapBuildingItem : MapItem
    {
        public int BuildingId;
        public override string GetResPath()
        {
            var conf = BuildingConf.Get(BuildingId);
            return conf.ResPath;
        }
    }

}
