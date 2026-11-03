using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This script handle all the control code, so detecting when the users click on a unit or building and selecting those
/// If a unit is selected it will give the order to go to the clicked point or building when right clicking.
/// </summary>
public class UserControl : MonoBehaviour
{
    public Camera GameCamera;
    public float PanSpeed = 10.0f;
    public GameObject Marker;
    
    private Unit selected = null;

    private InputAction moveAction;
    private InputAction leftClickAction;
    private InputAction rightClickAction;

    private void Awake()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        leftClickAction = new InputAction("LeftClick", InputActionType.Button, "<Mouse>/leftButton");
        rightClickAction = new InputAction("RightClick", InputActionType.Button, "<Mouse>/rightButton");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        leftClickAction.Enable();
        rightClickAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        leftClickAction.Disable();
        rightClickAction.Disable();
    }

    private void Start()
    {
        Marker.SetActive(false);
    }

    public void HandleSelection()
    {
        var ray = GameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //the collider could be children of the unit, so we make sure to check in the parent
            var unit = hit.collider.GetComponentInParent<Unit>();
            selected = unit;
            
            
            //check if the hit object have a IUIInfoContent to display in the UI
            //if there is none, this will be null, so this will hid the panel if it was displayed
            var uiInfo = hit.collider.GetComponentInParent<MainUIHandler.IUIInfoContent>();
            MainUIHandler.Instance.SetNewInfoContent(uiInfo);
        }
    }

    public void HandleAction()
    {
        //right click give order to the unit
        var ray = GameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var building = hit.collider.GetComponentInParent<Building>();
            
            if (building != null)
            {
                selected.GoTo(building);
            }
            else
            {
                selected.GoTo(hit.point);
            }
        }
    }

    private void Update()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        GameCamera.transform.position = GameCamera.transform.position + new Vector3(move.y, 0, -move.x) * PanSpeed * Time.deltaTime;

        if (leftClickAction.WasPressedThisFrame())
        {
            HandleSelection();
        }
        else if (selected != null && rightClickAction.WasPressedThisFrame())
        {   
            HandleAction();
        }

        MarkerHandling();
    }
    
    // Handle displaying the marker above the unit that is currently selected (or hiding it if no unit is selected)
    void MarkerHandling()
    {
        if (selected == null && Marker.activeInHierarchy)
        {
            Marker.SetActive(false);
            Marker.transform.SetParent(null);
        }
        else if (selected != null && Marker.transform.parent != selected.transform)
        {
            Marker.SetActive(true);
            Marker.transform.SetParent(selected.transform, false);
            Marker.transform.localPosition = Vector3.zero;
        }    
    }
}
