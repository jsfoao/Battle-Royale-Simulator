using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EntityAI : MonoBehaviour
{
    private Transform _transform;
    [NonSerialized] public GameManager gameManager;
    private EntityController controller;
    
    [Header("AI Tick")]
    [SerializeField] private float tick = 1f;
    private float currentTime;

    [Header("Distance Sense")]
    [SerializeField] private List<GameObject> surroundingEntities;
    [SerializeField] private List<GameObject> seenEntities;

    [Header("Vision Sense")] 
    [SerializeField] private float viewDistance = 25f;
    [SerializeField] [Range(0, 90)]
    private float fieldOfView = 20f;

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
        Shooting
    }

    private List<GameObject> SurroundingEntities(float range)
    {
        if (gameManager.entitiesList == null || gameManager.entitiesList.Count == 0)
        {
            return null;
        }
        
        // Check if there are any entities in range of player
        List<GameObject> entities = new List<GameObject>();
        foreach (var entity in gameManager.entitiesList)
        {
            float distance = (entity.transform.position - _transform.position).magnitude;
            if (distance > range)
            {
                if (entities.Contains(entity))
                {
                    entities.Remove(entity);
                }
                continue;
            }

            if (!entities.Contains(entity) && entity != gameObject)
            {
                entities.Add(entity);
            }
        }
        return entities;
    }

    private void EvaluateSurroundingEntities()
    {
        foreach (var entity in surroundingEntities)
        {
            Vector2 direction = entity.transform.position - _transform.position;
            
            // If entity is not in FOV
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
            
            Debug.DrawLine(_transform.position, entity.transform.position, Color.green);
        }
    }

    private void DoNothing()
    {
        if (controller.movement != null)
        {
            StopCoroutine(controller.movement);
        }
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
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        { 
            TickUpdate();
            
            currentTime = tick;
        }

        surroundingEntities = SurroundingEntities(viewDistance);
        EvaluateSurroundingEntities();
        
        switch (generalState)
        {
            case GeneralState.Nothing:
                DoNothing();
                break;
            case GeneralState.MouseMode:
                DoMouseMode();
                break;
            case GeneralState.Wandering:
                DoWandering();
                break;
        }
    }

    private void Start()
    {
        _transform = transform;
        controller = GetComponent<EntityController>();
        gameManager = FindObjectOfType<GameManager>();

        mapArea = new Rect(0f, 0f, controller.map.size.x - 1, controller.map.size.y - 1);
    }
    
    private void OnDrawGizmos()
    {
        _transform = transform;
        // Sense distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_transform.position, viewDistance);
        
        // Field of view
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_transform.position, _transform.rotation * new Vector2(Mathf.Sin(Mathf.Deg2Rad * fieldOfView), Mathf.Cos(Mathf.Deg2Rad * fieldOfView)) * viewDistance);
        Gizmos.DrawRay(_transform.position, _transform.rotation * new Vector2(Mathf.Sin(Mathf.Deg2Rad * fieldOfView), -Mathf.Cos(Mathf.Deg2Rad * fieldOfView)) * viewDistance);

    }
}
