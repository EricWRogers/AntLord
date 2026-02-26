using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class BuildingPlacementSystem : MonoBehaviour
{
//I mean this is pretty close to what we need.
    [SerializeField] private Camera sceneCamera;
    public List<BuildingSO> allBuildings;
    [SerializeField] private GameObject mouseIndicator, cellIndicator, cellIndicatorObj;
    public int selectedObjectIndex = -1;
    Vector3 lastPosition;
    [SerializeField] private LayerMask placementLayermask;
    [SerializeField] private Grid grid;
    public event Action OnClicked, OnExit;
    public bool inBuildMode = false;
    public float rotation = 0.0f;

    void Start()
    {
        StopPlacement();
    }
    void Update()
    {
        //events listening for the keypresses
        if (Input.GetMouseButtonDown(0))//if you want to crash unity delete Down
        {
            OnClicked?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnExit?.Invoke();
        }
        if (inBuildMode && Input.GetKeyDown(KeyCode.Q))
        {
            cellIndicatorObj.transform.Rotate(0.0f, 90.0f, 0.0f);
            rotation = (rotation + 90.0f >= 360)? 0 : rotation + 90.0f;
            Debug.Log(rotation);
        }
        else if(inBuildMode && Input.GetKeyDown(KeyCode.E))
        {
            cellIndicatorObj.transform.Rotate(0.0f, -90.0f, 0.0f);
            rotation = (rotation - 90.0f <= -360)? 0 : rotation - 90.0f;
            Debug.Log(rotation);
        }
        //tracks the mouse by grid
        Vector3 mousePosition = GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(grid.WorldToCell(mousePosition));
    }
    public void StartPlacement(int ID)
    {
        StopPlacement();
        inBuildMode = true;
        //checks to see if the index is in the list of buildings
        selectedObjectIndex = allBuildings.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex < 0)
        {
            UnityEngine.Debug.Log($"NO ID FOUND: {ID}");
            return;
        }
        cellIndicatorObj.transform.localScale = allBuildings[selectedObjectIndex].size;
        cellIndicatorObj.transform.position += (new Vector3(0.5f,0.0f,0.5f) * cellIndicatorObj.transform.localScale.x);
        cellIndicator.SetActive(true);
        //assigning methods to the events
        OnClicked += PlaceStruct;
        OnExit += StopPlacement;
    }
    void StopPlacement()
    {   inBuildMode = false;
        selectedObjectIndex = -1;
        cellIndicator.SetActive(false);
        cellIndicatorObj.transform.localPosition = new Vector3(0.0f,0.5f,0.0f);
        //removes previous invocation of event
        OnClicked -= PlaceStruct;
        OnExit -= StopPlacement;
    }
    void PlaceStruct()
    {
        if (IsPointerOverUI())
        {
            return;
        }
        Vector3 mousePosition = GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        //places the building at the selectedIndex at these grid coords
        GameObject buildingParent = Instantiate(allBuildings[selectedObjectIndex].preFab);
        buildingParent.transform.localScale = allBuildings[selectedObjectIndex].size;
        Debug.Log(rotation);
        GameObject building = buildingParent.transform.GetChild(0).gameObject;
        building.transform.Rotate(0.0f, rotation, 0.0f);
        //buildingParent.transform.localRotation = (0.0f, rotation, 0.0f);
        buildingParent.transform.position = grid.CellToWorld(gridPosition);

    }
    //look sometimes Lambda functions just do this
    public bool IsPointerOverUI()
    => EventSystem.current.IsPointerOverGameObject();

//tracks mouse position, ignores objects not rendered, shoots a raycast for whatever the building layer is
    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, placementLayermask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }

}
