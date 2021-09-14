using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string name;
    public int health;
    public int loot;
    public float range;

    public string Name { get => name; set => name = value; }
    public int Health { get => health; set => health = value; }
    public int Loot { get => loot; set => loot = value; }
    public float Range { get => range; set => range = value; }

    public void AddLoot()
    {
        Loot += 1;
    }
    
    private void Update()
    {
        if (Health <= 0)
        {
            GetComponent<EntityController>().DestroyEntity();
        }
    }
}
