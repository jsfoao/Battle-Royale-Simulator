using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public float fireRate = 1f;
    public float currentTime;
    public int damage = 10;
    public void Shoot(GameObject entityObject)
    {
        Debug.DrawLine(transform.position, entityObject.transform.position, Color.red);
        Entity entity = entityObject.GetComponent<Entity>();
        entity.Health -= damage;
    }

    private void Start()
    {
        currentTime = fireRate;
    }
}
