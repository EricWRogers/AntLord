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
    public GameObject mouseIndicator, cellIndicator, cellIndicatorObj;
    public MarchingCubes voxelTerrain;
    public int selectedObjectIndex = -1;
    protected Vector3 lastPosition;
    public LayerMask placementLayermask;
    public Grid grid;
    public event Action OnClicked, OnExit;
    public bool inBuildMode = false;
    float rotation = 0.0f;
    protected float angle;

    void Start()
    {
        StopPlacement();
    }
    protected virtual void Update()
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
            rotation = (rotation + 90.0f >= 360) ? 0 : rotation + 90.0f;
            Debug.Log(rotation);
        }
        else if (inBuildMode && Input.GetKeyDown(KeyCode.E))
        {
            cellIndicatorObj.transform.Rotate(0.0f, -90.0f, 0.0f);
            rotation = (rotation - 90.0f <= -360) ? 0 : rotation - 90.0f;
            Debug.Log(rotation);
        }
        //tracks the mouse by grid
        Vector3 mousePosition = GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        mouseIndicator.transform.position = mousePosition;

        cellIndicator.transform.position = (angle >= 45.0f) ? grid.CellToWorld(grid.WorldToCell(mousePosition)) + Vector3.up : grid.CellToWorld(grid.WorldToCell(mousePosition));
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
        cellIndicatorObj.transform.position += (new Vector3(0.5f, 0.0f, 0.5f) * cellIndicatorObj.transform.localScale.x);
        cellIndicator.SetActive(true);
        //assigning methods to the events
        OnClicked += PlaceStruct;
        OnExit += StopPlacement;
    }
    public void StopPlacement()
    {
        inBuildMode = false;
        selectedObjectIndex = -1;
        cellIndicator.SetActive(false);
        cellIndicatorObj.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        //removes previous invocation of event
        OnClicked -= PlaceStruct;
        OnExit -= StopPlacement;
    }
    protected virtual void PlaceStruct()
    {
        if (IsPointerOverUI() || ResourceManager.instance.GetFood() < allBuildings[selectedObjectIndex].buildCost)
        {
            return;
        }
        bool canBuild = true;
        Vector3 mousePosition = GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        //places the building at the selectedIndex at these grid coords
        GameObject buildingParent = allBuildings[selectedObjectIndex].preFab;
        buildingParent.transform.localScale = allBuildings[selectedObjectIndex].size;

        GameObject building = buildingParent.transform.GetChild(0).gameObject;
        building.transform.Rotate(0.0f, rotation, 0.0f);

        //45 because all the voxels should be perfect right triangles (weirdly some are like 54)
        //if its a 45 incline or more just bump the building up one so we dont have to bother with a bunch of conditionals
        buildingParent.transform.position = (angle >= 45.0f) ? grid.CellToWorld(gridPosition) + Vector3.up : grid.CellToWorld(gridPosition);


        //these are to set whatever the terrain we choose
        if (voxelTerrain != null && voxelTerrain.enabled)
        {
            canBuild = voxelTerrain.SetVoxel(
                building.transform.position, // le center of the brush
                building.transform.localScale.x * 3.0f);//le radius of the brush a bit bigger than normal to get surronding tiles
        }
        if (canBuild)
        {
            Instantiate(buildingParent);
            ResourceManager.instance.AddFood(-allBuildings[selectedObjectIndex].buildCost);
        }
        else
        {
            Debug.Log("NOPE");
        }

    }
    //look sometimes Lambda functions just do this
    public bool IsPointerOverUI()
    => EventSystem.current.IsPointerOverGameObject();

    //tracks mouse position, ignores objects not rendered, shoots a raycast for whatever the building layer is
    protected virtual Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, placementLayermask))
        {
            lastPosition = hit.point;
            // i had to draw a diagram for this
            //and remember stuff from physics which was about 2 years ago
            angle = Vector3.Angle(hit.normal, Vector3.up);
        }
        return lastPosition;
    }



    public void AssignActions()
    {
        OnClicked += PlaceStruct;
        OnExit += StopPlacement;
    }
    public void DismissActions()
    {
        OnClicked -= PlaceStruct;
        OnExit -= StopPlacement;
    }
    protected void TriggerClick()
    {
        OnClicked?.Invoke();
    }

    protected void TriggerExit()
    {
        OnExit?.Invoke();
    }
}
