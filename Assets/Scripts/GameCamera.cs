using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private Map _map;
    private void Start()
    {
        _map = GameObject.Find("Map").GetComponent<Map>();
        Vector3 newPos = new Vector3(((_map.size.x - 1) * _map.offset) / 2, 
                                                ((_map.size.y - 1) * _map.offset) / 2, 
                                                    -10f);
        transform.position = newPos;
    }
}
