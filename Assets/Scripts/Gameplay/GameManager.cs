using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Entity Spawner")]
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private int entitiesTotal = 2;
    public List<GameObject> entitiesList;

    [Header("Loot Spawner")] 
    [SerializeField] private GameObject lootPrefab;
    [SerializeField] private int lootTotal = 2;
    public List<GameObject> lootsList;

    private Map _map;

    public void DestroyEntity(GameObject entityObject)
    {
        entitiesList.Remove(entityObject);
        Destroy(entityObject);

        if (entitiesList.Count == 1)
        {
            Entity entity = entitiesList[0].GetComponent<Entity>();
            Debug.Log($"{entity.Name} is winner winner chicken dinner!");
        }
    }

    // Spawn Entity on location
    private void SpawnEntity(Vector3 location)
    {
        var entity= Instantiate(entityPrefab, location, Quaternion.identity);
        entity.transform.SetParent(transform.GetChild(0));
        entitiesList.Add(entity);
        entity.GetComponent<Entity>().Name = $"Entity {entitiesList.IndexOf(entity)}";
    }

    // Spawn Loot on location
    private void SpawnLoot(Vector3 location)
    {
        var loot= Instantiate(lootPrefab, location, Quaternion.identity);
        loot.transform.SetParent(transform.GetChild(1));
        lootsList.Add(loot);
    }

    // Generates random Vector3 bound by map world size
    private Vector3 RandomPosition()
    {
        Vector3 result;
        result.x = Random.Range(0f, _map.worldSize.x);
        result.y = Random.Range(0f, _map.worldSize.y);
        result.z = 0f;

        return result;
    }

    private void SpawnWorldObjects()
    {
        // Spawn entities
        for (int i = 0; i <= entitiesTotal; i++)
        {
            bool entitySpawned = false;
            if (i == entitiesTotal) { break; }

            while (!entitySpawned)
            {
                Vector3 position = RandomPosition();
                if (_map.TileFromWorldPosition(position).walkable)
                {
                    entitySpawned = true;
                    SpawnEntity(position);
                }
            }
        }

        // Spawn loots
        for (int i = 0; i <= lootTotal; i++)
        {
            bool lootSpawned = false;
            if (i == lootTotal) { break; }

            while (!lootSpawned)
            {
                Vector3 position = RandomPosition();
                if (_map.TileFromWorldPosition(position).walkable)
                {
                    lootSpawned = true;
                    SpawnLoot(position);
                }
            }
        }
    }
    
    private void Start()
    {
        _map = FindObjectOfType<Map>();

        SpawnWorldObjects();
    }
}
