using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EntityAI : MonoBehaviour
{
    private Transform _transform;
    [NonSerialized] public GameManager gameManager;
    private EntityController controller;
    
    [Header("AI Tick")]
    private float targetTime = 1f;
    private float currentTime;

    [Header("Distance Sense")]
    [SerializeField] private float senseDistance = 10f;
    [SerializeField] private List<GameObject> testList;

    [Header("State Machine")]
    [SerializeField] private GeneralState generalState = GeneralState.Wandering;
    [SerializeField] private ActionState actionState = ActionState.Neutral;

    private Rect mapArea;
    
    public enum GeneralState
    {
        MouseMode,
        Wandering,
    }
    
    public enum ActionState
    {
        Neutral,
    }

    private List<GameObject> SurroundingEntities(float range)
    {
        if (gameManager.entitiesList == null || gameManager.entitiesList.Count == 0)
        {
            return null;
        }
        
        List<GameObject> surroundingEntities = new List<GameObject>();
        foreach (var entity in gameManager.entitiesList)
        {
            float distance = (entity.transform.position - _transform.position).magnitude;
            if (distance > range)
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
    
    // Called every second
    private void TickUpdate()
    {
        testList = SurroundingEntities(senseDistance);
    }
    
    // Called every frame
    private void Update()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        { 
            TickUpdate();
            
            currentTime = targetTime;
        }

        switch (generalState)
        {
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_transform.position, senseDistance);
    }
}
