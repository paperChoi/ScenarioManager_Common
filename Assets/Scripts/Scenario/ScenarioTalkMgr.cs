using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AddExAttribute;
using UnityEngine.Playables;

public class ScenarioTalkMgr : MonoBehaviour
{
    public enum AddOptions
    {
        Talk_onlyOnce = (1 << 0),
        NextScenario = (1 << 1),
        Talkend_memberActive = (1 << 2),
    }

    public enum TALKTYPE { TALK = 0, EMOTICON, TIMELINE, ANIMATION, LOOKAT, CAMERA, WAIT, MOVE, ICON, VISIBLE, ACTIVATE, USE_ITEM, CONTROLLER_CHANGE, EXEC_FUNC }
    public enum WAIT_CONDITION { NONE, TIMELINE, MOVE, CAMERA }
    public enum ACTIVATE_TYPE { GAMEOBJECT, COLLIDER }

    [System.Serializable]
    public struct TalkContent
    {
        public int targetIndex;
        public TALKTYPE talktype;
        [ConditionalField("talktype", TALKTYPE.TALK)] public string text;
        [ConditionalField("talktype", TALKTYPE.TALK)] public ScenarioTalkUI.TALKUITYPE uitype;
        [ConditionalField("talktype", TALKTYPE.TIMELINE)] public PlayableDirector timelineObject;
        [ConditionalField("talktype", TALKTYPE.ANIMATION)] public string stateName;
        [ConditionalField("talktype", TALKTYPE.LOOKAT)] public GameObject lookAt;
        [ConditionalField("talktype", TALKTYPE.LOOKAT)] public float lookAtSpeed;
        [ConditionalField("talktype", TALKTYPE.CAMERA)] public float vCamBlendDuration;
        [ConditionalField("talktype", TALKTYPE.WAIT)] public WAIT_CONDITION waitCondition;
        [ConditionalField("talktype", TALKTYPE.MOVE)] public GameObject moveTarget;
        [ConditionalField("talktype", TALKTYPE.MOVE)] public float moveSpeed;
        [ConditionalField("talktype", TALKTYPE.ICON)] public string emoticonName;
        [ConditionalField("talktype", TALKTYPE.VISIBLE)] public GameObject visibleTarget;
        [ConditionalField("talktype", TALKTYPE.VISIBLE)] public bool visibility;
        [ConditionalField("talktype", TALKTYPE.ACTIVATE)] public GameObject activateTarget;
        [ConditionalField("talktype", TALKTYPE.ACTIVATE)] public ACTIVATE_TYPE activateType;
        [ConditionalField("talktype", TALKTYPE.ACTIVATE)] public bool activateValue;
        [ConditionalField("talktype", TALKTYPE.USE_ITEM)] public int itemID;
        [ConditionalField("talktype", TALKTYPE.EXEC_FUNC)] public string ComponentName;
        [ConditionalField("talktype", TALKTYPE.EXEC_FUNC)] public string ComponentFunc;
        [ConditionalField("talktype", TALKTYPE.EXEC_FUNC)] public GameObject ComponentParameter;
        public float waitTime;
    }

    [System.Serializable]
    public struct ScenarioContent
    {
        public int index;
        public int subIndex;
        public List<TalkContent> move_talk;
        public List<TalkContent> talkList;
        public List<TalkContent> exitTalkList;
        public bool memberActive_Talkend;
        [EnumFlagsAttribute(typeof(AddOptions))] public int AdditionalOptions;

        public bool DoBattleWhenEnd;
    }

    [System.Serializable]
    public struct ScenarioIndex
    {
        public int scenario;
        public int subscenario;
    }

    [System.Serializable]
    public struct ScenarioContentData
    {
        public string name;
        public ScenarioContent Data;
    }
    public List<ScenarioContentData> ScenarioContentDataList;

    public List<ScenarioContent> scenarioContent;
    public ScenarioTalkUI talkUI;

