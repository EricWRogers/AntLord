using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public class BuildingPlacementSystem : MonoBehaviour
{
    private Camera sceneCamera; //no touchie leave alone

    [Header("Building Placement Data")]
    public List<BuildingSO> allBuildings;
    public MarchingCubes voxelTerrain;
    public Grid grid;
    public GameObject firstBuilding;
    private BuildingSO currentBuilding;

    [Header("Building State")]
    public int selectedObjectIndex = -1;
    protected Vector3 lastPosition;
    protected float angle;
    public bool inBuildMode = false;
    float rotation = 0.0f;
    public LayerMask placementLayermask;
    public GameObject mouseIndicator;
    public GameObject cellIndicator;
    private GameObject cellIndicatorObj;
    public event Action OnClicked, OnExit;

    // ===== VR / UI SETTINGS =====
    [Header("VR & UI")]
    public GameObject buildingUI;
    private float spawnDistance = 10.0f;
    private TMP_Text nameText;
    private TMP_Text descText;
    private TMP_Text costText;
    private TMP_Text healthText;
    public FlyCam DesktopCam;

    void Start()
    {
        DesktopCam = FindFirstObjectByType<FlyCam>();
        cellIndicatorObj = cellIndicator.transform.GetChild(0).gameObject;
        nameText = buildingUI.transform.GetChild(0).GetChild(1).GetChild(0).gameObject.GetComponent<TMP_Text>();//high risk = high reward
        descText = buildingUI.transform.GetChild(0).GetChild(1).GetChild(1).gameObject.GetComponent<TMP_Text>();
        costText = buildingUI.transform.GetChild(0).GetChild(1).GetChild(2).gameObject.GetComponent<TMP_Text>();
        healthText = buildingUI.transform.GetChild(0).GetChild(1).GetChild(3).gameObject.GetComponent<TMP_Text>();
        if (!sceneCamera) sceneCamera = Camera.main;
        StopPlacement();
        voxelTerrain.SetVoxelWithTerritory(
            firstBuilding.transform.position,
            firstBuilding.transform.localScale.x * 3.0f,
            true);

    }
    protected virtual void Update()
    {
        if (DesktopCam != null)
        {
            //events listening for the keypresses
            if (Input.GetMouseButtonDown(0))//if you want to crash unity delete Down
            {
                OnClicked?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                inBuildMode = !inBuildMode;
                if (inBuildMode)
                {
                    OpenBuildMenu();
                }
                OnExit?.Invoke();
            }
        }
        //tracks BuildingUI to always face the player.
        buildingUI.transform.LookAt(new Vector3(sceneCamera.gameObject.transform.position.x, buildingUI.transform.position.y, sceneCamera.gameObject.transform.position.z));
        buildingUI.transform.forward *= -1;

        //tracks the mouse by grid
        Vector3 mousePosition = GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        mouseIndicator.transform.position = mousePosition;

        cellIndicator.transform.position = (angle >= 45.0f) ? grid.CellToWorld(grid.WorldToCell(mousePosition)) + Vector3.up : grid.CellToWorld(grid.WorldToCell(mousePosition));

        switch (currentBuilding.type)
        {
            case BuildingSO.BuildingType.ResourceExtraction:
                if (voxelTerrain.CheckVoxel(grid.WorldToCell(mousePosition), cellIndicatorObj.transform.localScale.x * 3.0f, false))
                    cellIndicatorObj.GetComponent<Renderer>().material.SetColor("_Diffuse", Color.green);

                else
                    cellIndicatorObj.GetComponent<Renderer>().material.SetColor("_Diffuse", Color.red);
                break;

            case BuildingSO.BuildingType.MilitaryIndustiralComplex:
                if (voxelTerrain.CheckVoxel(grid.WorldToCell(mousePosition), cellIndicatorObj.transform.localScale.x * 3.0f))
                    cellIndicatorObj.GetComponent<Renderer>().material.SetColor("_Diffuse", Color.green);

                else
                    cellIndicatorObj.GetComponent<Renderer>().material.SetColor("_Diffuse", Color.red);
                break;
        }

    }
    public void StartPlacement()
    {
        buildingUI.SetActive(false);
        inBuildMode = true;
        if (selectedObjectIndex < 0)
        {
            UnityEngine.Debug.Log($"NO ID FOUND: {selectedObjectIndex}");
            return;
        }
        Debug.Log(selectedObjectIndex);
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
        cellIndicator.SetActive(false);
        cellIndicatorObj.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        //removes previous invocation of event
        OnClicked -= PlaceStruct;
        OnExit -= StopPlacement;
    }
    protected virtual void PlaceStruct()
    {
        if (IsPointerOverUI() || ResourceManager.instance.rocks < allBuildings[selectedObjectIndex].RockCost || ResourceManager.instance.sticks < allBuildings[selectedObjectIndex].StickCost)
        {
            return;
        }
        bool canBuild = true;
        Vector3 mousePosition = GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        //places the building at the selectedIndex at these grid coords
        GameObject buildingParent = currentBuilding.preFab;
        buildingParent.transform.localScale = currentBuilding.size;

        GameObject building = buildingParent.transform.GetChild(0).gameObject;
        building.transform.Rotate(0.0f, rotation, 0.0f);

        //45 because all the voxels should be perfect right triangles (weirdly some are like 54)
        //if its a 45 incline or more just bump the building up one so we dont have to bother with a bunch of conditionals
        buildingParent.transform.position = (angle >= 45.0f) ? grid.CellToWorld(gridPosition) + Vector3.up : grid.CellToWorld(gridPosition);

        //these are to set whatever the terrain we choose
        if (voxelTerrain != null && voxelTerrain.enabled)
        {
            switch (currentBuilding.type)
            {
                //checks if a resource building that can be placed anywhere except ontop of buildings
                case BuildingSO.BuildingType.ResourceExtraction:
                    canBuild = voxelTerrain.SetVoxelWithoutTerritory(
                        building.transform.position,  // le center of the brush
                        building.transform.localScale.x * 3.0f);//le radius of the brush a bit bigger than normal to get surronding tiles
                    break;

                //checks if a defense building that must be built in own territory
                case BuildingSO.BuildingType.MilitaryIndustiralComplex:
                    canBuild = voxelTerrain.SetVoxelWithTerritory(
                        building.transform.position, // le center of the brush
                        building.transform.localScale.x * 3.0f);//le radius of the brush a bit bigger than normal to get surronding tiles
                    break;
            }
        }
        if (canBuild)
        {
            Instantiate(buildingParent);
            //esourceManager.instance.AddFood(-allBuildings[selectedObjectIndex].buildCost);
            ResourceManager.instance.AddRock(-allBuildings[selectedObjectIndex].RockCost);
            ResourceManager.instance.AddStick(-allBuildings[selectedObjectIndex].StickCost);
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
            angle = Vector3.Angle(hit.normal, Vector3.up);
        }
        return lastPosition;
    }
    public void OpenBuildMenu()
    {
        buildingUI.transform.position = sceneCamera.gameObject.transform.position +
            new Vector3(sceneCamera.gameObject.transform.forward.x, 0, sceneCamera.gameObject.transform.forward.z).normalized * spawnDistance;
        buildingUI.SetActive(true);
        selectedObjectIndex = 0;
        SetBuildingInfo(allBuildings[selectedObjectIndex]);

    }
    public void NextBuilding()
    {
        selectedObjectIndex = (selectedObjectIndex >= allBuildings.Count - 1) ? 0 : selectedObjectIndex + 1;
        SetBuildingInfo(allBuildings[selectedObjectIndex]);
    }

    public void PreviousBuilding()
    {
        selectedObjectIndex = (selectedObjectIndex <= 0) ? allBuildings.Count - 1 : selectedObjectIndex - 1;
        SetBuildingInfo(allBuildings[selectedObjectIndex]);
    }
    public void Build()
    {
        StartPlacement();
    }
    public void SetBuildingInfo(BuildingSO building)
    {
        currentBuilding = building;

        nameText.text = building.buildName;
        descText.text = building.buildDesc;
        costText.text = $"Stick Cost: {building.StickCost} Rock Cost: {building.RockCost}";
        healthText.text = $"Health: {building.buildHealth}";
    }

    //these are for VR invoking events
    //you cant directly invoke another classes events even if theyre inherited
    protected void TriggerClick()
    {
        OnClicked?.Invoke();
    }

    protected void TriggerExit()
    {
        OnExit?.Invoke();
    }
}
