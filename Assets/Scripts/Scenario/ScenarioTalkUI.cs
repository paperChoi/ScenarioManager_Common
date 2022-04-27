using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioTalkUI : MonoBehaviour
{
    public enum TALKUITYPE
    {
        LEFT = 0,
        RIGHT = 1,
        UP = 2,
        DOWN = 3
    }

    public enum UIEVENTTYPE
    {
        NONE = 0,
        BUTTON = 1,
        TIME = 2,
    }

    public Image talkBG;
    public Text talkText;

    public GameObject talkBG_Left;
    public GameObject talkBG_Right;

    public Vector2 uiAddPos = new Vector2(0.31f, 2.3f);
    public Vector2 uiLeftPos = new Vector2(0.31f, 2.3f);
    public Vector2 uiRightPos = new Vector2(0.31f, 2.3f);
    Vector2 _uiAddPos = Vector2.zero;

    public GameObject targetUI;
    TALKUITYPE curType;
    UIEVENTTYPE uiEventType;
    ScenarioTalkMgr.TALKTYPE talkType;
    public float textShowTime = 2.0f;
    float curTime = 0.0f;
    public int emoticonIndex = -1;

    float _bgWidth;
    float _bgHeight;

    public void Init(TALKUITYPE uiType, string _text, Vector3 _pos, Vector2 _addUIPos, UIEVENTTYPE eventTYPE, ScenarioTalkMgr.TALKTYPE _talkType)
    {
        curTime = 0.0f;
        uiEventType = eventTYPE;
        talkType = _talkType;

        if (uiEventType == UIEVENTTYPE.BUTTON)
            GetComponent<Button>().enabled = true;
        else if (uiEventType == UIEVENTTYPE.TIME)
            GetComponent<Button>().enabled = false;

        curType = uiType;
        switch (uiType)
        {
            case TALKUITYPE.LEFT:
                targetUI = talkBG_Left;
                break;
            case TALKUITYPE.RIGHT:
                targetUI = talkBG_Right;
                break;
            default:
                targetUI = talkBG_Right;
                break;
        }

        _text = _text.Replace("\\n", "\r\n");

        var imageBG = targetUI.GetComponentInChildren<Image>();
        var uiText = targetUI.GetComponentInChildren<Text>();
        uiText.text = _text;
        _bgWidth = uiText.preferredWidth + 60;
        float fontWidth = ((_bgWidth - 60) > 0) ? _bgWidth - 60 : 100;
        float fontHeight = Mathf.Min(uiText.preferredHeight, 116);

        _bgHeight = uiText.preferredHeight + 40;
        _bgHeight = Mathf.Max(_bgHeight, 90);
        _bgHeight = Mathf.Min(_bgHeight, 160);
        uiText.rectTransform.sizeDelta = new Vector2(fontWidth, fontHeight);
        imageBG.rectTransform.sizeDelta = new Vector2(_bgWidth, _bgHeight);

        if (curType == TALKUITYPE.LEFT)
            _uiAddPos = uiLeftPos;
        else if (curType == TALKUITYPE.RIGHT)
            _uiAddPos = uiRightPos;
        else
            _uiAddPos = uiAddPos;


        _pos.x += _uiAddPos.x;
        _pos.y += _uiAddPos.y;
        Vector3 uiPos = UIManager.instance.GetCanvasPos(_pos);

        if (_addUIPos.Equals(Vector2.zero) == false)
        {
            uiPos.x += _addUIPos.x;
            uiPos.y += _addUIPos.y;
        }

        Vector3 correctionPos = uiPos;

        switch (uiType)
        {
            case TALKUITYPE.LEFT:
                correctionPos.x -= _bgWidth;
                break;
            case TALKUITYPE.RIGHT:
                correctionPos.x += _bgWidth;
                break;
        }

        correctionPos = SetPositionCorrectionIsCamera(correctionPos, _bgHeight);
        uiPos += correctionPos;
        targetUI.transform.localPosition = uiPos;
    }

    Vector2 SetPositionCorrectionIsCamera(Vector3 pos, float height)
    {
        Vector2 correctionPos = Vector2.zero;
        if (pos.x < 0)
        {
            if (-UIManager.instance.canvasResolution_X > pos.x)
                correctionPos.x = -(UIManager.instance.canvasResolution_X + pos.x);
        }
        else if (pos.x > 0)
        {
            if (UIManager.instance.canvasResolution_X < pos.x)
                correctionPos.x = (pos.x - UIManager.instance.canvasResolution_X);
        }

        if (pos.y > UIManager.instance.canvasResolution_Y)
        {
            correctionPos.y = -(pos.y - UIManager.instance.canvasResolution_Y);
        }
        else if (pos.y - height < -UIManager.instance.canvasResolution_Y)
        {
            correctionPos.y = -((pos.y - height) + UIManager.instance.canvasResolution_Y);
        }

        return correctionPos;
    }

    public void UpdatePosition(Vector3 _pos, Vector2 _addUIPos)
    {
        if (uiEventType == UIEVENTTYPE.TIME)
        {
            curTime += Time.deltaTime;
            if (curTime > textShowTime)
            {
                if (talkType == ScenarioTalkMgr.TALKTYPE.TALK)
                    gameObject.SetActive(false);

                uiEventType = UIEVENTTYPE.NONE;
            }
        }
        
        if (curType == TALKUITYPE.LEFT)
        {
            _uiAddPos = uiLeftPos;
        }
        else if (curType == TALKUITYPE.RIGHT)
        {
            _uiAddPos = uiRightPos;
        }
        else
        {
            _uiAddPos = uiAddPos;
        }

        float addPosX = _addUIPos.x + _uiAddPos.x;
        float addPosY = _addUIPos.y + _uiAddPos.y;

        _pos.x += addPosX;
        _pos.y += addPosY;

        Vector3 uiPos = UIManager.instance.GetCanvasPos(_pos);
        Vector3 correctionPos = uiPos;

        switch (curType)
        {
            case TALKUITYPE.LEFT:
                correctionPos.x -= _bgWidth;
                break;
            case TALKUITYPE.RIGHT:
                correctionPos.x += _bgWidth;
                break;
        }

        correctionPos = SetPositionCorrectionIsCamera(correctionPos, _bgHeight);
        uiPos += correctionPos;
        if (targetUI)
        {
            targetUI.transform.localPosition = uiPos;
        }
    }
}
