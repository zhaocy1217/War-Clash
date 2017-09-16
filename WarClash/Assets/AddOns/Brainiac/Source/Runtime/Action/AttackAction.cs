using UnityEngine;
using System;
using Brainiac;
using Logic.LogicObject;
using Lockstep;
using Brainiac.Serialization;

[AddNodeMenu("Action/AttackAction")]
public class AttackAction : Brainiac.Action
{
    private Character _self;
    [BTProperty("SkillPath")]
    public MemoryVar skillid;

    private Character target = null;
    private bool _releaseSkillSuccess = false;
    public override void OnStart(AIAgent agent)
    {
        base.OnStart(agent);
        _self = agent.SceneObject as Character;
    }

    public override void OnReset()
    {
        base.OnReset();
        target = null;
    }

    protected override void OnEnter(AIAgent agent)
    {
        base.OnEnter(agent);
        if (target == null)
            target = agent.Blackboard.GetItem("Target") as Character;
        if (target != null)
        {
            _releaseSkillSuccess = _self.ReleaseSkill(skillid.AsInt.Value, target);
        }
        else
        {
            _releaseSkillSuccess = false;
        }
    }

    protected override void OnExit(AIAgent agent)
    {
        base.OnExit(agent);
    }

    protected override BehaviourNodeStatus OnExecute(AIAgent agent)
    {
        if (_releaseSkillSuccess)
        {
            if (_self.IsRunningSkill)
            {
                return BehaviourNodeStatus.Running;
            }
            else
            {
                return BehaviourNodeStatus.Success;
            }
        }
        else
        {
            return BehaviourNodeStatus.Failure;
        }
    }
}