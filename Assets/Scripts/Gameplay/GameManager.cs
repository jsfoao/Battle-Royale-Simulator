using System.Collections.Generic;
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

        for (int i = 0; i < lootTotal; i++)
        {
            SpawnLoot(RandomLocation());
        }
    }
}
