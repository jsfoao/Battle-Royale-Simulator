using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private int entitiesTotal = 2;
    public List<GameObject> entitiesList;

    private Map _map;

    private void DestroyEntity(GameObject entity)
    {
        entitiesList.Remove(entity);
        Destroy(entity);
    }
    
    private void SpawnEntity(Vector3 location)
    {
        var entity= Instantiate(entityPrefab, location, Quaternion.identity);
        entity.GetComponent<EntitySenses>().gameManager = this;
        entity.transform.SetParent(transform);
        entitiesList.Add(entity);
    }

    private Vector3 RandomLocation()
    {
        Vector3 result;
        result.x = Random.Range(0f, _map.worldSize.x);
        result.y = Random.Range(0f, _map.worldSize.y);
        result.z = 0f;
        return result;
    }
    
    private void Start()
    {
        _map = FindObjectOfType<Map>();
        
        for (int i = 0; i < entitiesTotal; i++)
        {
            SpawnEntity(RandomLocation());
        }
    }
}
