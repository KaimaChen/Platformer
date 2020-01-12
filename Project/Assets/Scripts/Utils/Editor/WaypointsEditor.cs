/*
 * 操作：
 * 1. 点击鼠标左键：添加点
 * 2. 拖动鼠标左键：移动选中的点
 * 3. Ctrl + 鼠标左键：移除点
 * 4. Shift + 拖动鼠标左键：将选中点吸附到前后两点所在水平和竖直线上
 * 
 * 说明：
 * 1. 起点和终点以虚线相连
 */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Waypoints))]
public class WaypointsEditor : Editor
{
    private readonly Color c_FirstPointColor = new Color(1, 0, 0); //第一个点的颜色
    private readonly Color c_NormalColor = new Color(1, 1, 0); //其他点和线的颜色

    private const float c_NormalPointRadius = 0.05f; //普通状态下点的半径
    private const float c_EditingPointRadius = 0.15f; //编辑状态下点的半径
    private const float c_SnapDistance = 0.15f; //吸附距离

    private const string c_StartEditWord = "开始编辑";
    private const string c_CancelEditWord = "取消编辑";

    private Waypoints m_Target;
    private bool m_IsEditing;
    private bool m_NeedRepaint;

    private int m_SelectedPoint = -1;
    private int m_SelectedLine = -1;

    private void OnEnable()
    {
        m_Target = target as Waypoints;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        string content = m_IsEditing ? c_CancelEditWord : c_StartEditWord;
        if (GUILayout.Button(content))
            m_IsEditing = !m_IsEditing;
    }

