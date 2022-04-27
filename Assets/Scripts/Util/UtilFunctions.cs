using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class UtilFunctions
{
    static public void DrawGizmoCircle(Vector3 center, float radiusX, float radiusZ)
    {
#if UNITY_EDITOR
        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 z = new Vector3(0, 0, 0);
        float segments = Mathf.Max(0.1f, Mathf.Pow(0.5f, Mathf.Max(1, Mathf.Max(radiusX, radiusZ))));

        for (float i = 0; i < Mathf.PI * 2; i += segments)
        {
            Vector3 p1 = new Vector3(Mathf.Cos(i) * radiusX, 0, Mathf.Sin(i) * radiusZ);

            if (i != 0)
            {
                Gizmos.DrawLine(p0 + center, p1 + center);
            }
            else z = p1;
            p0 = p1;
        }
        Gizmos.DrawLine(p0 + center, z + center);
#endif
    }

    static public Vector3 FloorLoc(Vector3 pos, float down_max_dist = 5.0f)
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

    static public Vector3 FloorLocLayerMask(Vector3 pos, int specificLayer, float down_max_dist = 5.0f)
    {
        Vector3 temp_pos = pos;
        temp_pos.y += 1.0f;

        Ray ray = new Ray(temp_pos, Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, down_max_dist, specificLayer) == true)
        {
            return hitInfo.point;
        }

        return pos;
    }

    static public Vector3 GetMoveForY(Vector3 src, Vector3 target)
    {
        src.y = 0;
        target.y = 0;
        return target - src;
    }

    static public Vector3 WorldToCanvas(Vector3 pos, Canvas canv, Camera cam = null)
    {
        if (cam == null)
        {
            if (GameInfo.instance.IsBattleMode() == false)
            {
                cam = ScenarioMgr.GetInstance() != null ? ScenarioMgr.GetInstance().GetScenarioCamera() : Camera.main;
            }
            else
                cam = Camera.main;
        }

        var viewport_position = cam.WorldToViewportPoint(pos);
        var canvas_rect = canv.GetComponent<RectTransform>();

        return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
                           (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
    }
}
