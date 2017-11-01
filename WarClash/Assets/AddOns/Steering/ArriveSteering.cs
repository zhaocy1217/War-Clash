﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lockstep;
using Logic;


class ArriveSteering : BaseSteering
{
    public Vector3d Target;
    private bool _finish;
    public override void GetDesiredSteering(SteeringResult rst)
    {
        if (Vector3d.SqrDistance(Target, Self.Position)<FixedMath.One/2)
        {
            _finish = true;
        }
        if(_finish)
            return;
        Vector3d dir = Target - Self.Position;
        Vector3d desiredVelocity = dir.Normalize()*Self.Speed;
        var acc = (desiredVelocity - Self.Velocity).Div(LockFrameMgr.FixedFrameTime);
        rst.DesiredSteering = acc;
    }

    public ArriveSteering(ISteering self) : base(self)
    {
    }
}
