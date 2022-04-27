using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioHeroController : MonoBehaviour
{
    [System.Serializable]
    public struct HeroIndex
    {
        public GameObject hero;
        public int index;
        public bool initVisibility;
    }
    public Dictionary<int, GameObject> _dicHeroIndex;

    public GameObject player;
    public GameObject[] members;
    public GameObject[] npcs;
    GameObject inputControllerHero;

    public List<HeroIndex> specificPawnList;

    public Dictionary<int, GameObject> dicHerolist;
    public List<HeroIndex> specialPawnList;

    public void Init()
    {
        if (dicHerolist == null)
            dicHerolist = new Dictionary<int, GameObject>();

        HeroIndex hero;
        hero.index = 1;
        hero.hero = player;
        hero.initVisibility = true;
        dicHerolist.Add(hero.index, hero.hero);

        for (int i = 0; i < members.Length; ++i)
        {
            int findIndex = members[i].name.IndexOf("Hero");
            if (findIndex == -1)
                continue;

            int heroNameLen = members[i].name.Length;
            int findStartIndex = findIndex + 4;
            int findLength = heroNameLen - findStartIndex;
            string nameIndex = members[i].name.Substring(findStartIndex, findLength);

            int heroIndex = 0;
            if (System.Int32.TryParse(nameIndex, out int _index))
                heroIndex = _index;

            HeroIndex heroAdd;
            heroAdd.index = heroIndex;
            heroAdd.hero = members[i];
            heroAdd.initVisibility = members[i].activeSelf;
            dicHerolist.Add(heroAdd.index, heroAdd.hero);
        }

        foreach (var pawnInfo in specialPawnList)
        {
            if (pawnInfo.hero == null) continue;

            if (dicHerolist.ContainsKey(pawnInfo.index))
            {
                Debug.LogErrorFormat("Duplication key exists. {0}, {1} with {2}", pawnInfo.index, dicHerolist[pawnInfo.index].name, pawnInfo.hero.name);
                break;
            }

            var controller = pawnInfo.hero.GetComponent<AIController_Scenario>();
            if (controller == null)
            {
                controller = pawnInfo.hero.AddComponent<AIController_Scenario>();
                controller.enabled = false;
            }
            pawnInfo.hero.SetActive(pawnInfo.initVisibility);
            _dicHeroIndex.Add(pawnInfo.index, pawnInfo.hero);
        }
    }

    public void SetPlayerComponentEnable(bool _flag)
    {
        Debug.LogFormat("SetPlayerComponentEnable {0}", _flag);
        if (player)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller)
                controller.enabled = _flag;

            var ScenarioMove = player.GetComponent<AIController_Scenario>();

            if (_flag == false)
            {
                if (ScenarioMove)
                {
                    ScenarioMove.enabled = true;
                    ScenarioMove.StartScenario();
                }
            }
            else
            {
                if (ScenarioMove)
                    ScenarioMove.enabled = false;
            }
        }
    }

    public void SetMemberControllerEnable(bool _flag)
    {
        if (members.Length > 0)
        {
            for (int i = 0; i < members.Length; ++i)
            {
                var ScenarioMove = members[i].GetComponent<AIController_Scenario>();

                if (_flag == false)
                {
                    if (ScenarioMove)
                    {
                        ScenarioMove.enabled = true;
                        ScenarioMove.StartScenario();
                    }
                }
                else
                {
                    ScenarioMove.enabled = false;
                    if (members[i].activeSelf == false)
                        members[i].SetActive(true);
                }
            }
        }
        SetSpecificPawnControllerEnable(_flag);
    }

    public void SetPlayerControllerEnable(bool _flag)
    {
        var input = player.GetComponent<InputController>();
        if (input)
        {
            input.enabled = _flag;
            if (_flag == true)
                inputControllerHero = player;
        }
    }

    public void SetSpecificPawnControllerEnable(bool _flag)
    {
        if (specificPawnList != null)
        {
            foreach (var info in specificPawnList)
            {
                var pawn = info.hero;

                SetSpecificPawnControllerEnable(info.index, _flag);
            }
        }
    }

    public void SetSpecificPawnControllerEnable(int heroIndex, bool _flag)
    {
        Debug.LogFormat("SetSpecificPawnControllerEnable {0}, {1}", heroIndex, _flag);
        if (_dicHeroIndex.ContainsKey(heroIndex))
        {
            var pawn = _dicHeroIndex[heroIndex];
            Controller controller = null;

            if (pawn.CompareTag("Npc"))
            {
                var ai = pawn.GetComponent<AIController_Scenario>();
                if (ai && ai.enabled == false) ai.enabled = true;
                return;
            }

            if (controller == null)
                controller = pawn.GetComponent<PlayerController>();

            if (controller == null)
                return;

            controller.enabled = _flag;

            var scenarioMove = pawn.GetComponent<AIController_Scenario>();
            if (scenarioMove)
            {
                scenarioMove.enabled = !_flag;
                if (scenarioMove.enabled)
                {
                    scenarioMove.StartScenario();
                }
            }
        }
    }

    public bool IsHelpHero(int _index)
    {
        string targetName = _dicHeroIndex[_index].name;
        string name = player.name;
        if (targetName.Equals(name))
            return false;

        int len = members.Length;
        for (int i = 0; i < len; ++i)
        {
            if (targetName.Equals(members[i].name))
                return false;
        }

        if (specificPawnList != null)
        {
            foreach (var heroInfo in specificPawnList)
            {
                if (heroInfo.hero == _dicHeroIndex[_index]) return false;
            }
        }

        return true;
    }

    public void SetScenarioController(int _index, bool _flag)
    {
        Debug.LogFormat("SetScenarioController: {0} = {1}", _index, _flag);

        string _tag = _dicHeroIndex[_index].tag;
        if (_tag.Equals("Npc"))
        {
            var _pawn = _dicHeroIndex[_index].GetComponent<Pawn>();
            if (_pawn)
                _pawn.enabled = true;

            var _controller = _dicHeroIndex[_index].GetComponent<CharacterController>();
            if (_controller)
                _controller.enabled = true;
        }

        var scenario = _dicHeroIndex[_index].GetComponent<AIController_Scenario>();
        if (scenario)
        {
            scenario.enabled = _flag;
            if (_flag)
                scenario.StartScenario();
        }

        var input = _dicHeroIndex[_index].GetComponent<InputController>();
        if (input)
            input.enabled = false;
    }

    public void SetMove(int _heroIndex, Vector3 _pos, Quaternion _rot, float _speed, AIController_Scenario.ArriveDelegate arrDelegate)
    {
        GameObject obj = GetHero(_heroIndex);
        if (obj != null)
        {
            var ScenarioMove = obj.GetComponent<AIController_Scenario>();
            if (ScenarioMove)
            {
                ScenarioMove.SetTarget(_pos, _rot, _speed, arrDelegate);
            }
        }
    }

    public bool IsMove(int _heroIndex)
    {
        GameObject obj = GetHero(_heroIndex);
        var ScenarioMove = obj.GetComponent<AIController_Scenario>();
        if (ScenarioMove)
        {
            return ScenarioMove.bArrive;
        }
        return false;
    }

    public GameObject GetHero(int _index)
    {
        if (_dicHeroIndex.ContainsKey(_index))
        {
            return _dicHeroIndex[_index];
        }
        return null;
    }

    public void SetInputController(int _index, bool _flag)
    {
        var inputController = _dicHeroIndex[_index].GetComponent<InputController>();
        if (inputController)
        {
            inputController.enabled = _flag;
            if (_flag == true)
                inputControllerHero = _dicHeroIndex[_index];
        }
    }

    public void SetTagChange(int _index, string _tag)
    {
        if (_dicHeroIndex.ContainsKey(_index))
        {
            _dicHeroIndex[_index].tag = _tag;
        }
    }

    public void SetLayerChange(int _index, int _layer)
    {
        if (_dicHeroIndex.ContainsKey(_index))
        {
            _dicHeroIndex[_index].layer = _layer;
        }
    }

    public void SetPosition(int _heroIndex, Vector3 _pos)
    {
        GameObject obj = GetHero(_heroIndex);
        var pawn = obj.GetComponent<Pawn>();
        if (pawn && pawn.enabled)
        {
            pawn.SetPosition(_pos);
        }
        else
        {
            obj.transform.position = _pos;
        }
    }

    public void SetRotation(int _heroIndex, float axis_Y)
    {
        GameObject obj = GetHero(_heroIndex);
        var ScenarioMove = obj.GetComponent<AIController_Scenario>();
        if (ScenarioMove)
        {
            ScenarioMove.ReRotation(axis_Y);
        }
    }

    public bool IsHeroActive(int _index)
    {
        if (_dicHeroIndex.ContainsKey(_index))
        {
            return _dicHeroIndex[_index].activeSelf;
        }
        return false;
    }

    public void HeroVisible(int _index, bool _bVisible)
    {
        _dicHeroIndex[_index].SetActive(_bVisible);
        Debug.LogFormat("HeroVisible: {0} = {1}", _index, _bVisible);
    }
}