    Dictionary<ScenarioIndex, List<TalkContent>> _dicScenarioMoveContent;
    Dictionary<ScenarioIndex, List<TalkContent>> _dicScenarioContent;
    Dictionary<ScenarioIndex, List<TalkContent>> _dicExitScenarioContent;
    ScenarioIndex findKey;
    List<TalkContent> _talList;
    List<TalkContent> _talkMoveList;

    Dictionary<ScenarioIndex, int> _dicSkipTalk;
    bool bTalkSkip = false;

    public int curTalkIndex = 0;
    public int curTalkMoveIndex = 0;

    public Vector2 uiAddPos;

    void Awake()
    {
        foreach (var contentData in ScenarioContentDataList)
        {
            int existIndex = scenarioContent.FindIndex(d => d.index == contentData.Data.index);
            if (existIndex != -1)
            {
                scenarioContent[existIndex] = contentData.Data;
                Debug.LogFormat("ScenarioTalkMgr: index {0} overriden by {1}", contentData.Data.index, contentData.name);
            }
            else
            {
                scenarioContent.Add(contentData.Data);
            }
        }
    }

    public void LoadScenarioData(ScenarioContent data)
    {
        int existIndex = scenarioContent.FindIndex(d => d.index == data.index);
        if (existIndex != -1)
        {
            scenarioContent[existIndex] = data;
            ScenarioIndex index = new ScenarioIndex();
            index.scenario = data.index;
            index.subscenario = data.subIndex;
            if (_dicSkipTalk.ContainsKey(index))
            {
                _dicSkipTalk.Remove(index);
            }
            Debug.LogFormat("ScenarioTalkMgr: index {0} overriden", data.index);
        }
        else
        {
            scenarioContent.Add(data);
        }
    }

    private void Start()
    {
        if (_dicScenarioContent == null)
            Init();
    }

    void Init()
    {
        _dicScenarioContent = new Dictionary<ScenarioIndex, List<TalkContent>>();
        _dicExitScenarioContent = new Dictionary<ScenarioIndex, List<TalkContent>>();
        _dicScenarioMoveContent = new Dictionary<ScenarioIndex, List<TalkContent>>();
        _dicSkipTalk = new Dictionary<ScenarioIndex, int>();

        for (int i = 0; i < scenarioContent.Count; ++i)
        {
            ScenarioIndex scenarioIndex = new ScenarioIndex();
            scenarioIndex.scenario = scenarioContent[i].index;
            scenarioIndex.subscenario = scenarioContent[i].subIndex;

            if (_dicScenarioContent.ContainsKey(scenarioIndex) == false)
                _dicScenarioContent.Add(scenarioIndex, scenarioContent[i].talkList);

            if (scenarioContent[i].exitTalkList.Count > 0)
            {
                if (_dicExitScenarioContent.ContainsKey(scenarioIndex) == false)
                    _dicExitScenarioContent.Add(scenarioIndex, scenarioContent[i].exitTalkList);
            }

            if (scenarioContent[i].move_talk.Count > 0)
            {
                if (_dicScenarioMoveContent.ContainsKey(scenarioIndex) == false)
                    _dicScenarioMoveContent.Add(scenarioIndex, scenarioContent[i].move_talk);
            }

            if (IsOption(scenarioContent[i].AdditionalOptions, AddOptions.Talk_onlyOnce))
                _dicSkipTalk.Add(scenarioIndex, 0);
        }
    }

    public bool IsNextScenario(int _curScenario, int _subScenario)
    {
        int len = scenarioContent.Count;
        for (int i = 0; i < len; ++i)
        {
            if (scenarioContent[i].index == _curScenario && scenarioContent[i].subIndex == _subScenario)
            {
                return IsOption(scenarioContent[i].AdditionalOptions, AddOptions.NextScenario);
            }
        }

        return false;
    }

    public bool IsMemberActive(int _curScenario, int _subScenario)
    {
        int len = scenarioContent.Count;
        for (int i = 0; i < len; ++i)
        {
            if (scenarioContent[i].index == _curScenario && scenarioContent[i].subIndex == _subScenario)
                return IsOption(scenarioContent[i].AdditionalOptions, AddOptions.Talkend_memberActive);
        }
        return false;
    }

