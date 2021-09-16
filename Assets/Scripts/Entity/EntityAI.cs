using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class EntityAI : MonoBehaviour
{
    private Transform _transform;
    private Transform _transformModel;
    private  GameManager gameManager;
    private EntityController controller;
    private Shooting _shooting;
    
    
    [Header("AI Tick")]
    [SerializeField] private float tick = 1f;
    private float currentTime;

    [Header("Distance Sense")]
    // Entities
    [NonSerialized] private List<GameObject> seenEntities;
    
    // Loots
    [NonSerialized] private List<GameObject> seenLoots;

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
    
    private List<GameObject> SeenInList(List<GameObject> list, GameObject closestObject)
    {
        if (list == null || list.Count == 0)
        {
            return null;
        }

        List<GameObject> seenList = new List<GameObject>();
        float minDistance = Int32.MaxValue;
        
        foreach (var worldObject in list)
        {
            Vector2 direction = worldObject.transform.position - _transform.position;
            float distance = direction.magnitude;
            
            // If not in range of player
            if (distance > viewDistance)
            {
                if (worldObject == closestSeenEntity)
                {
                    closestSeenEntity = null;
                }

                if (worldObject == closestSeenLoot)
                {
                    closestSeenLoot = null;
                }
                continue;
            }
            
            // If not in FOV
            if (!(Vector2.Angle(_transformModel.right, direction) <= 90f - fieldOfView))
            {
                if (worldObject == closestSeenEntity)
                {
                    closestSeenEntity = null;
                }
                if (worldObject == closestSeenLoot)
                {
                    closestSeenLoot = null;
                }
                continue;
            }
            
            // If closest seen entity
            if (direction.magnitude < minDistance)
            {
                if (worldObject != gameObject)
                {
                    if (worldObject.CompareTag("Entity"))
                    {
                        closestSeenEntity = worldObject;
                    }

                    if (worldObject.CompareTag("Loot"))
                    {
                        closestSeenLoot = worldObject;
                    }
                    minDistance = direction.magnitude;
                }
            }
        }

        return seenList;
    }

    private void DoNothing()
    {
    }
    
    private void DoMouseMode()
    {
        _shooting.AimToTarget(closestSeenEntity.transform.position);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            controller.moveTarget.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            controller.moveTarget.position = new Vector3(controller.moveTarget.position.x, controller.moveTarget.position.y, 0f);

            if (controller.moveTarget.position.x > controller.map.worldSize.x || controller.moveTarget.position.x < 0f) { return; }
            if (controller.moveTarget.position.y > controller.map.worldSize.y || controller.moveTarget.position.y < 0f) { return; }
            
            controller.MoveToTarget(controller.moveTarget);
        }
    }
    
    private void DoWandering()
    {
        if (controller.currentTarget != null)
        {
            _shooting.AimToTarget(controller.currentTarget);
        }
        if (!controller.moving)
        {
            controller.MoveToRandomTarget(mapArea);
        }
    }

    private void DoLooting()
    {
        if (closestSeenLoot != null)
        {
            _shooting.AimToTarget(closestSeenLoot.transform.position);
        }
        if (!controller.moving && closestSeenLoot != null)
        {
            controller.MoveToTarget(closestSeenLoot.transform);
        }
    }
    
    private void DoShooting()
    {
        if (closestSeenEntity != null)
        {
            _shooting.AimToTarget(closestSeenEntity.transform.position);
        }
        
        _shooting.currentTime -= Time.deltaTime;
        if (_shooting.currentTime <= 0)
        {
            _shooting.Shoot();
            
            _shooting.currentTime = _shooting.fireRate;
        }
    }
    
    // Called every tick time
    private void TickUpdate()
    {
        seenEntities = SeenInList(gameManager.entitiesList, closestSeenEntity);
        seenLoots = SeenInList(gameManager.lootsList, closestSeenLoot);
        
        RaycastHit2D hit = Physics2D.Raycast(_transformModel.position, _transformModel.right * viewDistance);
        if (hit.collider != null)
        {
            if (hit.collider.transform.CompareTag("Entity"))
            {
            }
        }
    }
    
    // Called every frame
    private void Update()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        { 
            TickUpdate();
            
            currentTime = tick;
        }

        switch (generalState)
        {
            case GeneralState.Nothing:
                DoNothing();
                
                // // Nothing to Shooting
                // if (closestSeenEntity != null)
                // {
                //     controller.StopMovement();
                //     generalState = GeneralState.Shooting;
                // }
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
                
                // Wandering to Shooting
                if (closestSeenEntity != null && GetComponent<Entity>().Loot >= 1)
                {
                    controller.StopMovement();
                    generalState = GeneralState.Shooting;
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
                
                // Looting to Shooting
                if (closestSeenEntity != null && GetComponent<Entity>().Loot >= 1)
                {
                    controller.StopMovement();
                    generalState = GeneralState.Shooting;
                }
                break;
            
            case GeneralState.Shooting:
                DoShooting();
                
                // Shooting to Wandering
                if (closestSeenEntity == null || GetComponent<Entity>().Loot <= 0)
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
        _transformModel = transform.Find("Model");
        controller = GetComponent<EntityController>();
        _shooting = GetComponent<Shooting>();
        gameManager = FindObjectOfType<GameManager>();
        currentTime = tick;
        mapArea = new Rect(0f, 0f, controller.map.size.x - 1, controller.map.size.y - 1);
    }
    
    private void OnDrawGizmos()
    {
        _transform = transform;
        _transformModel = transform.Find("Model");
        // Sense touch
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(_transform.position, proximityRange);
        
        // Sense distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_transform.position, viewDistance);
        
        // Field of view
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_transform.position, _transformModel.rotation * new Vector2(Mathf.Sin(Mathf.Deg2Rad * fieldOfView), Mathf.Cos(Mathf.Deg2Rad * fieldOfView)) * viewDistance);
        Gizmos.DrawRay(_transform.position, _transformModel.rotation * new Vector2(Mathf.Sin(Mathf.Deg2Rad * fieldOfView), -Mathf.Cos(Mathf.Deg2Rad * fieldOfView)) * viewDistance);

    }
}