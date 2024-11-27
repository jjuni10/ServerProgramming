using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTextRotate : MonoBehaviour
{
    //private Vector3 ScreenCenter;

    void Start()
    {
        //ScreenCenter = new Vector3(Camera.main.pixelWidth/2, Camera.main.pixelHeight/2);
    }

    void FixedUpdate()
    {
        //Ray ray = Camera.main.ScreenPointToRay(ScreenCenter);
        //this.transform.rotation = Quaternion.Euler(ray.direction);
        transform.forward = Camera.main.transform.forward;
    }
}
