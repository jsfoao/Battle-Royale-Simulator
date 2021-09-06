using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GameCamera : MonoBehaviour
{
    private Map _map;
    private Transform _transform;
    private Camera _camera;

    [Header("Movement")] 
    [SerializeField] private float movementSpeed = 10f;

    private Vector3 direction = Vector3.zero;
    
    [Header("Zoom")] 
    [SerializeField] private float currentZoom;
    [SerializeField] private float minZoom = 20f;
    [SerializeField] private float maxZoom = 5f;
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float zoomSpeed = 20f;

    private void CameraMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            direction.y = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            direction.y = -1;
        }
        else
        {
            direction.y = 0;
        }

        if (Input.GetKey(KeyCode.D))
        {
            direction.x = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            direction.x = -1;
        }
        else
        {
            direction.x = 0;
        }

        direction = direction.normalized;
        _transform.position += direction * movementSpeed * Time.deltaTime;
    }

    private void CameraZoom()
    {
        currentZoom -= Input.mouseScrollDelta.y * sensitivity;
        currentZoom = Mathf.Clamp(currentZoom, maxZoom, minZoom);
        float newSize = Mathf.MoveTowards(_camera.orthographicSize, currentZoom, zoomSpeed * Time.deltaTime);
        _camera.orthographicSize = newSize;
    }
    
    private void Update()
    {
        CameraMovement();
        CameraZoom();
    }

    private void Start()
    {
        _transform = transform;
        _camera = GetComponent<Camera>();
    }
}
