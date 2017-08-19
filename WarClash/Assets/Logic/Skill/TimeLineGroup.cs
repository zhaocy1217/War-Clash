﻿using Logic.LogicObject;
using Logic.Skill.Actions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Skill
{
    [Serializable]
    public class TimeLineGroup
    {
        [Display("名字")]
        [Newtonsoft.Json.JsonProperty]
        public string Name { get; private set; }

        [Display("ID")]
        [Newtonsoft.Json.JsonProperty]
        public int ID { get; private set; }
        public List<TimeLine> TimeLines { get; private set; }

        [Newtonsoft.Json.JsonProperty]
        public string path { get; private set; }
        public TimeLineGroup()
        {
            TimeLines = new List<TimeLine>();
        }

    }

    public class RuntimeTimeLineGroup : IPool
    {
        public TimeLineGroup sourceData;
        public RuntimeData m_RunningData;
        public List<RuntimeTimeLine> timelines = new List<RuntimeTimeLine>(); 
        //public Queue<object>    
        public Action finishAction;
        public bool isRunning;
        private int m_TimeLineCount
        {
            get { return sourceData.TimeLines.Count; }
        }
        private int m_CurrentTLIndex;

        public void Init(TimeLineGroup skill, RuntimeData srd, Action finishAction)
        {
            this.sourceData = skill;
            this.m_RunningData = srd;
            this.finishAction = finishAction;
            m_CurrentTLIndex = 0;
            isRunning = true;
            for (int i = 0; i < m_TimeLineCount; i++)
            {
                var rtl = Pool.SP.Get(typeof(RuntimeTimeLine)) as RuntimeTimeLine;
                timelines.Add(rtl);
                timelines[i].Init(sourceData.TimeLines[i], srd);
            }
        }
        public void Breath(float deltaTime)
        {
            var timeLine = timelines[m_CurrentTLIndex];
            if (timeLine.m_TimeLineStatus == TimeLineStatus.Finished)
            {
                if (m_CurrentTLIndex != m_TimeLineCount - 1)
                {
                    EnterNextTimeLine();
                    Breath(deltaTime);
                    return;
                }
            }
            else
            {
                deltaTime = timeLine.Breath(deltaTime);
                if (m_CurrentTLIndex == m_TimeLineCount - 1 && timeLine.m_TimeLineStatus == TimeLineStatus.Finished)
                {
                    OnFinish();
                }
                else
                {
                    if (m_CurrentTLIndex < m_TimeLineCount - 1 && deltaTime > 0)
                    {
                        EnterNextTimeLine();
                        Breath(deltaTime);
                    }
                }
            }
        }

        public virtual void OnFinish()
        {
            for (int i = 0; i < timelines.Count; i++)
            {
                var tl = timelines[i];
                for (int j = 0; j < tl.timeLine.BaseActions.Count; j++)
                {
                    var action = tl.timeLine.BaseActions[j] as DisplayAction;
                    if (action != null && action.stopCondition == StopCondition.SkillEnd)
                    {
                        Character so = null;
                        if (action.playTarget == PlayTarget.SENDER)
                        {
                            so = this.m_RunningData.sender as Character;
                        }else if (action.playTarget == PlayTarget.RECEIVER)
                        {
                            so = this.m_RunningData.receiver as Character;
                        }
                        so.EventGroup.FireEvent((int)Character.CharacterEvent.Stopdisplayaction, so, EventGroup.NewArg<EventSingleArgs<DisplayAction>, DisplayAction>(action as DisplayAction));
                    }
                }
            }
            if (finishAction != null)
            {
                finishAction.Invoke();
            }
        }
        public void EnterNextTimeLine()
        {
            Pool.SP.Recycle(timelines[m_CurrentTLIndex]);
            m_CurrentTLIndex++;
            timelines[m_CurrentTLIndex].Enter();
        }

        public void Reset()
        {
            timelines.Clear();
        }
    }
}
