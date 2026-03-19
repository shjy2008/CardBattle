using Assets.Scripts.logics;
using Assets.Scripts.utility.CommonInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBasicController : IInit, IUpdate, IDestroy
{
    protected RoundType roundType;

    public BattleBasicController()
    {

    }

    public void SetRoundType(RoundType _roundType) { roundType = _roundType; }
    public RoundType GetRoundType() { return roundType; }

    public virtual void Init()
    {
        throw new System.NotImplementedException();
    }

    public virtual void Destroy()
    {
        throw new System.NotImplementedException();
    }

    public virtual void Update()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnRoundStart()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnRoundEnd()
    {
        throw new System.NotImplementedException();
    }
}
