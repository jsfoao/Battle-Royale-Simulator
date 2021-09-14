using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityAI : MonoBehaviour
{
    private Transform _transform;
    [NonSerialized] public GameManager gameManager;
    private EntityController controller;
    
    [Header("AI Tick")]
    [SerializeField] private float tick = 1f;
    private float currentTime;

    [Header("Distance Sense")]
    // Entities
    [NonSerialized] private Dictionary<GameObject, float> surroundingEntities;
    [SerializeField] private List<GameObject> seenEntities;
    
    // Loots
    [NonSerialized] public Dictionary<GameObject, float> surroundingLoots;
    [SerializeField] public List<GameObject> seenLoots;
    [SerializeField] public bool pickupLoot = false;

    [Header("Vision Sense")] 
    [SerializeField] private float viewDistance = 25f;
    [SerializeField] [Range(0, 90)]
    private float fieldOfView = 20f;
    [SerializeField] private GameObject closestSeenEntity;
    [SerializeField] private GameObject closestSeenLoot;

    [Header("State Machine")]
    [SerializeField] private GeneralState generalState = GeneralState.Wandering;

    private Rect mapArea;
    private Vector2 peripheralRight;
    private Vector2 peripheralLeft;
    
    public enum GeneralState
    {
        Nothing,
        MouseMode,
        Wandering,
        Looting,
        Shooting
    }

    #region Entities
    // EVALUATE ENTITIES
    private void SurroundingEntities(float range)
    {
        if (gameManager.entitiesList == null || gameManager.entitiesList.Count == 0)
        {
            return;
        }
        
        Dictionary<GameObject, float> entities = new Dictionary<GameObject, float>();
        
        foreach (var entity in gameManager.entitiesList)
        {
            float distance = (entity.transform.position - _transform.position).magnitude;
            
            // Return and remove from surroundingEntities if not in range of player
            if (distance > range)
            {
                if (entities.ContainsKey(entity))
                {
                    entities.Remove(entity);
                }
                continue;
            }

            // Entity is in range of player
            if (!entities.ContainsKey(entity) && entity != gameObject)
            {
                entities.Add(entity, distance);
            }
        }

        surroundingEntities = entities;
        EvaluateSurroundingEntities();
    }

    private void EvaluateSurroundingEntities()
    {
        float minDistance = Int32.MaxValue;

        foreach (var entity in surroundingEntities.Keys)
        {
            // Check if entity is in FOV
            Vector2 direction = entity.transform.position - _transform.position;
            if (!(Vector2.Angle(_transform.right, direction) <= 90f - fieldOfView))
            {
                if (seenEntities.Contains(entity))
                {
                    seenEntities.Remove(entity);
                }
                continue;
            }
            if (!seenEntities.Contains(entity))
            {
                seenEntities.Add(entity);
            } 
            
            // Calculate closest seen entity
            if (surroundingEntities[entity] < minDistance && entity.CompareTag("Entity"))
            {
                closestSeenEntity = entity;
                minDistance = surroundingEntities[entity];
            }
            
            Debug.DrawLine(_transform.position, entity.transform.position, Color.green);
        }
    }
    #endregion

    #region Loot
    private void SurroundingLoots(float range)
    {
        if (gameManager.lootList == null || gameManager.lootList.Count == 0)
        {
            return;
        }
        
        Dictionary<GameObject, float> loots = new Dictionary<GameObject, float>();
        
        foreach (var loot in gameManager.lootList)
        {
            if (loot == null)
            {
                continue;
            }
            
            float distance = (loot.transform.position - _transform.position).magnitude;
            
            // Return and remove from surroundingLoots if not in range of player
            if (distance > range)
            {
                if (loots.ContainsKey(loot))
                {
                    loots.Remove(loot);
                }
                continue;
            }

            // Entity is in range of player
            if (!loots.ContainsKey(loot))
            {
                loots.Add(loot, distance);
            }
        }

        surroundingLoots = loots;
        EvaluateSurroundingLoots();
    }
    
    private void EvaluateSurroundingLoots()
    {
        float minDistance = Int32.MaxValue;
    
        foreach (var loot in surroundingLoots.Keys)
        {
            // Check if loot is in FOV
            Vector2 direction = loot.transform.position - _transform.position;
            if (!(Vector2.Angle(_transform.right, direction) <= 90f - fieldOfView))
            {
                continue;
            }
            // Calculate closest seen loot
            if (surroundingLoots[loot] < minDistance && loot.CompareTag("Loot"))
            {
                closestSeenLoot = loot;
                minDistance = surroundingLoots[loot];
            }
            Debug.DrawLine(_transform.position, loot.transform.position, Color.magenta);
        }
    }
    #endregion
    
    private void DoNothing()
    {
    }
    
    private void DoMouseMode()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            controller.target.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            controller.target.position = new Vector3(controller.target.position.x, controller.target.position.y, 0f);

            if (controller.target.position.x > controller.map.worldSize.x || controller.target.position.x < 0f) { return; }
            if (controller.target.position.y > controller.map.worldSize.y || controller.target.position.y < 0f) { return; }
            
            controller.MoveToTarget(controller.target);
        }
    }
    
    private void DoWandering()
    {
        if (!controller.moving)
        {
            controller.MoveToRandomTarget(mapArea);
        }
    }

    private void DoLooting()
    {
        if (!controller.moving && closestSeenLoot != null)
        {
            controller.MoveToTarget(closestSeenLoot.transform);
        }
    }
    
    private void DoShooting()
    {
        
    }
    
    // Called every second
    private void TickUpdate()
    {
    }
    
    // Called every frame
    private void Update()
    {
        // currentTime -= Time.deltaTime;
        // if (currentTime <= 0)
        // { 
        //     TickUpdate();
        //     
        //     currentTime = tick;
        // }
        
        SurroundingEntities(viewDistance);
        SurroundingLoots(viewDistance);
        
        switch (generalState)
        {
            case GeneralState.Nothing:
                DoNothing();
                controller.StopMovement();
                break;
            
            case GeneralState.MouseMode:
                DoMouseMode();
                break;
            
            case GeneralState.Wandering:
                DoWandering();
                
                // Wandering to Looting
                if (closestSeenLoot != null)
                {
                    controller.StopMovement();
                    generalState = GeneralState.Looting;
                }
                break;
            
            case GeneralState.Looting:
                DoLooting();
                
                // Looting to Wandering
                if (closestSeenLoot == null)
                {
                    controller.StopMovement();
                    generalState = GeneralState.Wandering;
                }
                break;
        }
    }

    private void Start()
    {
        _transform = transform;
        controller = GetComponent<EntityController>();
        gameManager = FindObjectOfType<GameManager>();
        currentTime = tick;
        mapArea = new Rect(0f, 0f, controller.map.size.x - 1, controller.map.size.y - 1);
    }
    
    private void OnDrawGizmos()
    {
        _transform = transform;
        
        // Sense touch
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(_transform.position, proximityRange);
        
        // Sense distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_transform.position, viewDistance);
        
        // Field of view
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_transform.position, _transform.rotation * new Vector2(Mathf.Sin(Mathf.Deg2Rad * fieldOfView), Mathf.Cos(Mathf.Deg2Rad * fieldOfView)) * viewDistance);
        Gizmos.DrawRay(_transform.position, _transform.rotation * new Vector2(Mathf.Sin(Mathf.Deg2Rad * fieldOfView), -Mathf.Cos(Mathf.Deg2Rad * fieldOfView)) * viewDistance);

    }
}