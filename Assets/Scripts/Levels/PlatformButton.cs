using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformButton : MonoBehaviour
{
    public bool translate = false;

    public Transform platform;

    public Vector3[] waypoints;
    private int currWaypoint = 0;
    public float translateSpeed = 1f;
    public float buttonCooldown = .2f;

    public bool singleUse = false;

    private bool isMoving;

    private int NextWaypoint => currWaypoint + 1 < waypoints.Length ? currWaypoint + 1 : 0;

    private void Update()
    {
        if(isMoving)
            platform.position = Vector3.MoveTowards(platform.position, waypoints[NextWaypoint], translateSpeed * Time.deltaTime);
    }

    private IEnumerator MovePlatform()
    {
        isMoving = true;
        yield return new WaitForSeconds(2f);
        yield return new WaitUntil(() => Vector3.Distance(platform.position, waypoints[NextWaypoint]) < 0.001f);
        if (!singleUse)
        {
            isMoving = false;
            currWaypoint = NextWaypoint;
        }
    }

    private void ButtonClicked()
    {
        if(translate && !isMoving)
        {
            StartCoroutine(MovePlatform());
        }
    }

    private void OnMouseDown()
    {
        ButtonClicked();
    }
}
