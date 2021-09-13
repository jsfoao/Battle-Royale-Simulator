using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntitySenses : MonoBehaviour
{
    private Transform _transform;
    [NonSerialized] public GameManager gameManager;
    private float targetTime = 1f;
    private float currentTime;

    [SerializeField] private float senseDistance = 10f;
    [SerializeField] private List<GameObject> testList;
    // todo vision, noise, touch

    private List<GameObject> CheckForEnemies()
    {
        if (gameManager.entitiesList == null || gameManager.entitiesList.Count == 0)
        {
            return null;
        }
        
        List<GameObject> surroundingEntities = new List<GameObject>();
        foreach (var entity in gameManager.entitiesList)
        {
            float distance = (entity.transform.position - _transform.position).magnitude;
            if (distance > senseDistance)
            {
                if (surroundingEntities.Contains(entity))
                {
                    surroundingEntities.Remove(entity);
                }
                continue;
            }

            if (!surroundingEntities.Contains(entity) && entity != gameObject)
            {
                surroundingEntities.Add(entity);
            }
        }
        return surroundingEntities;
    }
    
    private void Update()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        { 
            // tick
            testList = CheckForEnemies();
            
            currentTime = targetTime;
        }
    }

    private void Start()
    {
        _transform = transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_transform.position, senseDistance);
    }
}
