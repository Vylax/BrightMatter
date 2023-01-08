using System;
using System.Collections.Generic;
using UnityEngine;
using Point = LightMap.Point;

public class LightElement : MonoBehaviour
{
	public static List<Light> lights = new List<Light>();
	public static List<Light> staticLights = new List<Light>();
	public static List<Light> dynamicLights = new List<Light>();

	public BoxCollider boxCollider;

	public bool IsStatic = false;

	public Light currLight;

	public bool playerIsInside;

	public static LayerMask objectsHitByLight = (1 << 8);

	public float Angle => currLight.spotAngle * 0.48f;
	public float Range => currLight.range;

	public List<Point> litUpPoints;

	private LightManager lm;

	private List<GameObject> objectsWithinBounds = new List<GameObject>();

	public bool forceUpdate;

	void OnDrawGizmosSelected()
	{
        try
        {
			BoxCollider b = boxCollider;
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, b.size.z) * 0.5f), .1f);
			Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f), .1f);
			Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f), .1f);
			Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f), .1f);
			Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f), .1f);
			Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, -b.size.z) * 0.5f), .1f);
			Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, -b.size.z) * 0.5f), .1f);
			Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, b.size.z) * 0.5f), .1f);

			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(b.bounds.min, .1f);
			Gizmos.DrawSphere(b.bounds.max, .1f);
		}
        catch
        {

        }
	}

	private bool newObjectsWithinBounds = true;
	private bool wasJustEnabled = false;
	public bool NeedUpdate
    {
        get
		{
			if (forceUpdate || playerIsInside && (newObjectsWithinBounds || this.transform.hasChanged || wasJustEnabled))
			{
				newObjectsWithinBounds = false;
				wasJustEnabled = false;
				this.transform.hasChanged = false;
				return true;
			}
			return false;
		}
    }

	private void Awake()
	{
		if (currLight == null)
		{
			currLight = GetComponent<Light>();
		}

		boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.isTrigger = true;
		CalculateBoxCollider(currLight.type);

		litUpPoints = new List<Point>();
		wasJustEnabled = true;

		gameObject.layer = LayerMask.NameToLayer("Lights");

		lm = GameObject.FindGameObjectWithTag("LightManager").GetComponent<LightManager>();
	}

	private void OnEnable()
	{
		if (currLight == null)
		{
			currLight = GetComponent<Light>();
		}

		List<Point> pointsThatWereLitByLight = new List<Point>(litUpPoints);
		foreach (Point point in pointsThatWereLitByLight)
		{
			point.AddLightSource(currLight);
		}

		lights.Add(currLight);
		if (IsStatic)
		{
			staticLights.Add(currLight);
			return;
		}
		dynamicLights.Add(currLight);
		wasJustEnabled = true;
	}

	private void OnDisable()
	{
		List<Point> pointsThatWereLitByLight = new List<Point>(litUpPoints);
		foreach (Point point in pointsThatWereLitByLight)
		{
			point.RemoveLightSource(currLight);
		}

		lights.Remove(currLight);
		if (IsStatic)
		{
			staticLights.Remove(currLight);
			return;
		}
		dynamicLights.Remove(currLight);
	}

	protected void CalculateBoxCollider(LightType lightType)
	{
		CheckIfScaleIsNotOne();
		switch (lightType)
		{
			case LightType.Spot:
				{
					float count = currLight.spotAngle / 2f * Mathf.Deg2Rad;
					float temp = 2 * currLight.range * Mathf.Sin(count) / Mathf.Sin(Mathf.PI / 2f - count);
					boxCollider.size = new Vector3(temp, temp, currLight.range);
					boxCollider.center = new Vector3(0f, 0f, currLight.range / 2f);
					return;
				}
			case LightType.Directional:
				if (Application.isEditor)
				{
					Debug.LogError($"Can't use Directional light: {gameObject.name}", gameObject);
					return;
				}
				break;
			case LightType.Point:
				{
					float num3 = currLight.range * 2f;
					boxCollider.size = new Vector3(num3, num3, num3);
					boxCollider.center = Vector3.zero;
					return;
				}
			case LightType.Area:
				Debug.LogError($"Can't use Area light: {gameObject.name}", gameObject);
				break;
			default:
				return;
		}
	}

	private void CheckIfScaleIsNotOne()
	{
		if (transform.lossyScale != Vector3.one)
		{
			Transform parent = transform.parent;
			transform.parent = null;
			transform.localScale = Vector3.one;
			transform.parent = parent;
		}
	}

	public bool PathFromSourceToPointIsClear(Vector3 point)
    {
		RaycastHit hit;
		float dist = Vector3.Distance(currLight.transform.position, point);
		Vector3 dir = point - currLight.transform.position;

		if(Physics.Raycast(currLight.transform.position, dir, out hit, dist + lm.map.maxStep / Mathf.Sqrt(2), objectsHitByLight, QueryTriggerInteraction.Ignore))
        {
			return Mathf.Abs(hit.distance - dist) < lm.map.maxStep / Mathf.Sqrt(2);
        }
		//Debug.Log($"Nothing was hit");
		return false;
    }

	public bool PointIsWithinLightAngle(Vector3 point)
    {
		//Debug.Log($"point {point}");
		Vector3 sourceNormalVector = Range * currLight.transform.forward;
		float pointAngleFromSourceNormal = Vector3.Angle(point - currLight.transform.position, sourceNormalVector);

		/*if(Mathf.Abs(pointAngleFromSourceNormal) >= Angle)
        {
			Debug.Log($"wtf angle {pointAngleFromSourceNormal} {Angle} {point - currLight.transform.position} {sourceNormalVector}");
        }*/

		return Mathf.Abs(pointAngleFromSourceNormal) < Angle;
    }

	public bool PointIsWithinLightRange(Vector3 point, bool pointLight = false)
    {
		return Vector3.Distance(point, currLight.transform.position) < Range * (pointLight ? 0.985f : 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
			playerIsInside = true;
		}
		else if(other.tag == "Ground")
        {
			objectsWithinBounds.Add(other.gameObject);
			newObjectsWithinBounds = true;
        }
    }

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			playerIsInside = false;
		}
		else if (other.tag == "Ground")
		{
			objectsWithinBounds.Remove(other.gameObject);
			newObjectsWithinBounds = true;
		}
	}
}
