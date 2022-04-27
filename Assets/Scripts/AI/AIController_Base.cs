using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController_Base : Controller
{
    //길찾기 관련 내용
    protected NavMeshPath _path_current = null;
    protected bool _path_finding_successed = false;
    protected bool _path_finding_end = false;
    protected int _path_current_idx = 0;
    protected Vector3 _path_target_pos = Vector3.zero;
    protected GameObject _path_target_object = null;
    protected float _path_last_tracking_target_t = 0.0f;
    protected float _path_refresh_tracking_target_length_t = 0.0f;
    protected float _path_reach_tolerance = 0.5f;
    protected float _path_initial_speed = 0.0f;

    //목표 지점을 좌표로 셋팅하여 길찾기
    public virtual bool SetTargetPath(Vector3 target, float initial_speed, float target_reach_tolerance = 0.5f, int specificLayer = -1)
    {
        _path_target_object = null;
        _path_current_idx = 0;
        _path_last_tracking_target_t = Time.time;
        _path_target_pos = target;
        _path_reach_tolerance = target_reach_tolerance;
        _path_initial_speed = initial_speed;
        if (_path_current == null)
            _path_current = new NavMeshPath();

        bool bResult = false;
        if (specificLayer == -1)
        {
            bResult = NavMesh.CalculatePath(PAWN.FLOOR_POS, UtilFunctions.FloorLoc(target), 1, _path_current);
        }
        else
        {
            bResult = NavMesh.CalculatePath(PAWN.FLOOR_POS, UtilFunctions.FloorLocLayerMask(target, specificLayer), 1, _path_current);
        }

        _path_finding_successed = bResult;
        PAWN.SetSpeed(initial_speed);
        return bResult;
    }

    //목표 타겟을 게임 오브젝트로 셋팅
    public virtual bool SetTargetPath(GameObject go, float initial_speed, float refresh_tracking = 1.0f, float target_reach_tolerance = 0.5f)
    {
        if (go == null) return false;

        _path_refresh_tracking_target_length_t = refresh_tracking;
        _path_last_tracking_target_t = Time.time;
        bool bResult = SetTargetPath(go.transform.position, initial_speed, target_reach_tolerance);
        _path_target_object = go;
        PAWN.SetSpeed(initial_speed);
        _path_initial_speed = initial_speed;
        return bResult;
    }

    //길찾기 취소
    public virtual void CancelTargetPath()
    {
        _path_target_object = null;
        _path_current_idx = 0;
        _path_last_tracking_target_t = 0.0f;
        _path_current = null;
        _path_target_pos = Vector3.zero;
    }

    //길찾기 이동 구현(리턴: 도착 여부)
    protected virtual bool UpdatePath()
    {
        if (_path_current == null) return false;

        var corners = _path_current.corners;

        if (corners == null || corners.Length == 0) return false;
        if (_path_finding_successed == false) return false;

        //타겟 패스 추적을 리프레쉬해야하는지 체크 및 리프레쉬
        float tracking_t = Time.time - _path_last_tracking_target_t;

        if (_path_target_object != null && tracking_t > _path_refresh_tracking_target_length_t)
        {
            SetTargetPath(_path_target_object, _path_initial_speed, _path_refresh_tracking_target_length_t);
            _path_last_tracking_target_t = Time.time;
            return false;
        }

        if (corners.Length <= _path_current_idx) return true;   //모든 패스를 돌았다면

        var dest_pos = corners[_path_current_idx];
        if (UpdateMoveTo(dest_pos) == true)
        {
            ++_path_current_idx;
        }

        return false;
    }

    //해당 지점까지 이동 업데이트
    Vector3 _prev_position = Vector3.zero;
    protected bool UpdateMoveTo(Vector3 target, float tolerance = 0.5f)
    {
        var moved_vec = PAWN.transform.position - _prev_position;
        var prev_vec = target - _prev_position;

        //타겟보다 크게 움직였다면 도착지점에 도달한것으로 판단(우선 보정은 안함)
        bool bReached = false;
        if (moved_vec.magnitude >= prev_vec.magnitude)
        {
            bReached = true;
        }

        _prev_position = transform.position;

        if (bReached == false)
        {
            var desired_dir = UtilFunctions.GetMoveForY(transform.position, target).normalized;
            if (desired_dir.magnitude > 0)
            {
                var q2 = Quaternion.LookRotation(desired_dir);
                var q1 = Quaternion.LookRotation(transform.forward);
                var rq = Quaternion.Lerp(q1, q2, 0.5f);
                var result_dir = q2 * Vector3.forward;
                PAWN.SetDirection(result_dir);
            }
            return false;
        }

        return true;
    }
}
