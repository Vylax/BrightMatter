using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : MonoBehaviour
{
    [SerializeField] [Range(0f, 2f)] private float timeScale = 1f;

    private void Update()
    {
        Time.timeScale = timeScale;
    }
}
