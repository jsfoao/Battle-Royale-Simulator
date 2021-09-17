using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class EntityAI : MonoBehaviour
{
    private Transform _transform;
    private Transform _transformModel;
    private  GameManager _gameManager;
    private EntityController _controller;
    private Shooting _shooting;
    private Entity _entity;

    [Header("AI Tick")]
    [SerializeField] private float tick = 1f;
    private float currentTime;

    [Header("Vision Sense")]
    [SerializeField] public float viewDistance = 25f;
    [SerializeField] [Range(0, 90)]
    private float fieldOfView = 20f;
    private Vector2 _closestSeenLootPosition;
    private Vector2 _closestSeenEntityPosition;
    private readonly Vector2 _unseenVector = new Vector2(-1, -1);


    [Header("State Machine")]
    [SerializeField] private GeneralState generalState = GeneralState.Wandering;

    private Rect mapArea;

    private enum GeneralState
    {
        Nothing,
        MouseMode,
        Wandering,
        Looting,
        Shooting
    }
    
    private Vector2 ClosestSeenPosition(List<GameObject> list)
    {
        if (list == null || list.Count == 0) { return _unseenVector; }

        Vector2 closestPosition = _unseenVector;
        float minDistance = Int32.MaxValue;
        foreach (var worldObject in list)
        {
            Vector2 direction = worldObject.transform.position - _transform.position;
            float distance = direction.magnitude;
            
            // If not in range of player
            if (distance > viewDistance)
            {
                continue;
            }
            
            // If not in FOV
            if (!(Vector2.Angle(_transformModel.right, direction) <= 90f - fieldOfView))
            {
                continue;
            }
            
            // If not in line of sight
            RaycastHit2D hit = Physics2D.Raycast((Vector2)_transform.position + direction.normalized * 0.8f, direction.normalized * viewDistance);
            if (hit.collider != null)
            {
                if (hit.collider.transform.tag == "Wall")
                {
                    continue;
                }
            }
            
            // Debug.DrawRay((Vector2)_transform.position + direction.normalized * 0.8f, direction.normalized * viewDistance, Color.red);
            // Calculate closest
            if (distance < minDistance && worldObject != gameObject)
            {
                closestPosition = worldObject.transform.position;
                minDistance = direction.magnitude;
            }
        }
        return closestPosition;
    }

    #region State Behaviours
    private void DoNothing()
    {
        if (_closestSeenEntityPosition != _unseenVector)
        {
            _controller.AimToTarget(_closestSeenEntityPosition);
        }
    }
    
    private void DoMouseMode()
    {
        if (_closestSeenEntityPosition != _unseenVector)
        {
            _controller.AimToTarget(_closestSeenEntityPosition);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // moveTarget is set on mouse position
            var position = _controller.moveTarget.position;
            position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position = (Vector2)(position);
            _controller.moveTarget.position = position;

            // Return if clicked outside of map
            if (_controller.moveTarget.position.x > _controller.map.worldSize.x || _controller.moveTarget.position.x < 0f) { return; }
            if (_controller.moveTarget.position.y > _controller.map.worldSize.y || _controller.moveTarget.position.y < 0f) { return; }
            
            _controller.MoveToPosition(_controller.moveTarget.position);
        }
    }
    
    private void DoWandering()
    {
        _controller.moveSpeed = _controller.sprintSpeed;
        if (_controller.currentTarget != null)
        {
            _controller.AimToTarget(_controller.currentTarget);
        }
        if (!_controller.moving)
        {
            _controller.MoveToRandomPosition(mapArea);
        }
    }

    private void DoLooting()
    {
        _controller.moveSpeed = _controller.sprintSpeed;
        if (_closestSeenLootPosition != _unseenVector)
        {
            _controller.AimToTarget(_closestSeenLootPosition);
        }
        if (!_controller.moving && _closestSeenLootPosition != _unseenVector)
        {
            _controller.MoveToPosition(_closestSeenLootPosition);
        }
    }
    
    private void DoShooting()
    {
        _controller.moveSpeed = _controller.walkSpeed;
        if (_closestSeenEntityPosition != _unseenVector)
        {
            _controller.AimToTarget(_closestSeenEntityPosition);
        }
        
        if (!_controller.moving)
        {
            _controller.MoveToRandomPosition(mapArea);
        }
        
        _shooting.currentTime -= Time.deltaTime;
        if (_shooting.currentTime <= 0)
        {
            _shooting.Shoot();
            
            _shooting.currentTime = _shooting.fireRate;
        }
    }
    #endregion

    #region Switch State Conditions

    private void SwitchToWandering()
    {
        _controller.StopMovement();
        _controller.SetSprintSpeed();
        generalState = GeneralState.Wandering;
    }
    private void SwitchToLooting()
    {
        _controller.StopMovement();
        _controller.SetSprintSpeed();
        generalState = GeneralState.Looting;
    }
    private void SwitchToShooting()
    {
        _controller.StopMovement();
        _controller.SetWalkSpeed();
        generalState = GeneralState.Shooting;
    }


    #endregion
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
                break;
            
            case GeneralState.MouseMode:
                DoMouseMode();
                break;
            
            case GeneralState.Wandering:
                DoWandering();
                // Wandering to Looting
                if (_closestSeenLootPosition != _unseenVector) 
                {
                    SwitchToLooting();
                }
                // Wandering to Shooting
                if (_closestSeenEntityPosition != _unseenVector && GetComponent<Entity>().Loot >= 1)
                {
                    SwitchToShooting();
                }
                break;

            case GeneralState.Looting:
                DoLooting();
                
                // Looting to Wandering
                if (_closestSeenLootPosition == _unseenVector)
                {
                    SwitchToWandering();
                }
                // Looting to Shooting
                if (_closestSeenEntityPosition != _unseenVector && _entity.Loot >= 1)
                {
                    SwitchToShooting();
                }
                break;
            
            case GeneralState.Shooting:
                DoShooting();
                // Shooting to Wandering
                if (_closestSeenEntityPosition == _unseenVector || _entity.Loot <= 0)
                {
                    SwitchToWandering();
                }
                break;
        }
    }
    
    // Called every tick time
    private void TickUpdate()
    {
        _closestSeenLootPosition = ClosestSeenPosition(_gameManager.lootsList);
        _closestSeenEntityPosition = ClosestSeenPosition(_gameManager.entitiesList);
    }

    private void Start()
    {
        _transform = transform;
        _transformModel = _transform.Find("Model");
        _controller = GetComponent<EntityController>();
        _shooting = GetComponent<Shooting>();
        _gameManager = FindObjectOfType<GameManager>();
        _entity = GetComponent<Entity>();
        currentTime = tick;
        mapArea = new Rect(0f, 0f, _controller.map.size.x - 1, _controller.map.size.y - 1);
    }
    
    private void OnDrawGizmos()
    {
        _transform = transform;
        _transformModel = transform.Find("Model");

        // Sense distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_transform.position, viewDistance);
        
        // Field of view
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_transform.position, _transformModel.rotation * new Vector2(Mathf.Sin(Mathf.Deg2Rad * fieldOfView), Mathf.Cos(Mathf.Deg2Rad * fieldOfView)) * viewDistance);
        Gizmos.DrawRay(_transform.position, _transformModel.rotation * new Vector2(Mathf.Sin(Mathf.Deg2Rad * fieldOfView), -Mathf.Cos(Mathf.Deg2Rad * fieldOfView)) * viewDistance);

        Gizmos.color = Color.magenta;
        if (_closestSeenLootPosition != _unseenVector)
        {
            Gizmos.DrawLine(_transform.position, _closestSeenLootPosition);
        }
        
        Gizmos.color = Color.green;
        if (_closestSeenEntityPosition != _unseenVector)
        {
            Gizmos.DrawLine(_transform.position, _closestSeenEntityPosition);
        }
    }
}