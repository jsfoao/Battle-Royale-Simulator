using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private GameManager _gameManager;
    public string name;
    [NonSerialized] public int health;
    [Tooltip("Bullets that Entity is carrying")]
    public int loot;

    public string Name { get => name; set => name = value; }
    public int Health { get => health; set => health = value; }
    public int Loot { get => loot; set => loot = value; }

    public void AddLoot()
    {
        Loot += 1;
    }

    public void RemoveLoot()
    {
        Loot -= 1;
    }

    public void DoDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            _gameManager.DestroyEntity(gameObject);
        }
    }

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }
}
