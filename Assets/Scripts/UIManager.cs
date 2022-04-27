using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager _Instance = null;

    public static UIManager instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType(typeof(UIManager)) as UIManager;
                Debug.LogWarning("No instance UIManager");
            }
            return _Instance;
        }
    }

    Canvas _canvas;
    public float canvasResolution_X { get; private set; }
    public float canvasResolution_Y { get; private set; }

    public GameObject minimap;
    public ScenarioTalkUI scenarioTalkUI;

    public void SetScenario()
    {
        minimap.SetActive(true);
    }

    public bool IsMiniInventoryOpen()
    {
        return minimap.activeSelf;
    }
    
    public Vector3 GetCanvasPos(Vector3 pos)
    {
        if (_canvas == null)
            _canvas = this.gameObject.GetComponent<Canvas>();

        Vector3 vecPos = UtilFunctions.WorldToCanvas(pos, _canvas);
        return vecPos;
    }

    public void SetTalkUI(bool _flag, float autoCloseTime = 0)
    {
        if (scenarioTalkUI.gameObject.activeSelf != _flag)
            scenarioTalkUI.gameObject.SetActive(_flag);

        if (scenarioTalkUI.targetUI)
            scenarioTalkUI.targetUI.SetActive(_flag);

        if (_flag && 0 < autoCloseTime)
        {
            Invoke(nameof(OnScenarioNextTalk), autoCloseTime);
        }
    }

    public void OnScenarioNextTalk()
    {
        Debug.Log(nameof(OnScenarioNextTalk));
        if (ScenarioMgr.GetInstance().triggerMgr)
            StartCoroutine(ScenarioMgr.GetInstance().OnNextTalkIE());

        CancelInvoke(nameof(OnScenarioNextTalk));
    }
}
