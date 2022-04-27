using System;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    [Serializable]
    public class MemberInfo
    {
        public MemberInfo(GameObject go, Controller c)
        {
            this.go = go;
            this.controller = c;
        }

        public GameObject go;
        public Controller controller;

        public int battle_idx;
        public int follow_idx;
    }

    public enum TEAM_TYPE
    {
        PLAYER,
        MONSTER
    }
    public List<MemberInfo> members = new List<MemberInfo>(10);

    public TEAM_TYPE teamType = TEAM_TYPE.MONSTER;

    public static T SpawnTeam<T>(string team_name) where T : Team
    {
        var go = GameObject.Find(team_name);
        T comp = null;
        if (go != null)
        {
            comp = go.GetComponent<T>();
        }
        else
        {
            go = new GameObject(team_name);
            comp = go.AddComponent<T>();
        }

        if (team_name == "monster_team")
            GameInfo.instance.SetMonsterTeam(comp);
        else if (team_name == "player_team")
            GameInfo.instance.SetPlayerTeam(comp);

        return comp;
    }

    public void AddMember(GameObject go, Controller c)
    {
        foreach (var v in members)
        {
            if (v.go == go)
            {
                v.controller = c;
                return;
            }
        }

        members.Add(new MemberInfo(go, c));

        //Sorting by vnum
        members.Sort((a, b) =>
        {
            if (a.controller.PAWN.vnum < b.controller.PAWN.vnum)
                return -1;
            else if (a.controller.PAWN.vnum > b.controller.PAWN.vnum)
                return 1;

            return 0;
        });

        //Sorting by job type
        List<MemberInfo> sorted_members = new List<MemberInfo>(members);
        sorted_members.Sort((a, b) =>
        {
            int a_idx = GetBattleSlotWeight(a.controller.PAWN.jobType);
            int b_idx = GetBattleSlotWeight(b.controller.PAWN.jobType);

            if (a_idx < b_idx)
                return -1;
            else if (a_idx > b_idx)
                return 1;

            return 0;
        });

        for (int idx = 0; idx < sorted_members.Count; idx++)
        {
            var sorted_member = sorted_members[idx];
            sorted_member.battle_idx = idx;
        }

        for (int idx = 0; idx < members.Count; idx++)
        {
            var member = members[idx];
            member.follow_idx = idx;
        }
    }

    public int GetBattleSlotWeight(Pawn.JobType jobType)
    {
        int index_for_job = 0;
        switch (jobType)
        {
            case Pawn.JobType.Tanker:
                index_for_job = 0;
                break;
            case Pawn.JobType.Dealer:
                index_for_job = 1;
                break;
            case Pawn.JobType.Ranger:
                index_for_job = 2;
                break;
            case Pawn.JobType.Healer:
                index_for_job = 3;
                break;
            case Pawn.JobType.Supporter:
                index_for_job = 4;
                break;
        }

        return index_for_job;
    }
}
