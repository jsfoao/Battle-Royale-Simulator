using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private Transform _transform;

    private void OnDrawGizmos()
    {
        _transform = transform;
        Gizmos.color = Color.blue;
        
        Gizmos.DrawSphere(_transform.position, _transform.localScale.x / 2);
    }
}
