using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : Controller
{
    NavMeshPath _path = null;
    public float _UIAddDamagePosY = 80;

    protected virtual void Initialize()
    {
        InitTeam("player_team");
    }

    protected void InitTeam(string team_name)
    {
        myTeam.AddMember(gameObject, this);
    }
}
