// Draw lines to the connected game objects that a script has.
// If the target object doesnt have any game objects attached
// then it draws a line from the object to (0, 0, 0).

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConnectedObjects))]
class ConnectLineHandle : Editor
{
    float minDist = float.MaxValue;
    float maxDist = 0;
    float totDist = 0;
    float avgDist = 0;

    void OnSceneGUI()
    {
        ConnectedObjects connectedObjects = target as ConnectedObjects;
        if (connectedObjects.objs == null)
            return;
        Vector3 ArrowLeft = Vector3.zero;
        Vector3 ArrowRight = Vector3.zero;
        Handles.color = Color.green;
        Handles.SphereHandleCap(0, connectedObjects.objs[connectedObjects.objs.Length-1].transform.position, Quaternion.identity, 2, EventType.Repaint);
        Handles.color = Color.blue;
        Handles.SphereHandleCap(0, connectedObjects.objs[0].transform.position, Quaternion.identity, 2, EventType.Repaint);
        GameObject dummy = new GameObject();
        Handles.color = connectedObjects.drawColor;
        minDist = float.MaxValue;
        maxDist = 0;
        totDist = 0;
        for (int i = 0; i < connectedObjects.objs.Length-1; i++)
        {
            GameObject connectedObject1 = connectedObjects.objs[i];
            GameObject connectedObject2 = connectedObjects.objs[i + 1];
            dummy.transform.position = connectedObject2.transform.position;
            dummy.transform.LookAt(connectedObject1.transform);
            dummy.transform.Rotate(0, 180, 0, Space.Self);
            float distance = Vector3.Distance(connectedObject1.transform.position, connectedObject2.transform.position);
            if (connectedObjects.arroHeadSize > 0)
            {
                float arrowHeadAngle = 60;
                Vector3 direction = connectedObject2.transform.position - connectedObject1.transform.position;
                float size = connectedObjects.arroHeadSize;
                if (size > distance)
                    size = distance;
                var angle = 180 - (arrowHeadAngle / 2);
                var dist = size * Mathf.Sin(Mathf.Rad2Deg * (90 - arrowHeadAngle / 2));
                ArrowLeft = new Ray(connectedObject2.transform.position, Quaternion.AngleAxis(-angle, dummy.transform.up) * direction).GetPoint(dist);
                ArrowRight = new Ray(connectedObject2.transform.position, Quaternion.AngleAxis(angle, dummy.transform.up) * direction).GetPoint(dist);
            }
            if (connectedObject1 && connectedObject2)
            {
                Handles.DrawLine(connectedObject1.transform.position, connectedObject2.transform.position);
                if (connectedObjects.arroHeadSize > 0)
                {
                    Handles.DrawLine(connectedObject2.transform.position, ArrowLeft);
                    Handles.DrawLine(connectedObject2.transform.position, ArrowRight);
                }
            }
            if (distance < minDist)
                minDist = distance;
            if(distance > maxDist)
                maxDist = distance;
            totDist += distance;
            avgDist = totDist / connectedObjects.objs.Length;
        }
        DestroyImmediate(dummy);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Label("Avg Objects Dist: " + avgDist);
        GUILayout.Label("Min Objects Dist: " + minDist);
        GUILayout.Label("Max Objects Dist: " + maxDist);
        GUILayout.Label("Tot Objects Dist: " + totDist);
    }
}
