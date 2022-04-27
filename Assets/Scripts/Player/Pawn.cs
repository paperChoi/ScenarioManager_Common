using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Pawn : MonoBehaviour
{
    public enum JobType
    {
        Melee,
        Dealer,
        Ranger,
        Tanker,
        Healer,
        Supporter,
    }

    public float min_speed = 0.5f;
    public float max_speed = 1.5f;
    public int vnum;
    public JobType jobType;
    public float playerHeight;

    Animator _animator = null;
    CharacterController _cc = null;

    Vector3 _direction = Vector3.forward;
    Vector3 _captured_direction = Vector3.forward;
    Vector3 _desired_direction = Vector3.forward;

    float _rot_speed = 0.5f;
    float _current_rot_t = 0.0f;
    bool _root_motion_rot_mode = false;
    float _speed = 0.0f;

    bool DeadMotion = false;
    bool HitMotion = false;
    bool bSetForcePos = false;
    Vector3 vSetForcePos = Vector3.zero;

    public bool Uncontrollable
    {
        get
        {
            return (DeadMotion || HitMotion);
        }
    }
    public Vector3 FLOOR_POS
    {
        get
        {
            return FloorLoc(transform.position);
        }
    }

    public Vector3 FloorLoc(Vector3 pos, float down_max_dist = 5.0f)
    {
        Vector3 temp_pos = pos;
        temp_pos.y += 1.0f;

        Ray ray = new Ray(temp_pos, Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, down_max_dist) == true)
        {
            return hitInfo.point;
        }

        return pos;
    }

    public void SetSpeed(float speed)
    {
        if (Uncontrollable == false)
        {
            _speed = speed;
            if (hasAnimParam("Speed") == true)
            {
                _animator.SetFloat("Speed", _speed);
            }
        }
    }

    public float GetSpeed()
    {
        return _speed;
    }

    public void SetDirection(Vector3 dir)
    {
        if (Uncontrollable)
            return;

        _direction = dir.normalized;
        _captured_direction = _direction;
        _desired_direction = _direction;
    }

    Dictionary<string, bool> _lookup_animParams = new Dictionary<string, bool>(50);
    bool hasAnimParam(string name)
    {
        if (_animator == null || _animator.isInitialized == false) return false;

        if (_lookup_animParams.ContainsKey(name) == true)
            return _lookup_animParams[name];

        bool bFound = false;
        foreach (var param in _animator.parameters)
        {
            if (param.name == name)
            {
                bFound = true;
                break;
            }
        }

        _lookup_animParams.Add(name, bFound);
        return bFound;
    }

    public void SetPosition(Vector3 pos, bool bCheckOutside = true)
    {
        bSetForcePos = true;
        vSetForcePos = pos;
        this.transform.position = pos;
    }

    public void SetRotation(Vector3 dir)
    {
        this.transform.rotation = Quaternion.LookRotation(dir);
    }

    public void SetRotation(Quaternion dir)
    {
        this.transform.rotation = dir;
    }
}
