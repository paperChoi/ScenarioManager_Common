using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AddExAttribute;

public class ScenarioEventMove : MonoBehaviour
{
    public enum VisibleType { None, Visible, Invisible }
    public enum WaitingCondition { None, Time, Timeline, ExecFunc }
    public enum MoveEventType { None, ExitMove, TalkAfter }

    public enum FinishEventType { None, Object_Func, Coroutine, Delegate }
    public enum FinishSelf_CoroutineOption { None, OnInputController }
    public enum FinishSelf_DelegateOption { None, Hide }

    [System.Serializable]
    public struct OnFinishNotify
    {
        public GameObject OnFinishNotifyObject;
        public string OnFinishNotifyFunc;
    }

    [System.Serializable]
    public struct OnFinishSelfFunc
    {
        public string OnComponentName;
        public string OnComponentFunc;
        public GameObject ComponentParameter;
    }

    [System.Serializable]
    public struct HeroPositionContainer
    {
        public int heroIndex;
        public GameObject positionOBJ;
        public float speed;
        public VisibleType visibleType;
        [ConditionalField("visibleType", VisibleType.Visible)] public GameObject visiblepos;

        public FinishEventType finishEvent;
        [ConditionalField("finishEvent", FinishEventType.Object_Func)] public OnFinishNotify finishNotify;
        [ConditionalField("finishEvent", FinishEventType.Coroutine)] public float finsihDelayTime;
        [ConditionalField("finishEvent", FinishEventType.Coroutine)] public OnFinishSelfFunc finishSelfNotify;
        [ConditionalField("finishEvent", FinishEventType.Coroutine)] public FinishSelf_CoroutineOption CoroutineAfterOption;
        [ConditionalField("finishEvent", FinishEventType.Delegate)] public FinishSelf_DelegateOption finishSelf_Delegate;
    }

    [System.Serializable]
    public struct ProgressContainer
    {
        public int progressIndex;
        public WaitingCondition waitingCondition;
        [ConditionalField("waitingCondition", WaitingCondition.Time)] public float delayTime;
        public List<HeroPositionContainer> heroPosition;
    }

    [System.Serializable]
    public struct AddHeroPosition
    {
        public List<ProgressContainer> exitHeroPosition;
        public List<ProgressContainer> talkAfterMovePosition;
    }

    [System.Serializable]
    public struct HeroRotation
    {
        public int heroIndex;
        public float axis_Y;
    }

    [System.Serializable]
    public struct HeroRotationIndex
    {
        public int progressIndex;
        public List<HeroRotation> heroRotationValue;
    }

    [System.Serializable]
    public struct HeroRotationList
    {
        public List<HeroRotationIndex> heroRotationList;
    }

    [System.Serializable]
    public class KeepMoveInfo
    {
        public int progressIndex;
        public WaitingCondition waitingCondition;
        [ConditionalField("waitingCondition", WaitingCondition.Time)] public float delayTime;
        public List<KeepMoveContainer> heroPosition;
        //string waitAni;
    }

    [System.Serializable]
    public class KeepMoveContainer
    {
        public int heroIndex;
        public List<GameObject> positionList;
        public float speed;
        [HideInInspector] public int curPositionIndex = 0;
    }

    public int scenarioIndex;
    public int subScenarioIndex;
    public int nextScenarioIndex;

    public List<KeepMoveInfo> heroKeepMoveInfo;

    public int GetHeroKeepMoveCount(int _progerssIndex)
    {
        int _count = 0;
        int len = heroKeepMoveInfo.Count;
        for (int i = 0; i < len; ++i)
        {
            if (heroKeepMoveInfo[i].progressIndex == _progerssIndex)
            {
                _count = heroKeepMoveInfo[i].heroPosition.Count;
                break;
            }
        }
        return _count;
    }

    public void GetHeroKeepMove(int _progressIndex, int _listIndex, out int heroIndex, out Vector3 _pos, out Quaternion _rot, out float _speed)
    {
        int len = heroKeepMoveInfo.Count;
        for (int i = 0; i < len; ++i)
        {
            if (heroKeepMoveInfo[i].progressIndex == _progressIndex)
            {
                heroIndex = heroKeepMoveInfo[i].heroPosition[_listIndex].heroIndex;
                int _index = heroKeepMoveInfo[i].heroPosition[_listIndex].curPositionIndex;

                _pos = heroKeepMoveInfo[i].heroPosition[_listIndex].positionList[_index].transform.position;
                _rot = heroKeepMoveInfo[i].heroPosition[_listIndex].positionList[_index].transform.rotation;
                _speed = heroKeepMoveInfo[i].heroPosition[_listIndex].speed;


                return;
            }
        }
        heroIndex = 0;
        _pos = Vector3.zero;
        _rot = Quaternion.identity;
        _speed = 0;
    }

    public void SetNextKeepMoveInfo(int _progressIndex, int _listIndex)
    {
        int len = heroKeepMoveInfo.Count;
        for (int i = 0; i < len; ++i)
        {
            if (heroKeepMoveInfo[i].progressIndex == _progressIndex)
            {
                int _count = heroKeepMoveInfo[i].heroPosition[_listIndex].positionList.Count - 1;
                int _index = ++heroKeepMoveInfo[i].heroPosition[_listIndex].curPositionIndex;
                if (_index > _count)
                {
                    heroKeepMoveInfo[i].heroPosition[_listIndex].curPositionIndex = 0;
                }
                break;
            }
        }
    }

    public void TriggerCall_OnTriggerEnter()
    {
        ScenarioMgr.GetInstance().SetScenario(scenarioIndex, subScenarioIndex);
    }

    public void TriggerCall_OnTriggerExit()
    {
        ScenarioMgr.GetInstance().ExitScenarioArea();
    }
}