    private void OnSceneGUI()
    {
        Event e = Event.current;

        if(m_IsEditing)
        {
            Tools.current = Tool.None;

            if (e.type == EventType.Repaint)
                DrawWaypoints();
            else if (e.type == EventType.Layout)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); //不能选中Scene场景中的东西
            else
                HandleInput();
        }
        else
        {
            if (e.type == EventType.Repaint)
                DrawWaypoints();
        }
    }

    private void DrawWaypoints()
    {
        Color originColor = Handles.color;

        List<Vector2> points = m_Target.Points;
        for(int i = 0; i < points.Count; i++)
        {
            //画点
            Color color = (i == 0 ? c_FirstPointColor : c_NormalColor);
            color.a = (i == m_SelectedPoint ? 1 : 0.2f);
            Handles.color = color;
            float radius = (m_IsEditing ? c_EditingPointRadius : c_NormalPointRadius);
            Handles.DrawSolidDisc(points[i], -Vector3.forward, radius);

            //画线
            color = c_NormalColor;
            color.a = (i == m_SelectedLine ? 1 : 0.2f);
            Handles.color = color;
            if(points.Count > 1)
            {
                if (i + 1 < m_Target.Count)
                    Handles.DrawLine(points[i], points[i + 1]);
                else
                    Handles.DrawDottedLine(points[i], points[0], 4); //起点和终点用虚线连接起来
            }
        }

        Handles.color = originColor;
    }

    private void HandleInput()
    {
        Event e = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;

        UpdateSelection(e, mousePos);

        if (e.button == 0)
        {
            switch(e.type)
            {
                case EventType.MouseDown:
                    HandleMouseDown(e, mousePos);
                    break;
                case EventType.MouseUp:
                    HandleMouseUp();
                    break;
                case EventType.MouseDrag:
                    HandleMouseDrag(e, mousePos);
                    break;
            }
        }

        if(m_NeedRepaint)
        {
            HandleUtility.Repaint();
            m_NeedRepaint = false;
        }
    }

    private void HandleMouseDown(Event e, Vector2 mousePos)
    {
        bool delete = e.modifiers == EventModifiers.Control;

        if(delete)
        {
            CheckDeletePoint();
        }
        else
        {
            CheckAddPoint(mousePos);
        }
    }

    private void CheckAddPoint(Vector2 mousePos)
    {
        if(m_SelectedPoint < 0)
        {
            int index = (m_SelectedLine >= 0 ? m_SelectedLine + 1 : m_Target.Count);

            Undo.RecordObject(m_Target, "Add Waypoint");
            m_Target.Points.Insert(index, mousePos);
            m_SelectedPoint = index;
        }
    }

    private void CheckDeletePoint()
    {
        if(m_SelectedPoint >= 0)
        {
            Undo.RecordObject(m_Target, "Remove Waypoint");
            m_Target.Points.RemoveAt(m_SelectedPoint);
            m_SelectedPoint = -1;
        }
    }

    private void HandleMouseUp()
    {
        m_SelectedPoint = -1;
        m_SelectedLine = -1;
    }

    private void HandleMouseDrag(Event e, Vector2 mousePos)
    {
        if(m_SelectedPoint >= 0)
        {
            Vector2 targetPos = mousePos;

            bool snap = (e.modifiers == EventModifiers.Shift);
            if (snap)
            {
                int prevIndex = (m_SelectedPoint - 1) % m_Target.Count;
                Vector2 prevPt = m_Target.GetPoint(prevIndex);
                int nextIndex = (m_SelectedPoint + 1) % m_Target.Count;
                Vector2 nextPt = m_Target.GetPoint(nextIndex);
                targetPos = SnapPosition(prevPt, nextPt, mousePos);
            }

            Undo.RecordObject(m_Target, "Drag Waypoint");
            m_Target.Points[m_SelectedPoint] = targetPos;
            m_NeedRepaint = true;
        }
    }

    private void UpdateSelection(Event e, Vector2 mousePos)
    {
        List<Vector2> points = m_Target.Points;

        bool isDraging = (e.button == 0 && e.type == EventType.MouseDrag);
        if(isDraging == false || m_SelectedPoint < 0)
        {
            int pointIndex = -1;

            for (int i = 0; i < points.Count; i++)
            {
                if (Vector2.Distance(points[i], mousePos) <= c_EditingPointRadius)
                {
                    pointIndex = i;
                    break;
                }
            }

            if (m_SelectedPoint != pointIndex)
            {
                m_SelectedPoint = pointIndex;
                m_NeedRepaint = true;
            }
        }

        //点和线的选择状态是互斥的
        if (m_SelectedPoint >= 0)
        {
            m_SelectedLine = -1;
        }
        else
        {
            int lineIndex = -1;
            float cloestDist = c_SnapDistance;
            for(int i = 0; i < points.Count; i++)
            {
                Vector2 curtPt = points[i];
                Vector2 nextPt = points[(i + 1) % points.Count];
                float dist = HandleUtility.DistancePointToLineSegment(mousePos, curtPt, nextPt);

                if(dist <= cloestDist)
                {
                    lineIndex = i;
                    cloestDist = dist;
                }
            }

            if(m_SelectedLine != lineIndex)
            {
                m_SelectedLine = lineIndex;
                m_NeedRepaint = true;
            }
        }
    }

    private Vector2 SnapPosition(Vector2 prevPt, Vector2 nextPt, Vector2 mousePos)
    {
        Vector2 targetPos = mousePos;

        float prevDelta = Mathf.Abs(mousePos.x - prevPt.x);
        float nextDelta = Mathf.Abs(mousePos.x - nextPt.x);
        if(prevDelta < nextDelta)
        {
            if (prevDelta < c_SnapDistance)
                targetPos.x = prevPt.x;
        }
        else
        {
            if (nextDelta < c_SnapDistance)
                targetPos.x = nextPt.x;
        }

        prevDelta = Mathf.Abs(mousePos.y - prevPt.y);
        nextDelta = Mathf.Abs(mousePos.y - nextPt.y);
        if(prevDelta < nextDelta)
        {
            if (prevDelta < c_SnapDistance)
                targetPos.y = prevPt.y;
        }
        else
        {
            if (nextDelta < c_SnapDistance)
                targetPos.y = nextPt.y;
        }

        return targetPos;
    }
}
