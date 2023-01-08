using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateNTranslate : MonoBehaviour
{
    public bool rotate = false;
    public bool translate = false;

    public Vector3 rotateAxis;
    public Vector3 translateAxis;
    private Vector3 RotateAxis;
    private Vector3 TranslateAxis;

    public float rotateSpeed;
    public float translateSpeed;

    public float rotateHalfAngle;
    public float translateHalfDist;

    private Vector3 originPos;
    private Vector3 originRot;

    private void Start()
    {
        originPos = transform.localPosition;
        originRot = transform.localEulerAngles;
    }

    private void FixedUpdate()
    {
        RotateAxis = rotateAxis.normalized;
        TranslateAxis = translateAxis.normalized;
        
        if(translate)
            transform.localPosition = originPos + TranslateAxis * translateHalfDist * Mathf.Sin(translateSpeed * Time.time);

        if (rotate)
            transform.localEulerAngles = originRot + RotateAxis * rotateHalfAngle * Mathf.Sin(rotateSpeed * Time.time);
    }
}
