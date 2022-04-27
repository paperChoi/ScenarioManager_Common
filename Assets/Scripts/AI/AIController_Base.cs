using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController_Base : Controller
{
    //��ã�� ���� ����
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

    //��ǥ ������ ��ǥ�� �����Ͽ� ��ã��
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

    //��ǥ Ÿ���� ���� ������Ʈ�� ����
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

    //��ã�� ���
    public virtual void CancelTargetPath()
    {
        _path_target_object = null;
        _path_current_idx = 0;
        _path_last_tracking_target_t = 0.0f;
        _path_current = null;
        _path_target_pos = Vector3.zero;
    }

    //��ã�� �̵� ����(����: ���� ����)
    protected virtual bool UpdatePath()
    {
        if (_path_current == null) return false;

        var corners = _path_current.corners;

        if (corners == null || corners.Length == 0) return false;
        if (_path_finding_successed == false) return false;

        //Ÿ�� �н� ������ ���������ؾ��ϴ��� üũ �� ��������
        float tracking_t = Time.time - _path_last_tracking_target_t;

        if (_path_target_object != null && tracking_t > _path_refresh_tracking_target_length_t)
        {
            SetTargetPath(_path_target_object, _path_initial_speed, _path_refresh_tracking_target_length_t);
            _path_last_tracking_target_t = Time.time;
            return false;
        }

        if (corners.Length <= _path_current_idx) return true;   //��� �н��� ���Ҵٸ�

        var dest_pos = corners[_path_current_idx];
        if (UpdateMoveTo(dest_pos) == true)
        {
            ++_path_current_idx;
        }

        return false;
    }

    //�ش� �������� �̵� ������Ʈ
    Vector3 _prev_position = Vector3.zero;
    protected bool UpdateMoveTo(Vector3 target, float tolerance = 0.5f)
    {
        var moved_vec = PAWN.transform.position - _prev_position;
        var prev_vec = target - _prev_position;

        //Ÿ�ٺ��� ũ�� �������ٸ� ���������� �����Ѱ����� �Ǵ�(�켱 ������ ����)
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
