using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Controller : MonoBehaviour
{
    //Æù ¾ò¾î¿À±â
    private Pawn _pawn = null;
    public virtual Pawn PAWN
    {
        get
        {
            if (_pawn == null)
                _pawn = GetComponent<Pawn>();

            return _pawn;
        }
    }
    //ÆÀ ¸â¹ö °ü·Ã
    protected Team myTeam
    {
        get
        {
            if (tag == "Player" || tag == "PartyMember")
            {
                var player_team = GameInfo.instance.FindPlayerTeam();
                if (player_team == null)
                {
                    player_team = Team.SpawnTeam<Team>("player_team");
                    player_team.teamType = Team.TEAM_TYPE.PLAYER;
                }
                return player_team;
            }
            else
            {
                var monster_team = GameInfo.instance.FindEnemyTeam();
                if (monster_team == null)
                {
                    monster_team = Team.SpawnTeam<Team>("monster_team");
                    monster_team.teamType = Team.TEAM_TYPE.MONSTER;
                }
                return monster_team;
            }
        }
    }

    protected virtual void Start() { }

    public virtual void OnDeadEnemy(GameObject dead_obj) { }
    public virtual void OnDeadMember(GameObject dead_obj) { }
    public virtual void OnAllDeadEnemy() { }

    public Team GetTeam()
    {
        return myTeam;
    }

    public bool IsPlayerTeam
    {
        get
        {
            if (myTeam != null && myTeam.name == "player_team")
            {
                return true;
            }

            return false;
        }
    }
}