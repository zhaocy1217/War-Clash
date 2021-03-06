﻿using Logic;
using Logic.Skill.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class U3DDisplayAction : IPool{
    public static Dictionary<Type, Type> LogicDisplayActions = new Dictionary<Type, Type>()
    {
        { typeof(PlayAnimationAction), typeof(U3DPlayAnimationAction) },
        { typeof(PlayFXAction), typeof(U3DPlayFXAction) },

    };

    public DisplayAction Action;

    public abstract void Execute(U3DSceneObject sender, U3DSceneObject receiver, object data);

    public void Reset() { }

    public abstract void Stop();

    public virtual void Update() { }
}

public class U3DDisplayActionManager
{
    private U3DSceneObject u3dCharacter;
    private List<U3DDisplayAction> displayActions = new List<U3DDisplayAction>();

    public U3DDisplayActionManager(U3DSceneObject u3dCharacter)
    {
        this.u3dCharacter = u3dCharacter;
    }
    public void Play(DisplayAction action)
    {
        Type targetType = U3DDisplayAction.LogicDisplayActions[action.GetType()];
        U3DDisplayAction u3dDisplayAction = Pool.SP.Get(targetType) as U3DDisplayAction;
        displayActions.Add(u3dDisplayAction);
        u3dDisplayAction.Action = action;
        u3dDisplayAction.Execute(u3dCharacter, null, null);
    }
    public void Stop(DisplayAction action)
    {
        for (int i = 0; i < displayActions.Count; i++)
        {
            if(displayActions[i].Action == action)
            {
                displayActions[i].Stop();
                Pool.SP.Recycle(displayActions[i]);
                displayActions.RemoveAt(i);
                i--;
            }
        }
    }
}
