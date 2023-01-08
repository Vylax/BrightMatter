using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Point = LightMap.Point;

public class Player : MonoBehaviour
{
    Collider m_Collider;
    Vector3 m_Center;
    Vector3 m_Size, m_Min, m_Max;

    Vector3[] checkPoints;
    [SerializeField] [Range(3, 50)] private int checkPointsCount = 12;
    [SerializeField] private float checkPointsDistFromCenter = 0.05f;

    private int litUpPointsCount = 0;

    public bool isInLight = false;

    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
    }

    private void Update()
    {
        UpdateLightStatus();
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 200, 100), $"In Light: {isInLight}\nPoints In Light:{litUpPointsCount}");
    }

    private void OnDrawGizmos()
    {
        try
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(playerFeetPosition, .01f);
            for (int i = 0; i < checkPointsCount; i++)
            {
                Gizmos.DrawSphere(checkPoints[i], .01f);
            }
        }
        catch
        {

        }
    }

    private void UpdateCheckpointsPositions()
    {
        checkPoints = new Vector3[checkPointsCount];
        for (int i = 0; i < checkPointsCount; i++)
        {
            checkPoints[i] = CheckPointPositionFromId(i);
        }
    }

    private void UpdateLightStatus()
    {
        UpdateCheckpointsPositions();

        litUpPointsCount = 0;

        foreach (Vector3 pos in checkPoints)
        {
            Point point = LightMap.GetClosestPointOnMap(pos);
            litUpPointsCount += (point != null && point.isInLight) ? 1 : 0;
        }

        isInLight = litUpPointsCount > 0;
    }

    public Vector3 playerFeetPosition => transform.position - Vector3.up * m_Collider.bounds.extents.y;

    private Vector3 CheckPointPositionFromId(int id)
    {
        float angle = 360f / checkPointsCount * id * Mathf.Deg2Rad;
        return playerFeetPosition + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * checkPointsDistFromCenter;
    }
}
