using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private void Start()
    {
        //Choose Random model for this obstacle
        Transform[] obstacleModels = transform.GetComponentsInChildren<Transform>(true);
        int modelIndex = Random.Range(1, obstacleModels.Length - 1);
        obstacleModels[modelIndex].gameObject.SetActive(true);
    }
}
