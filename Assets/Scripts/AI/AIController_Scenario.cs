using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController_Scenario : AIController_Base
{
    public enum ArriveDelegate { None, Hide }

    public delegate void eventHandler();
    public event eventHandler execEvent;

    Vector3 _arrivalPos;
    Quaternion _targetRot;
    bool bMove = false;
    bool bRotate = false;

    public bool bArrive = false;
    float _smartPathSpeed = 0;
    float _pastTime;
    
    SmartPathTracer pathTracer = new SmartPathTracer();

    public void SetTarget(Vector3 _pos, Quaternion _rot, float _speed, ArriveDelegate arrEvent)
    {
        _pastTime = 0;
        _arrivalPos = _pos;
        _targetRot = _rot;
        bMove = true;
        bArrive = false;
        bRotate = true;

        SmartTargetPath(_pos, _speed);

        if (arrEvent == ArriveDelegate.Hide)
            execEvent += Hide;
    }

    protected void SmartTargetPath(Vector3 destPos, float speed)
    {
        pathTracer.Init(transform, destPos);
        _smartPathSpeed = speed;
        _pastTime = 0;
    }

    public void Hide()
    {
        execEvent -= Hide;
        gameObject.SetActive(false);
    }

    public void ReRotation(float _axis_Y)
    {
        bArrive = false;
        bMove = false;
        bRotate = true;
        _targetRot = Quaternion.Euler(this.transform.localRotation.x, _axis_Y, this.transform.localRotation.z);
    }

    public class SmartPathTracer
    {
        UnityEngine.AI.NavMeshPath _navPath = null;
        int _navPathIndex = 0;
        float _navPathLength = 0;

        Transform _transform = null;

        Vector3 _lastPosition = Vector3.zero;
        Vector3 _lastForward = Vector3.forward;
        public float hero_Radius;

        public float Length
        {
            get
            {
                return _navPathLength;
            }
        }

        public bool bCampingMode = false;


        public void Init(Transform transform, Vector3 goal)
        {
            _transform = transform;

            if (_navPath == null)
            {
                _navPath = new UnityEngine.AI.NavMeshPath();
            }

            _lastPosition = transform.position;
            _lastForward = transform.forward;

            var navStart = _lastPosition;
            var navEnd = goal;


            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(navStart, out navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                navStart = navHit.position;
            }
            if (UnityEngine.AI.NavMesh.SamplePosition(navEnd, out navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                navEnd = navHit.position;
            }

            if (UnityEngine.AI.NavMesh.CalculatePath(navStart, navEnd, UnityEngine.AI.NavMesh.AllAreas, _navPath))
            {
                _navPathLength = 0;
                _navPathIndex = 0;

                for (int i = 1; i < _navPath.corners.Length; ++i)
                {
                    var dist = Vector3.Distance(_navPath.corners[i - 1], _navPath.corners[i]);
                    _navPathLength += dist;
                }
            }
        }

        public void Update()
        {
            if (_navPathIndex < _navPath.corners.Length)
            {
                var wayPoint = _navPath.corners[_navPathIndex];

                var wayDist = UtilFunctions.GetMoveForY(_transform.position, wayPoint).magnitude;

                bool wayPointReached = wayDist <= float.Epsilon || 0 >= Vector3.Dot(UtilFunctions.GetMoveForY(_lastPosition, wayPoint), UtilFunctions.GetMoveForY(_transform.position, wayPoint));

                if (bCampingMode)
                {
                    if (wayPointReached == false)
                    {
                        if (_navPath.corners.Length - _navPathIndex == 1)
                        {
                            if (Vector3.Distance(_transform.position, wayPoint) < hero_Radius)
                            {
                                wayPointReached = true;
                            }
                        }
                    }
                }


                if (wayPointReached)
                {
                    ++_navPathIndex;
                }
            }

            _lastPosition = _transform.position;
            _lastForward = _transform.forward;
        }

        public bool IsReachedGoal()
        {
            return _navPathIndex >= _navPath.corners.Length;
        }

        public Vector3 GetWayPoint()
        {
            var wayPoint = _navPath.corners[Mathf.Min(_navPathIndex, _navPath.corners.Length - 1)];

            return wayPoint;
        }

        public Vector3 GetWayPointDirection()
        {
            var wayPoint = _navPath.corners[Mathf.Min(_navPathIndex, _navPath.corners.Length - 1)];

            return UtilFunctions.GetMoveForY(_transform.position, wayPoint).normalized;
        }

        public float GetRemainDist()
        {
            if (_navPath == null || _navPathIndex >= _navPath.corners.Length)
            {
                return 0.0f;
            }

            var wayPoint = _navPath.corners[_navPathIndex];

            var wayDist = UtilFunctions.GetMoveForY(_transform.position, wayPoint).magnitude;

            float remainDist = wayDist;
            Vector3 origin = _navPath.corners[_navPathIndex];
            for (int i = _navPathIndex + 1; i < _navPath.corners.Length; ++i)
            {
                remainDist += Vector3.Distance(origin, _navPath.corners[i]);
                origin = _navPath.corners[i];
            }
            return remainDist;
        }
    }
    public void StartScenario()
    {
        PAWN.SetSpeed(0);
    }
}
