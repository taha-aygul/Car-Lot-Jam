using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    [SerializeField] public LevelData levelData;
    [SerializeField] List<GameObject> cellGameObjects;
    public Vector3 finishPoint;
    public Vector2Int exitPoint;
    GameObject levelParent;
    [SerializeField] CellData[] cells;
    [SerializeField] List<ObjectOnCellData> objDatas;
    [SerializeField] Camera normalCam, topOrthographicCameras;
    [SerializeField] int cameraChangeTresholdSizeY = 6;
    public int carCount;

    public static LevelGenerator Instance;

    void Awake()
    {
        MakeSingleton();
    }

    private void MakeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    public void GenerateLevel()
    {
        int gridY = levelData.gridY;
        int gridX = levelData.gridX;

        SetCamera(gridY - 2);

        carCount = levelData.carCount;
        cells = new CellData[levelData.cells_1d.Length];

        cells = levelData.cells_1d;


        levelParent = new GameObject("Level");

        // Spawning grid and roads
        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                int index = (y * gridX) + x;
                GameObject cellGO = Instantiate(cells[index].cellBasePrefab, levelParent.transform);
                cellGO.AddComponent<CellController>();
                cellGO.GetComponent<CellController>().myData = cells[index];
                cellGO.transform.position = cells[index].cellPosition; //spawnPosition;
                cellGO.transform.eulerAngles = cells[index].cellRotation;
                cellGO.name = "x." + x + " y." + y + " , index." + index;
            }
        }

        // Spawning exit point 
        float gridOff = levelData.gridOffset;
        Vector3 exitSpawnStartPoint;
        Vector3 exitSpawnRot;
        Vector3 exitBarrierSpawnPoint;
        Vector3 exitBarrierSpawnRot = Vector3.zero;

        switch (levelData.exitPoint)
        {
            case LevelData.ExitPoint.TopRight:
                exitSpawnStartPoint = cells[(0 * levelData.gridX) + gridX - 1].cellPosition;
                break;
            case LevelData.ExitPoint.TopLeft:
                exitSpawnStartPoint = cells[(0 * levelData.gridX) + 0].cellPosition;
                break;
            case LevelData.ExitPoint.BottomLeft:
                exitSpawnStartPoint = cells[(gridY - 1) * gridX + 0].cellPosition;
                break;
            case LevelData.ExitPoint.BottomRight:
                exitSpawnStartPoint = cells[(gridY - 1) * gridX + gridX - 1].cellPosition;
                break;
            default:
                exitSpawnStartPoint = cells[0].cellPosition;
                break;
        }
        switch (levelData.exitPointAlignment)
        {
            case LevelData.ExitPointAlignment.RightLeft:
                exitSpawnRot = new Vector3(0, 90, 0);
                break;
            case LevelData.ExitPointAlignment.TopDown:
                exitSpawnRot = Vector3.zero;
                break;
            default:
                exitSpawnRot = Vector3.zero;
                break;
        }

        exitBarrierSpawnPoint = exitSpawnStartPoint;

        if (levelData.exitPointAlignment == LevelData.ExitPointAlignment.RightLeft)
        {
            if (levelData.exitPoint == LevelData.ExitPoint.TopRight || levelData.exitPoint == LevelData.ExitPoint.BottomRight)
            {
                exitBarrierSpawnPoint.x += gridOff;
                exitBarrierSpawnPoint.z -= gridOff;
                exitBarrierSpawnRot = new Vector3(0, 270, 0);
            }
            else if (levelData.exitPoint == LevelData.ExitPoint.TopLeft || levelData.exitPoint == LevelData.ExitPoint.BottomLeft)
            {
                exitBarrierSpawnPoint.x -= gridOff;
                exitBarrierSpawnPoint.z += gridOff;
                exitBarrierSpawnRot = new Vector3(0, 90, 0);
            }
        }
        else if (levelData.exitPointAlignment == LevelData.ExitPointAlignment.TopDown)
        {
            if (levelData.exitPoint == LevelData.ExitPoint.BottomLeft || levelData.exitPoint == LevelData.ExitPoint.BottomRight)
            {
                exitBarrierSpawnPoint.x -= gridOff;
                exitBarrierSpawnPoint.z -= gridOff;
                exitBarrierSpawnRot = new Vector3(0, 0, 0);
            }
            else if (levelData.exitPoint == LevelData.ExitPoint.TopLeft || levelData.exitPoint == LevelData.ExitPoint.TopRight)
            {
                exitBarrierSpawnPoint.x += gridOff;
                exitBarrierSpawnPoint.z += gridOff;
                exitBarrierSpawnRot = new Vector3(0, 180, 0);
            }
        }


        //Spawning exit roads parent
        GameObject exitRoads = new GameObject("ExitRoads");
        exitRoads.transform.parent = levelParent.transform;

        // Spawning exit barrier
        GameObject exitBarrier = Instantiate(levelData.exitBarrierPrefab, exitRoads.transform);
        exitBarrier.transform.position = exitBarrierSpawnPoint;
        exitBarrier.transform.eulerAngles = exitBarrierSpawnRot;

        // Spawning exit roads
        Vector3 spawnPoint = exitSpawnStartPoint;
        for (int i = 0; i < 20; i++)
        {
            // Calculating roads direction
            if (levelData.exitPointAlignment == LevelData.ExitPointAlignment.RightLeft)
            {
                if (levelData.exitPoint == LevelData.ExitPoint.BottomLeft || levelData.exitPoint == LevelData.ExitPoint.TopLeft)
                {
                    spawnPoint.x -= gridOff;
                }
                else if (levelData.exitPoint == LevelData.ExitPoint.BottomRight || levelData.exitPoint == LevelData.ExitPoint.TopRight)
                {
                    spawnPoint.x += gridOff;
                }
            }
            else if (levelData.exitPointAlignment == LevelData.ExitPointAlignment.TopDown)
            {
                if (levelData.exitPoint == LevelData.ExitPoint.TopRight || levelData.exitPoint == LevelData.ExitPoint.TopLeft)
                {
                    spawnPoint.z += gridOff;
                }
                else if (levelData.exitPoint == LevelData.ExitPoint.BottomRight || levelData.exitPoint == LevelData.ExitPoint.BottomLeft)
                {
                    spawnPoint.z -= gridOff;
                }
            }

            // Instantiating road
            GameObject road = Instantiate(levelData.roadStraightPrefab, exitRoads.transform);
            road.transform.position = spawnPoint;
            road.transform.eulerAngles = exitSpawnRot;
            finishPoint = road.transform.position;
        }

        GenerateObjects(levelParent);

    }



    private void SetCamera(int gridY)
    {
        if (gridY < cameraChangeTresholdSizeY)
        {
            topOrthographicCameras.enabled = false;
            normalCam.enabled = true;
            topOrthographicCameras.gameObject.SetActive(false);
            normalCam.gameObject.SetActive(true);
        }
        else
        {
            topOrthographicCameras.enabled = true;
            normalCam.enabled = false;
            topOrthographicCameras.gameObject.SetActive(true);
            normalCam.gameObject.SetActive(false);

        }

    }

    private void GenerateObjects(GameObject parent)
    {

        objDatas = levelData.cellObjects;

        for (int i = 0; i < objDatas.Count; i++)
        {
            Vector2Int[] occCells = objDatas[i].occupiedCellsIndexes;
            List<Vector3> posList = new List<Vector3>();
            for (int j = 0; j < occCells.Length; j++)
            {
                posList.Add(levelData.GetCellData(occCells[j].x, occCells[j].y).cellPosition);
            }

            Vector3 spawnPos = levelData.GetCellData(occCells[0].x, occCells[0].y).cellPosition;

            if (objDatas[i].objectPrefab == levelData.car3xPrefab)
            {
                spawnPos = levelData.GetCellData(occCells[1].x, occCells[1].y).cellPosition;
            }
            if (objDatas[i].objectPrefab == levelData.barrierPrefab)
            {
                spawnPos = CalculateAveragePosition(posList);
            }

            // Selecting correct rotation
            Vector3 spawnRot;
            switch (objDatas[i].objectDirection)
            {
                case ObjectOnCellData.ObjectDirection.None:
                    spawnRot = Vector3.zero;
                    break;
                case ObjectOnCellData.ObjectDirection.Forward:
                    spawnRot = Vector3.zero;
                    break;
                case ObjectOnCellData.ObjectDirection.Right:
                    spawnRot = new Vector3(0, 270, 0);
                    break;
                case ObjectOnCellData.ObjectDirection.Back:
                    spawnRot = new Vector3(0, 180, 0);
                    break;
                case ObjectOnCellData.ObjectDirection.Left:
                    spawnRot = new Vector3(0, 90, 0);
                    break;
                default:
                    spawnRot = new Vector3(0, 0, 0);
                    break;
            }

            // Instantiating object
            GameObject newGO = Instantiate(objDatas[i].objectPrefab, parent.transform);
            newGO.AddComponent<GridObjectConnection>().myData = objDatas[i];

            // Adding correct material
            if (objDatas[i].objectMaterial != null)
            {
                if (objDatas[i].objectPrefab == levelData.humanPrefab)
                {
                    newGO.GetComponentInChildren<SkinnedMeshRenderer>().material = objDatas[i].objectMaterial;
                }
                else
                {
                    MeshRenderer[] meshRenderers = newGO.GetComponentsInChildren<MeshRenderer>();
                    for (int j = 0; j < meshRenderers.Length; j++)
                    {
                        meshRenderers[j].material = objDatas[i].objectMaterial;
                    }
                }


            }

            newGO.transform.position = spawnPos;
            newGO.transform.eulerAngles = spawnRot;
            cellGameObjects.Add(newGO);
        }
    }
    private Vector3 CalculateAveragePosition(List<Vector3> pos)
    {
        Vector3 totPos = Vector3.zero;
        for (int i = 0; i < pos.Count; i++)
        {
            totPos += pos[i];
        }
        return totPos / pos.Count;
    }

    public void DeleteLevel()
    {
        cellGameObjects.Clear();
        GameObject.DestroyImmediate(GameObject.Find("Level"));
        if (levelParent != null)
        {
            GameObject.DestroyImmediate(levelParent);
        }
        //levelData.ResetCells();
        //levelData.ResetObjects();
    }

    public void CarLeftParkingLot()
    {
        carCount--;
        if (carCount <= 0)
        {
            GameManager.Instance.LevelFinished();
        }
    }

   

    public GameObject GetGameObject(Vector2Int coordinate)
    {
        for (int i = 0; i < cellGameObjects.Count; i++)
        {
            Vector2Int[] occCell = cellGameObjects[i].GetComponent<GridObjectConnection>().myData.occupiedCellsIndexes;
            for (int j = 0; j < occCell.Length; j++)
            {
                if (coordinate == occCell[j])
                {
                    return cellGameObjects[i];
                }
            }
        }
        return null;
    }
    public CellData GetCellData(int x, int y)
    {
        return cells[(y * levelData.gridX) + x];
    }
    public CellData GetCellData(Vector2Int index)
    {
        return cells[(index.y * levelData.gridX) + index.x];
    }
    public void OccupyCell(int x, int y)
    {
        cells[(y * levelData.gridX) + x].occupied = true;
    }
    public void DeOccupyCell(int x, int y)
    {
        cells[(y * levelData.gridX) + x].occupied = false;

    }
    public void OccupyCell(Vector2Int index)
    {
        cells[(index.y * levelData.gridX) + index.x].occupied = true;
    }
    public void DeOccupyCell(Vector2Int index)
    {
        cells[(index.y * levelData.gridX) + index.x].occupied = false;
        cells[(index.y * levelData.gridX) + index.x].cellValue = levelData.emptyInGrid;
    }
    public int GridX()
    {
        return levelData.gridX;
    }
    public int GridY()
    {
        return levelData.gridY;
    }
}

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{

    private LevelGenerator myTarget;
    private SerializedObject soTarget;
    private void OnEnable()
    {
        myTarget = (LevelGenerator)target;
        soTarget = new SerializedObject(target);
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(50);
        GUI.color = Color.green;

        if (GUILayout.Button("Generate", GUILayout.Width(300), GUILayout.Height(30)))
        {
            myTarget.GenerateLevel();
        }

        GUILayout.Space(30);
        GUI.color = Color.red;
        if (GUILayout.Button("Delete", GUILayout.Width(300), GUILayout.Height(20)))
        {
            Debug.Log("delete");
            myTarget.DeleteLevel();
        }
        EditorUtility.SetDirty(target);


    }
}