    public void SetTalk(int _index, int _subIndex)
    {
        if (_dicScenarioContent == null)
            Init();

        findKey.scenario = _index;
        findKey.subscenario = _subIndex;

        if (_dicScenarioContent.ContainsKey(findKey))
        {
            int len = scenarioContent.Count;
            if (_dicSkipTalk.ContainsKey(findKey))
            {
                if (_dicSkipTalk[findKey] < 1)
                    bTalkSkip = false;
                else
                    bTalkSkip = true;

                int refCount = _dicSkipTalk[findKey];
                _dicSkipTalk[findKey] = ++refCount;
            }
            else
                bTalkSkip = false;

            _talList = _dicScenarioContent[findKey];
            curTalkIndex = 0;
        }
        else
        {
            curTalkIndex = 0;
            bTalkSkip = false;
        }
    }

    public bool SetMoveTalk(int _index, int _subIndex)
    {
        findKey.scenario = _index;
        findKey.subscenario = _subIndex;

        if (_dicScenarioMoveContent.ContainsKey(findKey))
        {
            _talkMoveList = _dicScenarioMoveContent[findKey];
            curTalkMoveIndex = 0;
            return true;
        }
        return false;
    }

    public bool OnNextMoveTalk(out TalkContent outContent)
    {
        if (_talkMoveList != null && curTalkMoveIndex < _talkMoveList.Count)
        {
            outContent = _talkMoveList[curTalkMoveIndex];
            ++curTalkMoveIndex;
            return true;
        }
        outContent = new TalkContent();
        return false;
    }

    public bool OnNextMoveTalk(out TALKTYPE talkType, out ScenarioTalkUI.TALKUITYPE _uiType, out int _heroIndex, out string _text, out PlayableDirector _timelineObject)
    {
        if (_talkMoveList != null)
        {
            if (curTalkMoveIndex >= _talkMoveList.Count)
            {
                talkType = TALKTYPE.TALK;
                _heroIndex = 0;
                _text = string.Empty;
                _uiType = ScenarioTalkUI.TALKUITYPE.LEFT;
                _timelineObject = null;
                return false;
            }
            else
            {
                talkType = _talkMoveList[curTalkMoveIndex].talktype;
                _heroIndex = _talkMoveList[curTalkMoveIndex].targetIndex;
                _text = _talkMoveList[curTalkMoveIndex].text;
                _uiType = _talkMoveList[curTalkMoveIndex].uitype;
                _timelineObject = _talkMoveList[curTalkMoveIndex].timelineObject;
                ++curTalkMoveIndex;
                return true;
            }
        }

        talkType = TALKTYPE.TALK;
        _heroIndex = 0;
        _text = string.Empty;
        _uiType = ScenarioTalkUI.TALKUITYPE.LEFT;
        _timelineObject = null;
        return false;
    }

    public bool SetExitTalk(int _index, int _subIndex)
    {
        findKey.scenario = _index;
        findKey.subscenario = _subIndex;

        if (_dicExitScenarioContent.ContainsKey(findKey))
        {
            _talList = _dicExitScenarioContent[findKey];
            curTalkIndex = 0;
            return true;
        }
        return false;
    }

    public int GetScenarioContentLenght()
    {
        return _dicScenarioContent[findKey].Count;
    }

    public bool OnNextTalk(out TalkContent outContent, out bool outTalkSkip)
    {
        outTalkSkip = bTalkSkip;

        if (_talList != null && curTalkIndex < _talList.Count)
        {
            outContent = _talList[curTalkIndex];
            ++curTalkIndex;
            return true;
        }
        outContent = new TalkContent();
        return false;
    }

    public bool IsEndScenario()
    {
        if (_talList != null)
        {
            if (curTalkIndex >= _talList.Count)
                return true;
            else
                return false;
        }
        return true;
    }

    public bool IsOption(int options, AddOptions type)
    {
        return 0 != (options & (int)type);
    }
}
