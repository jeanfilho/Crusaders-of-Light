﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "move_pattern_idle", menuName = "Combat/AI/MovPatIdle", order = 2)]
public class MovPatIdle : MovePattern {

    public override void UpdateMovePattern(PhysicsController PhysCont, Character Self, Character TargetCharacter)
    {
        PhysCont.SetVelRot(Vector3.zero, Vector3.zero);
    }

}
