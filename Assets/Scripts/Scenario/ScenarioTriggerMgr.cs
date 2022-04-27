using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioTriggerMgr : MonoBehaviour
{
    public struct ScenarioIndex
    {
        public int scenario;
        public int subscenario;

        public ScenarioIndex(int scenarioIndex1, int scenarioIndex2) : this()
        {
            this.scenario = scenarioIndex1;
            this.subscenario = scenarioIndex2;
        }
    }

    public struct LinkScenario
    {
        public int firstIndex;
        public int firstSubIndex;
        public int secondIndex;
        public int secondSubIndex;
    }

    public List<LinkScenario> linkScenarioList;

    public List<ScenarioEventMove> TriggerList;

    ScenarioIndex findKey;
    List<TriggerEventObject.TriggerEventOBJ> progressTrigger;
    Dictionary<ScenarioIndex, ScenarioEventMove> _dicTrigger;
    TriggerEventObject.TriggerEventOBJ saveTrigger;

    public void Init()
    {
        _dicTrigger = new Dictionary<ScenarioIndex, ScenarioEventMove>();
        findKey = new ScenarioIndex();
        int len = TriggerList.Count;
        for (int i = 0; i < len; ++i)
        {
            if (TriggerList[i])
            {
                ScenarioIndex scenarioIndex = new ScenarioIndex(TriggerList[i].scenarioIndex, TriggerList[i].subScenarioIndex);
                if (_dicTrigger.ContainsKey(scenarioIndex) == false)
                    _dicTrigger.Add(scenarioIndex, TriggerList[i]);
            }
        }
    }

    public bool IsScenarioTrigger(int _index, int _subIndex)
    {
        findKey.scenario = _index;
        findKey.subscenario = _subIndex;
        return _dicTrigger.ContainsKey(findKey);
    }

    public void SetTriggerAble(bool _flag, int _index, int _subIndex)
    {
        if (IsScenarioTrigger(_index, _subIndex))
        {
            _dicTrigger[findKey].enabled = _flag;
        }
    }

    public bool GetFirstLinkScenario(int _dungeonTriggerprogress, out int firstLinkScenario, out int firstLinkSubscenario)
    {
        int len = linkScenarioList.Count;
        for (int i = 0; i < len; ++i)
        {
            if (linkScenarioList[i].secondIndex == _dungeonTriggerprogress)
            {
                firstLinkSubscenario = linkScenarioList[i].firstSubIndex;
                firstLinkScenario = linkScenarioList[i].firstIndex;
                return true;

            }
        }
        firstLinkSubscenario = 0;
        firstLinkScenario = 0;
        return false;
    }


    public bool IsLinkScenario(int _curScenario, int _curSubScenario, out int nextScenario, out int nextSubScenario)
    {
        int len = linkScenarioList.Count;
        for (int i = 0; i < len; ++i)
        {
            if (linkScenarioList[i].firstIndex == _curScenario && linkScenarioList[i].firstSubIndex == _curSubScenario)
            {
                nextScenario = linkScenarioList[i].secondIndex;
                nextSubScenario = linkScenarioList[i].secondSubIndex;
                return true;
            }
        }
        nextScenario = 0;
        nextSubScenario = 0;
        return false;
    }

    public bool IsLinkScenario(int _triggerIndex, int _dungeonTriggerprogress)
    {
        int len = linkScenarioList.Count;
        for (int i = 0; i < len; ++i)
        {
            if (linkScenarioList[i].firstIndex == _triggerIndex && linkScenarioList[i].secondIndex == _dungeonTriggerprogress)
            {
                return true;
            }
        }
        return false;
    }

    public void AddProgressTrigger(TriggerEventObject.TriggerEventOBJ _go, GameObject _trigger)
    {
        if (progressTrigger == null)
            progressTrigger = new List<TriggerEventObject.TriggerEventOBJ>();


        int len = progressTrigger.Count;
        for (int i = 0; i < len; ++i)
        {
            if (progressTrigger[i].dungeonProgressIndex == _go.dungeonProgressIndex)
            {
                if (progressTrigger[i].LinkedTrigger == _trigger)
                {
                    saveTrigger = progressTrigger[i];
                    break;
                }
            }
        }

        progressTrigger.Add(_go);
    }

    public void RemoveProgressTrigger(TriggerEventObject.TriggerEventOBJ _go)
    {
        if (progressTrigger != null)
        {
            progressTrigger.Remove(_go);
        }
    }

    public TriggerEventObject.TriggerEventOBJ GetSaveTrigger()
    {
        return saveTrigger;
    }

    private void OnDestroy()
    {
        if (progressTrigger != null)
            progressTrigger.Clear();
    }
}
