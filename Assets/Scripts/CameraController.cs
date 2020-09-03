using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utils;
using Grid = Utils.Grid;

//control the camera movement
public class CameraController : Singleton<CameraController>
{
    private const float MoveSpeed = 5f; //the move speed of the camera
    private const float MinX = 0f; //minimum horizontal position of the camera
    private const float MaxX = Grid.GridWidth - 1; //maximum horizontal position of the camera
    private const float MinZ = -2f; //minimum forward position of the camera
    private const float MaxZ = 3f; //maximum forward position of the camera
    private const float DragSpeed = 2;
    private Vector3 _dragOrigin;
    
    private Camera _mainCamera; //the main camera component of scene
    
    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void FixedUpdate()
    {
        //move camera controller based on arrow key inputs
        float xMovement = Input.GetAxis("Horizontal") * Time.deltaTime * MoveSpeed;
        float yMovement = Input.GetAxis("Vertical") * Time.deltaTime * MoveSpeed;
        transform.Translate(xMovement, 0, yMovement);

        //move camera based on drag
        //from https://forum.unity.com/threads/click-drag-camera-movement.39513/
        if (Input.GetButtonDown("Drag"))
        {
            _dragOrigin = Input.mousePosition;
            return;
        }

        if (Input.GetButton("Drag"))
        {
            Vector3 pos = _mainCamera.ScreenToViewportPoint(Input.mousePosition - _dragOrigin);
            Vector3 move = new Vector3(pos.x * DragSpeed, 0, pos.y * DragSpeed);
 
            transform.Translate(move, Space.World);
        }
        
        //check for x & z boundaries (camera controller)
        Vector3 currentPosition = transform.position;

        if (currentPosition.x < MinX)
        {
            currentPosition = new Vector3(MinX, currentPosition.y, currentPosition.z);
            transform.position = currentPosition;
        }
        else if (currentPosition.x > MaxX)
        {
            currentPosition = new Vector3(MaxX, currentPosition.y, currentPosition.z);
            transform.position = currentPosition;
        }

        if (currentPosition.z < MinZ)
            transform.position = new Vector3(currentPosition.x, currentPosition.y, MinZ);
        else if (currentPosition.z > MaxZ)
            transform.position = new Vector3(currentPosition.x, currentPosition.y, MaxZ);
    }
}
