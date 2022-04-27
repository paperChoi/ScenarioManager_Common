using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEventObject : MonoBehaviour
{
    [System.Serializable]
    public struct TriggerEventOBJ
    {
        public int dungeonProgressIndex;
        public ScenarioEventMove scenarioEventMove;
        public GameObject LinkedTrigger;
    }

    public enum ScenarioConditionType
    {
        COMPLETE,
        INCOMPLETE,
    }

    [System.Serializable]
    public struct ScenarioCondition
    {
        public ScenarioConditionType Condition;
        public int SceneIndex;
    }

    public ScenarioCondition[] ScenarioConditions;

    bool IsAvail()
    {
        return true;
    }

    bool CheckScenarioCondition()
    {
        if (ScenarioConditions != null && 0 < ScenarioConditions.Length)
        {
            for (int i = 0; i < ScenarioConditions.Length; ++i)
            {
                var condition = ScenarioConditions[i];
                switch (condition.Condition)
                {
                    case ScenarioConditionType.COMPLETE:
                        if (false == ScenarioMgr.GetInstance().CheckCompletedScenario(condition.SceneIndex))
                        {
                            return false;
                        }
                        break;
                    case ScenarioConditionType.INCOMPLETE:
                        if (true == ScenarioMgr.GetInstance().CheckCompletedScenario(condition.SceneIndex))
                        {
                            return false;
                        }
                        break;
                }
            }
            return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsAvail() == false)
            return;

        if (ScenarioMgr.GetInstance().isScenario)
            return;

        int maskLayer = LayerMask.NameToLayer("Player");
        if (other.gameObject.layer != maskLayer) 
            return;

        if (CheckScenarioCondition())
        {
            var scenarioEventMove = GetComponent<ScenarioEventMove>();
            if (scenarioEventMove)
            {
                scenarioEventMove.TriggerCall_OnTriggerEnter();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsAvail() == false) return;

        if (ScenarioMgr.GetInstance().isScenario) 
            return;

        int maskLayer = LayerMask.NameToLayer("Player");
        if (other.gameObject.layer != maskLayer) 
            return;

        if (CheckScenarioCondition())
        {
            var scenarioEventMove = GetComponent<ScenarioEventMove>();
            if (scenarioEventMove)
            {
                scenarioEventMove.TriggerCall_OnTriggerExit();
            }
        }
    }
}
