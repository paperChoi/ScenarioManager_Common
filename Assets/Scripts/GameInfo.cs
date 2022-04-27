using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    private static GameInfo _Instance = null;

    public static GameInfo instance
    {
        get
        {
            if (_Instance == null)
                _Instance = GameObject.FindObjectOfType(typeof(GameInfo)) as GameInfo;

            return _Instance;
        }
    }

    protected Team _playerTeam;
    protected Team _monsterTeam;

    protected bool bBattleMode = false;

    public Team FindPlayerTeam()
    {
        return _playerTeam;
    }

    public Team FindEnemyTeam()
    {
        return _monsterTeam;
    }

    public void SetPlayerTeam(Team playerTeam)
    {
        this._playerTeam = playerTeam;
    }

    public void SetMonsterTeam(Team monsterTeam)
    {
        this._monsterTeam = monsterTeam;
    }

    public bool IsBattleMode()
    {
        return bBattleMode;
    }
}
