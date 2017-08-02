﻿
using System;
using System.Collections.Generic;
using System.Text;
using Logic.LogicObject;
using Logic.Skill.Actions;
using System.Runtime.Serialization;

namespace Logic.Skill.Actions
{
    public enum StopCondition
    {
        SkillEnd,
        TimelineEnd,
    }
    public enum Range
    {
        RECT = 1,
        SPHERE = 2,
        FAN = 4
    }

    public enum PlayTarget
    {
        SENDER,
        RECEIVER
    }
    [Serializable]
    public class DataBind<T>
    {
        public T value {
            get
            {
                if (needDataBind)
                {
                    return default(T);
                }
                else
                {
                    return value_SetDirectly; 
                }
            }
        }
        [Newtonsoft.Json.JsonProperty]
        private T value_SetDirectly { get;  set; }
        [Newtonsoft.Json.JsonProperty]
        public bool needDataBind { get; private set; }
        [Newtonsoft.Json.JsonProperty]
        public string bindFrom { get; private set; }
    }

    [Serializable]
    public abstract class BaseAction
    {
        [Display("执行帧数", "执行帧数", UIControlType.Range)]
        [Newtonsoft.Json.JsonProperty]
        public int ExecuteFrameIndex { get; private set; }

        
        public virtual void Execute(SceneObject sender, SceneObject reciever, object data)
        {
            string str = reciever == null ? "" : reciever.ToString();
            UnityEngine.Debug.LogError(sender.ToString()+"  "+str+"  "+GetType()+"  "+ LogicCore.SP.realFixedFrame);
        }
    }
   
    [Serializable]
    public class DisplayAction : BaseAction
    {
        [Display("停止条件", UIControlType.Default)]
        [Newtonsoft.Json.JsonProperty]
        public StopCondition stopCondition { get; private set; }

        [Display("播放目标", UIControlType.Default)]
        [Newtonsoft.Json.JsonProperty]
        public PlayTarget playTarget { get; private set; }

        public override void Execute(SceneObject sender, SceneObject reciever, object data)
        {
            Character so = null;
            if (playTarget == PlayTarget.SENDER)
            {
                so = sender as Character;
            }
            else if (playTarget == PlayTarget.RECEIVER)
            {
                so = reciever as Character;
            }
            so.EventGroup.FireEvent((int) Character.CharacterEvent.EXECUTEDISPLAYACTION, sender,
                EventGroup.NewArg<EventSingleArgs<DisplayAction>, DisplayAction>(this));
            base.Execute(sender, reciever, data);
        }
    }
}
