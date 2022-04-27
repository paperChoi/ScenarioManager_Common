using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    public float maxDist = 100.0f;

    [HideInInspector]
    public bool isPlay = true;

    Vector3 _input_first;
    Vector3 _last_touch;
    bool _bTouchBegan = false;
    bool clickInput = false;
    HashSet<GameObject> _receivers = new HashSet<GameObject>();
    public GameObject directionPlanePrefab;
    public GameObject directionPlane
    {
        get;
        private set;
    }

    //!!temp edited

    public GameObject directionPlaneArrow
    {
        get;
        private set;
    }

    public GameObject directionPlaneArrowTail
    {
        get;
        private set;
    }

    public Vector3 Direction
    {
        get;
        set;
    }

    public float Weight
    {
        get;
        set;
    }

    public bool IsTouch
    {
        get
        {
            return _bTouchBegan;
        }
    }

    public bool IsControll
    {
        get
        {
            return isPlay == true && clickInput == true;
        }
    }

    public Vector3 First
    {
        get
        {
            return _input_first;
        }
    }

    public Vector3 Last
    {
        get
        {
            return _last_touch;
        }
    }

    public PlayerController OwnerPC
    {
        get
        {
            PlayerController owner = null;
            foreach (var recv in _receivers)
            {
                var pc = recv.GetComponent<PlayerController>();
                if (pc != null)
                {
                    owner = pc;
                    break;
                }
            }
            return owner;
        }

    }


    private void Awake()
    {
        if (directionPlanePrefab != null)
        {
            directionPlane = Instantiate(directionPlanePrefab, transform);
            directionPlane.transform.localPosition = Vector3.up * 0.006f;
        }
    }

    bool bKeyPushed = false;

    private void Update()
    {
        bool bDisable = ScenarioMgr.GetInstance().isScenario;
        bDisable |= GameInfo.instance.IsBattleMode();
        bDisable |= UIManager.instance.IsMiniInventoryOpen();

        if (bDisable)
        {
            isPlay = false;
            clickInput = false;
            _bTouchBegan = false;
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPlay = true;
            clickInput = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            clickInput = true;
        }

        if (clickInput && isPlay)
        {
            bool bPressed = Input.GetMouseButton(0);
            if (bPressed == true)
            {
                if (_bTouchBegan == false)
                {
                    _bTouchBegan = true;
                    _input_first = Input.mousePosition;
                    _last_touch = _input_first;
                }
                else
                {
                    if (_last_touch != Input.mousePosition)
                    {
                        Vector2 direction = (Input.mousePosition - _input_first).normalized;
                        float dist = (Input.mousePosition - _input_first).magnitude / (Screen.height / 720.0f);
                        float weight = Mathf.Min(1.0f, dist / maxDist);

                        var lookAt = Camera.main.transform.right * direction.x + Vector3.Cross(Camera.main.transform.right, Vector3.up) * direction.y;
                        direction.x = lookAt.x;
                        direction.y = lookAt.z;
                        direction = direction.normalized;

                        _last_touch = Input.mousePosition;

                        SendInputMessage(direction, weight);
                    }
                }
            }
            else
            {
                _bTouchBegan = false;
                SendInputMessage(Vector3.zero, 0.0f);
            }
        }
        else
        {
            _bTouchBegan = false;
            SendInputMessage(Vector3.zero, 0.0f);
        }

        // Pawn re-positioning.
        if (Input.GetKeyDown("space"))
        {
            foreach (var rec in _receivers)
            {
                RaycastHit hit;
                if (Physics.Raycast(rec.transform.position + Vector3.up * 1000, Vector3.down, out hit))
                {
                    rec.transform.position = hit.point + Vector3.up * 1;
                }
            }
        }
        var input_v = Input.GetAxis("Vertical");
        var input_h = Input.GetAxis("Horizontal");

        if (Mathf.Abs(input_v) > float.Epsilon || Mathf.Abs(input_h) > float.Epsilon)
        {
            Vector2 inputVector = new Vector2(input_h, input_v);
            Vector2 direction = inputVector.normalized;
            float weight = inputVector.magnitude;

            var lookAt = Camera.main.transform.right * direction.x + Vector3.Cross(Camera.main.transform.right, Vector3.up) * direction.y;
            direction.x = lookAt.x;
            direction.y = lookAt.z;
            direction = direction.normalized;

            SendInputMessage(direction, weight);
        }
    }

    public void AttachInputReceiver(GameObject obj)
    {
        if (_receivers.Contains(obj) == false)
        {
            _receivers.Add(obj);
        }
    }

    public void DetechInputReceiver(GameObject obj)
    {
        if (_receivers.Contains(obj) == true)
        {
            _receivers.Remove(obj);
        }
    }

    void SendInputMessage(Vector2 direction, float weight)
    {
        Direction = direction;
        Weight = weight;

        foreach (var rec in _receivers)
        {
            rec.SendMessage("OnInput", SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnEnable()
    {
        if (directionPlane)
        {
            directionPlane.SetActive(true);
        }
    }

    void OnDisable()
    {
        if (directionPlane)
        {
            directionPlane.SetActive(false);
        }
    }
}
