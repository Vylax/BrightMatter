using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticClass;

[System.Serializable]
public class LightMap
{
    public static LightMap map;
    public static List<Point> drawPoints = new List<Point>();
    //public static List<Vector3Int> isLitByLightCheck = new List<Vector3Int>();

    public LightMap(Vector3 min, Vector3 max, bool highRes = false)
    {
        map = this;
        Init(min, max, highRes);
    }

    [System.Serializable]
    public class Point
    {
        public int Id { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3Int MapCoords { get; private set; }
        public bool isInLight => status == PointStatus.inLight || status == PointStatus.inPermanentLight;
        public PointStatus status = PointStatus.inShadow;

        [System.NonSerialized] public Point[] neighbors;

        private List<Light> lightSources;

        public bool PermanentStatus => status == PointStatus.inPermanentLight || status == PointStatus.inPermanentShadow;

        public Point(Vector3Int mapCoords, bool inLight = false, bool permanentLightStatus = false)
        {
            MapCoords = mapCoords;
            Position = MapCoordsToWorldPosition(mapCoords);
            Id = GetIdFromMapCoords(MapCoords);
            //isInLight = inLight;
            SetLightStatus(inLight, permanentLightStatus);
            lightSources = new List<Light>();
        }

        public Point(int x, int y, int z, bool inLight = false, bool permanentLightStatus = false)
        {
            MapCoords = new Vector3Int(x, y, z);
            Position = MapCoordsToWorldPosition(MapCoords);
            Id = GetIdFromMapCoords(MapCoords);
            //isInLight = inLight;
            SetLightStatus(inLight, permanentLightStatus);
            lightSources = new List<Light>();
        }

        public void AddLightSource(Light light)
        {
            if (PermanentStatus)
                return;

            if (!lightSources.Contains(light))
                lightSources.Add(light);
            if (!light.GetComponent<LightElement>().litUpPoints.Contains(this))
                light.GetComponent<LightElement>().litUpPoints.Add(this);
            //isInLight = true;
            SetLightStatus(true);
        }

        public void RemoveLightSource(Light light)
        {
            if (PermanentStatus)
                return;

            if (lightSources.Contains(light))
                lightSources.Remove(light);
            if (light.GetComponent<LightElement>().litUpPoints.Contains(this))
                light.GetComponent<LightElement>().litUpPoints.Remove(this);
            //isInLight = lightSources.Count > 0;
            SetLightStatus(lightSources.Count > 0);
        }

        public static int GetIdFromMapCoords(Vector3Int mapCoords)
        {
            return mapCoords.x + map.sqrRes * mapCoords.y + map.sqrRes * map.sqrRes * mapCoords.z;
        }

        public static Vector3 MapCoordsToWorldPosition(Vector3Int mapCoords)
        {
            return MapCoordsToWorldPosition(mapCoords.x, mapCoords.y, mapCoords.z);
        }

        public static Vector3 MapCoordsToWorldPosition(int x, int y, int z)
        {
            return new Vector3(x * map.step.x + map.min.x, y * map.step.y + map.min.y, z * map.step.z + map.min.z);
        }

        private static List<Vector3Int> neighborsDir;

        private void InitNeighborsDir()
        {
            neighborsDir = new List<Vector3Int>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if(!(i==0 && j==0 && k==0))
                            neighborsDir.Add(new Vector3Int(i, j, k));
                    }
                }
            }
        }

        public Point[] GetNeighbors()
        {
            if(neighbors != null)
            {
                return neighbors;
            }
            else
            {
                if(neighborsDir == null)
                {
                    InitNeighborsDir();
                }
                List<Point> temp = new List<Point>();
                foreach (Vector3Int dir in neighborsDir)
                {
                    Vector3Int neighbor = dir + this.MapCoords;
                    if (IsWithinMapBounds(neighbor) && map.points[neighbor.x, neighbor.y, neighbor.z] != null)
                    {
                        temp.Add(map.points[neighbor.x, neighbor.y, neighbor.z]);
                    }
                }

                neighbors = temp.ToArray();
                return neighbors;
            }
        }

        public static bool IsWithinMapBounds(Vector3Int coords)
        {
            bool withinXBounds = coords.x >= 0 && coords.x < map.sqrRes;
            bool withinYBounds = coords.y >= 0 && coords.y < map.sqrRes;
            bool withinZBounds = coords.z >= 0 && coords.z < map.sqrRes;
            return withinXBounds && withinYBounds && withinZBounds;
        }

        public bool IsLitByLight(LightElement light)
        {
            //check 1: check if the point is within the light trigger collider (ALREADY DONE WHEN THIS IS CALLED)
            //check 2: check if the point is within the light angle
            //Debug.Log($"Check 2: {light.PointIsWithinLightAngle(Position)}");
            //check 3: check if the point is within the range of the light
            //Debug.Log($"Check 3: {light.PointIsWithinLightRange(Position)}");
            //check 4: check if the point is no obstacle between the light source and the player
            //Debug.Log($"Check 4: {light.PathFromSourceToPointIsClear(Position)}");

            //Debug
            /*isLitByLightCheck.Add(new Vector3Int(
                light.PointIsWithinLightAngle(Position) ? 1 : 0,
                light.PointIsWithinLightRange(Position) ? 1 : 0,
                light.PathFromSourceToPointIsClear(Position) ? 1 : 0
                ));*/
            if (light.currLight.type == LightType.Spot)
            {
                return light.PointIsWithinLightAngle(Position) && light.PointIsWithinLightRange(Position) && light.PathFromSourceToPointIsClear(Position);
            }
            else if (light.currLight.type == LightType.Point)
            {
                return light.PathFromSourceToPointIsClear(Position) && light.PointIsWithinLightRange(Position, true);
            }
            return false;
        }

        public void SetLightStatus(bool inLight, bool permStatus = false)
        {
            if (this.status == PointStatus.inPermanentLight || this.status == PointStatus.inPermanentShadow)
                return;

            this.status = permStatus ? (inLight ? PointStatus.inPermanentLight : PointStatus.inPermanentShadow) : ((inLight ? PointStatus.inLight : PointStatus.inShadow));
        }
    }

    public int resolution;
    public int sqrRes;
    public Vector3 step;
    public Point[,,] points;
    public Vector3 min;
    public Vector3 max;

    #region points management
    public static Vector3Int GetClosestPointMapCoords(float x, float y, float z)
    {
        return new Vector3Int(
            CustomClamp(x - map.min.x),
            CustomClamp(y - map.min.y),
            CustomClamp(z - map.min.z));
    }

    public static Vector3 MapCoordsToWorldPosition(int x, int y, int z)
    {
        return new Vector3(x * map.step.x + map.min.x, y * map.step.y + map.min.y, z * map.step.z + map.min.z);
    }

    private static int CustomClamp(float x, int index=0)
    {
        int temp = Mathf.RoundToInt(x / (index == 0 ? map.step.x : (index == 1 ? map.step.y : map.step.z)));
        if(temp < 0)
        {
            return 0;
        }else if(temp >= map.sqrRes - 1)
        {
            return map.sqrRes - 1;
        }
        return temp;
    }

    public static Vector3Int GetClosestPointMapCoords(Vector3 vec)
    {
        return GetClosestPointMapCoords(vec.x, vec.y, vec.z);
    }

    public static Vector3 GetClosestPositionOnMap(float x, float y, float z)
    {
        return new Vector3(Mathf.RoundToInt(x / map.step.x) * map.step.x, Mathf.RoundToInt(y / map.step.y) * map.step.y, Mathf.RoundToInt(z / map.step.z) * map.step.z);
    }

    public static Vector3 GetClosestPositionOnMap(Vector3 vec)
    {
        return GetClosestPositionOnMap(vec.x, vec.y, vec.z);
    }

    public static Point GetClosestPointOnMap(Vector3 vec)
    {
        Vector3Int mapCoords = GetClosestPointMapCoords(vec);
        Point closestPoint = map.points[mapCoords.x, mapCoords.y, mapCoords.z];
        return closestPoint;
    }

    public static bool IsInLight(Vector3 pos)
    {
        Point point = GetClosestPointOnMap(pos);
        //return point.isInLight;
        return point.status == PointStatus.inLight || point.status == PointStatus.inPermanentLight;
    }

    public List<Point> GetAllPointsWithinBounds(Vector3 minBounds, Vector3 maxBounds)
    {
        //Debug.Log($"ccc {minBounds} {maxBounds}");
        minBounds -= Vector3.one * step.x;
        maxBounds += Vector3.one * step.x;
        minBounds = GetClosestPositionOnMap(minBounds);
        maxBounds = GetClosestPositionOnMap(maxBounds);
        //Debug.Log($"bbb {minBounds} {maxBounds}");

        int width = Mathf.FloorToInt((maxBounds.x - minBounds.x) / step.x);
        int height = Mathf.FloorToInt((maxBounds.y - minBounds.y) / step.y);
        int depth = Mathf.FloorToInt((maxBounds.z - minBounds.z) / step.z);
        //Debug.Log($"aaaa {width} {height} {depth}");

        List<Point> temp = new List<Point>();
        Vector3Int coords = GetClosestPointMapCoords(minBounds);
        //Debug.Log($"putain {coords} {minBounds}");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {

                    /*Point tempPoint = GetClosestPointOnMap(minBounds + x * step.x * Vector3.right + y * step.y * Vector3.up + z * step.z * Vector3.forward);
                    Debug.Log($"bruh {tempPoint.Position} {minBounds}");*/
                    Point tempPoint = null;
                    if (Point.IsWithinMapBounds(new Vector3Int(coords.x + x, coords.y + y, coords.z + z)))
                        tempPoint = map.points[coords.x+x, coords.y+y, coords.z+z];
                    //Debug.Log($"bruh {tempPoint.Position}");
                    if (tempPoint != null)
                        temp.Add(tempPoint);
                }
            }
        }

        //Debug.Log($"mmmh {temp.Count}");
        return temp;
    }

    public List<Point> GetAllPointsWithinBounds(LightElement light)
    {
        return GetAllPointsWithinBounds(light.boxCollider.bounds.min, light.boxCollider.bounds.max);
    }
    #endregion

    private void Init(Vector3 min, Vector3 max, bool highRes)
    {
        this.min = min;
        this.max = max;

        sqrRes = Mathf.RoundToInt((highRes?6.4f:3.2f) * Mathf.Max((max.x - min.x), (max.z - min.z)));
        resolution = sqrRes * sqrRes * sqrRes;
        
        //step = new Vector3((max.x - min.x) / sqrRes, (max.y - min.y) / sqrRes, (max.z - min.z) / sqrRes);
        step = Vector3.one * Mathf.Min(new float[] { (max.x - min.x) / sqrRes, (max.z - min.z) / sqrRes }); ;

        points = new Point[sqrRes, sqrRes, sqrRes];

        for (int x = 0; x < sqrRes; x++)
        {
            for (int y = 0; y < sqrRes; y++)
            {
                for (int z = 0; z < sqrRes; z++)
                {
                    //Debug.Log($"check {x} {y} {z} {Point.MapCoordsToWorldPosition(x, y, z)}");
                    if (Physics.CheckSphere(Point.MapCoordsToWorldPosition(x,y,z), maxStep/Mathf.Sqrt(2), LightElement.objectsHitByLight, QueryTriggerInteraction.Ignore))//check if "groundtype" object is near point
                    {
                        //Debug.Log($"test {x} {y} {z}");
                        points[x, y, z] = new Point(x, y, z);
                    }
                }
            }
        }

        //clean useless points and initialize neighbors
        List<Vector3Int> temp = new List<Vector3Int>();
        for (int x = 0; x < sqrRes; x++)
        {
            for (int y = 0; y < sqrRes; y++)
            {
                for (int z = 0; z < sqrRes; z++)
                {
                    if (points[x, y, z] != null && points[x, y, z].GetNeighbors().Length == (3 * 9 - 1))
                        temp.Add(new Vector3Int(x, y, z));
                }
            }
        }

        foreach (Vector3Int coords in temp)
        {
            points[coords.x, coords.y, coords.z] = null;
        }
    }

    public void UpdateLightMap()
    {
        foreach (Light light in LightElement.lights)
        {
            if (light.GetComponent<LightElement>().NeedUpdate)
            {
                CalculatePointsLitByLight(light);
            }
        }
    }

    private void CalculatePointsLitByLight(Light light)
    {
        List<Point> pointsToCheck = GetAllPointsWithinBounds(light.GetComponent<LightElement>());
        drawPoints = pointsToCheck;
        List<Point> pointsThatWereLitByLight = new List<Point>(light.GetComponent<LightElement>().litUpPoints);

        foreach (Point point in pointsThatWereLitByLight)
        {
            if (point != null && !pointsToCheck.Contains(point))
            {
                point.RemoveLightSource(light);
            }
        }

        int debug = 0;
        int existingPoints = 0;
        //Debug.Log($"Points to check : {pointsToCheck.Count}");
        foreach (Point point in pointsToCheck)
        {
            //Debug.Log($"Here we are {point.Position} {point.Id}");
            if (point != null && !point.PermanentStatus)
            {
                existingPoints++;
                if (point.IsLitByLight(light.GetComponent<LightElement>()))
                {
                    point.AddLightSource(light);
                }
                else
                {
                    point.RemoveLightSource(light);
                    debug++;
                }
            }
        }
        //Debug.Log($"Points that exist: {existingPoints}");
        //Debug.Log($"removed {debug} points from light");
    }

    #region HelpMethods
    public float maxStep => Mathf.Max(new float[3] { step.x, step.y, step.z });
    public float minStep => Mathf.Min(new float[3] { step.x, step.y, step.z });
    #endregion
}
