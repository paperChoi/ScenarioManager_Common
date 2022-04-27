using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ScenarioMgr : MonoBehaviour
{
    static ScenarioMgr _instance = null;

    public static ScenarioMgr GetInstance()
    {
        if (_instance == null)
        {
            var go = GameObject.Find("ScenarioMgr");
            if (go == null)
            {
                go = new GameObject("ScenarioMgr");
                go.AddComponent<ScenarioMgr>();
            }

            _instance = go.GetComponent<ScenarioMgr>();
        }

        return _instance;
    }

    public ScenarioHeroController scenarioHero;
    public ScenarioTalkMgr scenarioTalkMgr;
    public ScenarioTriggerMgr triggerMgr;

    Camera lastTimelineCamera = null;
    PlayableDirector lastTimeline = null;
    
    protected bool _isScenario = false;
    public bool isScenario
    {
        get { return _isScenario; }
        set { _isScenario = value; }
    }

    GameObject talkHero;

    int curScenario;
    int curSubScenario;
    int controllerHeroIndex;
    int beforeControllerHeroIndex;
    bool isMoveTalk;
    bool bExitTalk;
    bool isTalkCoroutine;

    Vector2 addUIPos;
    List<int> keepMoveHeroIndex = new List<int>();
    List<int> scenarioCompleted = new List<int>();
    List<int> heroMoveList = new List<int>();

    private void Start()
    {
        if (scenarioHero)
            scenarioHero.Init();
        if (triggerMgr)
            triggerMgr.Init();

        Init();
    }

    private void Init()
    {
        controllerHeroIndex = 1;
        beforeControllerHeroIndex = controllerHeroIndex;
        isMoveTalk = false;
        bExitTalk = false;
        isTalkCoroutine = false;
    }

    public bool CheckCompletedScenario(int scenarioIndex)
    {
        return scenarioCompleted.Contains(scenarioIndex);
    }

    public void SetScenarioCompletedState(int scenarioIndex, bool completed)
    {
        if(completed)
        {
            if(false == scenarioCompleted.Contains(scenarioIndex))
                scenarioCompleted.Add(scenarioIndex);
        }
        else
        {
            if(scenarioCompleted.Contains(scenarioIndex))
                scenarioCompleted.Remove(scenarioIndex);
        }
    }

    public void SetScenario(int _index, int subIndex)
    {
        curScenario = _index;
        curSubScenario = subIndex;
        if (scenarioHero)
        {
            scenarioHero.SetPlayerComponentEnable(false);
            scenarioHero.SetPlayerControllerEnable(false);

            scenarioHero.SetMemberControllerEnable(false);
        }
        if (scenarioTalkMgr)
        {
            scenarioTalkMgr.SetTalk(curScenario, curSubScenario);
            isMoveTalk = scenarioTalkMgr.SetMoveTalk(curScenario, curSubScenario);
        }

        if (triggerMgr)
        {
            StartCoroutine(OnNextTalkIE());
        }
    }

    void SetScenarioEndData(bool _bExitArea)
    {
        if (scenarioHero)
            scenarioHero.SetMemberControllerEnable(true);

        if (lastTimeline)
        {
            lastTimeline.gameObject.SetActive(false);
        }
    }

    public void ExitScenarioArea()
    {
        if (scenarioTalkMgr.SetExitTalk(curScenario, curSubScenario))
        {
            bExitTalk = true;

            if (scenarioHero.IsHelpHero(controllerHeroIndex))
                scenarioHero.SetScenarioController(controllerHeroIndex, true);
            else
                scenarioHero.SetPlayerComponentEnable(false);


            StartCoroutine(OnNextTalkIE());
        }
        else
            SetScenarioEndData(true);

        keepMoveHeroIndex.Clear();
    }

    public Camera GetScenarioCamera()
    {
        if (lastTimelineCamera && lastTimelineCamera.gameObject.activeInHierarchy)
        {
            return lastTimelineCamera;
        }
        return Camera.main;
    }

    public IEnumerator OnNextTalkIE()
    {
        isTalkCoroutine = true;

        bool runNextTalkForTimeline = false;

        if (triggerMgr.IsScenarioTrigger(curScenario, curSubScenario))
        {
            isScenario = true;
            UIManager.instance.SetTalkUI(false);
        }

        yield return null;

        isScenario = true;
        if (scenarioTalkMgr.OnNextTalk(out ScenarioTalkMgr.TalkContent _content, out bool _talkSkip))
        {
            if (_talkSkip)
            {
                if (_content.talktype == ScenarioTalkMgr.TALKTYPE.MOVE)
                {
                    Debug.LogFormat("{0} : MoveTo {1}", _content.targetIndex, _content.moveTarget);
                    if (false == heroMoveList.Contains(_content.targetIndex))
                    {
                        heroMoveList.Add(_content.targetIndex);
                    }
                    scenarioHero.SetMove(_content.targetIndex, _content.moveTarget.transform.position, _content.moveTarget.transform.rotation, _content.moveSpeed, AIController_Scenario.ArriveDelegate.None);
                    runNextTalkForTimeline = true;

                    if (0 < _content.waitTime)
                    {
                        yield return new WaitForSeconds(_content.waitTime);
                    }
                }
                else if (_content.talktype == ScenarioTalkMgr.TALKTYPE.WAIT && _content.waitCondition == ScenarioTalkMgr.WAIT_CONDITION.MOVE)
                {
                    while (true)
                    {
                        int arriveCount = 0;
                        foreach (int heroIndex in heroMoveList)
                        {
                            if (scenarioHero.IsMove(heroIndex))
                            {
                                ++arriveCount;
                            }
                        }
                        if (arriveCount == heroMoveList.Count) break;

                        yield return null;
                    }
                    heroMoveList.Clear();
                    runNextTalkForTimeline = true;

                    if (0 < _content.waitTime)
                    {
                        yield return new WaitForSeconds(_content.waitTime);
                    }
                }
                else
                {
                    talkHero = scenarioHero.GetHero(_content.targetIndex);
                    if (talkHero)
                    {
                        if (_content.talktype == ScenarioTalkMgr.TALKTYPE.CONTROLLER_CHANGE)
                        {
                            SetControllerHeroChange(_content.targetIndex);
                            runNextTalkForTimeline = true;
                        }
                    }
                }
            }
            else
            {
                if (_content.talktype == ScenarioTalkMgr.TALKTYPE.ACTIVATE)
                {
                    var target = _content.activateTarget;
                    switch (_content.activateType)
                    {
                        case ScenarioTalkMgr.ACTIVATE_TYPE.GAMEOBJECT:
                            target.SetActive(_content.activateValue);
                            break;
                        case ScenarioTalkMgr.ACTIVATE_TYPE.COLLIDER:
                            var collider = target.GetComponent<Collider>();
                            collider.enabled = _content.activateValue;
                            break;
                    }
                    runNextTalkForTimeline = true;
                    if (0 < _content.waitTime)
                    {
                        yield return new WaitForSeconds(_content.waitTime);
                    }
                }
                else if (_content.talktype == ScenarioTalkMgr.TALKTYPE.TIMELINE)
                {
                    addUIPos = Vector2.zero;
                    runNextTalkForTimeline = true;
                    if (0 < _content.waitTime)
                    {
                        yield return new WaitForSeconds(_content.waitTime);
                    }
                }
                else if (_content.talktype == ScenarioTalkMgr.TALKTYPE.MOVE)
                {
                    Debug.LogFormat("{0} : MoveTo {1}", _content.targetIndex, _content.moveTarget);
                    if (false == heroMoveList.Contains(_content.targetIndex))
                    {
                        heroMoveList.Add(_content.targetIndex);
                    }
                    scenarioHero.SetMove(_content.targetIndex, _content.moveTarget.transform.position, _content.moveTarget.transform.rotation, _content.moveSpeed, AIController_Scenario.ArriveDelegate.None);
                    runNextTalkForTimeline = true;

                    if (0 < _content.waitTime)
                    {
                        yield return new WaitForSeconds(_content.waitTime);
                    }
                }
                else if (_content.talktype == ScenarioTalkMgr.TALKTYPE.WAIT)
                {
                    switch (_content.waitCondition)
                    {
                        case ScenarioTalkMgr.WAIT_CONDITION.MOVE:
                            while (true)
                            {
                                int arriveCount = 0;
                                foreach (int heroIndex in heroMoveList)
                                {
                                    if (scenarioHero.IsMove(heroIndex))
                                    {
                                        ++arriveCount;
                                    }
                                }
                                if (arriveCount == heroMoveList.Count) break;

                                yield return null;
                            }
                            heroMoveList.Clear();
                            runNextTalkForTimeline = true;
                            break;
                        case ScenarioTalkMgr.WAIT_CONDITION.TIMELINE:
                            while (lastTimeline != null && lastTimeline.gameObject.activeSelf && lastTimeline.state == PlayState.Playing)
                            {
                                yield return null;
                            }
                            runNextTalkForTimeline = true;
                            break;
                        case ScenarioTalkMgr.WAIT_CONDITION.NONE:
                            runNextTalkForTimeline = true;
                            break;
                        default:
                            Debug.LogErrorFormat("Not implement wait condition - {0}", _content.waitCondition);
                            break;
                    }
                    if (0 < _content.waitTime)
                    {
                        yield return new WaitForSeconds(_content.waitTime);
                    }
                }
                else
                {
                    talkHero = scenarioHero.GetHero(_content.targetIndex);
                    if (talkHero)
                    {
                        if (_content.talktype == ScenarioTalkMgr.TALKTYPE.VISIBLE)
                        {
                            Debug.LogFormat("{0} : Visible {1}", talkHero, _content.visibility);
                            if (_content.visibleTarget)
                            {
                                scenarioHero.SetPosition(_content.targetIndex, _content.visibleTarget.transform.position);
                                scenarioHero.SetRotation(_content.targetIndex, _content.visibleTarget.transform.eulerAngles.y);
                            }
                            scenarioHero.HeroVisible(_content.targetIndex, _content.visibility);
                            if (_content.visibility)
                            {
                                if (false == scenarioHero.IsHelpHero(_content.targetIndex))
                                {
                                    scenarioHero.SetSpecificPawnControllerEnable(_content.targetIndex, !_content.visibility);
                                }
                                else
                                {
                                    scenarioHero.SetScenarioController(_content.targetIndex, true);

                                    scenarioHero.SetTagChange(_content.targetIndex, "HelpMember");
                                    int maskLayer = LayerMask.NameToLayer("NPC");
                                    scenarioHero.SetLayerChange(_content.targetIndex, maskLayer);
                                }
                            }
                            runNextTalkForTimeline = true;
                            if (0 < _content.waitTime)
                            {
                                yield return new WaitForSeconds(_content.waitTime);
                            }
                        }
                        else if (_content.talktype == ScenarioTalkMgr.TALKTYPE.LOOKAT)
                        {
                            Debug.LogFormat("{0} : LookAt {1}", talkHero, _content.lookAt);
                            var ai = talkHero.GetComponent<AIController_Scenario>();
                            if (ai)
                            {
                                runNextTalkForTimeline = true;
                            }
                            if (0 < _content.waitTime)
                            {
                                yield return new WaitForSeconds(_content.waitTime);
                            }
                        }
                        else if (_content.talktype == ScenarioTalkMgr.TALKTYPE.ANIMATION)
                        {
                            Debug.Log(_content.stateName);
                            var pawn = talkHero.GetComponent<Pawn>();
                            if (pawn)
                            {
                                runNextTalkForTimeline = true;
                            }
                            if (0 < _content.waitTime)
                            {
                                yield return new WaitForSeconds(_content.waitTime);
                            }
                        }
                        else if (_content.talktype == ScenarioTalkMgr.TALKTYPE.CONTROLLER_CHANGE)
                        {
                            SetControllerHeroChange(_content.targetIndex);
                            runNextTalkForTimeline = true;
                        }
                        else
                        {
                            addUIPos = Vector2.zero;
                            addUIPos.y = talkHero.GetComponent<Pawn>().playerHeight;

                            UIManager.instance.scenarioTalkUI.Init(_content.uitype, _content.text, talkHero.transform.position, addUIPos, ScenarioTalkUI.UIEVENTTYPE.BUTTON, _content.talktype);

                            if (_content.talktype == ScenarioTalkMgr.TALKTYPE.TALK)
                                UIManager.instance.SetTalkUI(true, _content.waitTime);
                        }
                    }
                    else
                    {
                        string log = string.Format("heroIndex Not Find - {0}", _content.targetIndex);
                        Debug.Log(log);
                    }
                }
            }
        }

        isTalkCoroutine = false;

        if (runNextTalkForTimeline)
        {
            yield return StartCoroutine(OnNextTalkIE());
        }

        yield return null;
    }

    void SetControllerHeroChange(int _heroIndex)
    {
        beforeControllerHeroIndex = controllerHeroIndex;
        controllerHeroIndex = _heroIndex;

        if (controllerHeroIndex == beforeControllerHeroIndex)
            return;

        scenarioHero.SetInputController(beforeControllerHeroIndex, false);
        scenarioHero.SetTagChange(beforeControllerHeroIndex, "HelpMember");
        int maskLayer = LayerMask.NameToLayer("NPC");
        scenarioHero.SetLayerChange(beforeControllerHeroIndex, maskLayer);


        scenarioHero.SetInputController(controllerHeroIndex, true);
        scenarioHero.SetTagChange(controllerHeroIndex, "Player");
        maskLayer = LayerMask.NameToLayer("Player");
        scenarioHero.SetLayerChange(controllerHeroIndex, maskLayer);
    }
}
