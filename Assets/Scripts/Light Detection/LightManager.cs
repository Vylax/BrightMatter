using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Point = LightMap.Point;

public class LightManager : MonoBehaviour
{
    bool mapInitialized = false;
    public LightMap map;

    public bool drawLightMap = false;
    public bool highRes = false;

    public Vector3 min= new Vector3(-5, -1.5f, -5);
    public Vector3 max= new Vector3(5, 1f, 5);

    public bool updateLightMap = false;

    public Vector3Int[] checkLogs = new Vector3Int[300];

    private void Start()
    {
        map = new LightMap(min, max, highRes);
        mapInitialized = true;
        //UpdateCheckLogs(true);
    }

    private void Update()
    {
        if (updateLightMap)
        {
            map.UpdateLightMap();
            //UpdateCheckLogs();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(min, 1f);
        Gizmos.DrawSphere(max, 1f);
    }

    private void OnDrawGizmos()
    {
        if (drawLightMap && mapInitialized)
        {
            try
            {
                Vector3 playerPos = GameObject.Find("Player").transform.position;
                //Point point = LightMap.GetClosestPointOnMap(playerPos);
                for (int x = 0; x < 100; x++)
                {
                    for (int y = 0; y < 100; y++)
                    {
                        for (int z = 0; z < 100; z++)
                        {
                            if(Point.IsWithinMapBounds(new Vector3Int(x, y, z)))
                            {
                                Point point = map.points[x, y, z];
                                if (point != null && point.isInLight)
                                {
                                    Gizmos.color = point.isInLight ? Color.yellow : Color.black;
                                    Gizmos.DrawSphere(point.Position, map.step.x / 10f);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        /*if(LightMap.pointsToCheck.Count > 0)
        {
            Gizmos.color = Color.red;
            foreach (Point point in LightMap.pointsToCheck)
            {
                Gizmos.DrawSphere(point.Position, 0.05f);
            }
        }*/
    }

    private void UpdateCheckLogs(bool firstTime = false)
    {
        if (firstTime)
        {
            for (int i = 0; i < checkLogs.Length; i++)
            {
                checkLogs[i] = Vector3Int.one * -1;
            }
            return;
        }

        /*if (LightMap.isLitByLightCheck.Count <= 0)
            return;

        for (int i = 0; i < checkLogs.Length; i++)
        {
            if(LightMap.isLitByLightCheck.Count-checkLogs.Length+1 > i)
            {
                checkLogs[i] = LightMap.isLitByLightCheck[LightMap.isLitByLightCheck.Count-1-i];
            }
            else
            {
                checkLogs[i] = Vector3Int.one * -1;
            }
        }*/
    }

    private void OnApplicationQuit()
    {
        try
        {
            map.points = null;
            GC.Collect();
            Debug.Log("cleaned memory");
        }
        catch
        {

        }
    }
}
