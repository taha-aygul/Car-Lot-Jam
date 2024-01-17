using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Custom/LevelData", order = 1)]

public class LevelData : ScriptableObject, ISerializationCallbackReceiver
{
    public int coin=10;
    public float gridOffset;
    public GameObject gridPrefab;
    public GameObject roadStraightPrefab;
    public GameObject roadTurningPrefab;
    public GameObject roadIntersectionPrefab;
    public GameObject exitBarrierPrefab;
    public GameObject barrierPrefab;
    public GameObject conePrefab;
    public GameObject humanPrefab;
    public GameObject car2xPrefab;
    public GameObject car3xPrefab;

    [HideInInspector] public int gridSizeX;
    [HideInInspector] public int gridSizeY;
    [HideInInspector] public int gridX;
    [HideInInspector] public int gridY;
    [HideInInspector] public Color selectedColor = Color.white;
    [HideInInspector] public string selectedValue;
    [HideInInspector] public Color defaultGridColor = Color.gray;
    [HideInInspector] public Color defaultRoadColor = Color.cyan;
    [HideInInspector] public bool resetDone;


    [HideInInspector] public ColorEnum colorEnum;
    [HideInInspector] public ObjectEnum objectEnum;
    [HideInInspector] public Direction direction;
    [HideInInspector] public ExitPoint exitPoint;
    [HideInInspector] public ExitPointAlignment exitPointAlignment;


    public string humanInGrid = "H";
    public string car2xInGridFront = "C2F";
    public string car2xInGridBack = "C2R";
    public string car3xInGridFront = "C3F";
    public string car3xInGridBack = "C3R";
    public string coneInGrid = "C";
    public string barrierInGrid = "B";
    public string emptyInGrid = "x";
    public string roadInGrid = "R";

    [HideInInspector] public Vector2Int exitCoordinate;
    public ColorData colorData;

    // public CellData[,] cells;
    [HideInInspector] public CellData[] cells_1d;

    [HideInInspector] public List<ObjectOnCellData> cellObjects;

    public Color coneColor;
    public Color barrierColor;

    [HideInInspector] public int carCount;
    public enum ColorEnum
    {
        None, Purple, Black, Blue, Green, Orange, Pink, Red, Yellow
    }
    public enum ObjectEnum
    {
        Human, Car2x, Car3x, Cone, Barrier, Delete
    }
    public enum Direction
    {
        Right, Left, Forward, Back,
    }
    public enum ExitPoint
    {
        TopRight, TopLeft, BottomLeft, BottomRight
    }
    public enum ExitPointAlignment
    {
        RightLeft, TopDown
    }

    public void ResetCells()
    {
        resetDone = false;
        carCount = 0;
        gridX = gridSizeX + 2;
        gridY = gridSizeY + 2;
        //cells = new CellData[gridX, gridY];
        cells_1d = new CellData[gridX * gridY];
        Debug.Log("RESETT");
        Vector3 spawnPosition = Vector3.zero;

        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                if (y == 0 || y == gridY - 1 || x == 0 || x == gridX - 1)
                {

                    cells_1d[(y * gridX) + x] = new CellData(defaultRoadColor, roadInGrid, new Vector2Int(x, y));
                    spawnPosition.y = 0.1f;
                    cells_1d[(y * gridX) + x].cellPosition = spawnPosition;
                    
                    cells_1d[(y * gridX) + x].selectable = false;
                    cells_1d[(y * gridX) + x].cellBasePrefab = roadStraightPrefab;
                    if ((y == 0 && x == 0) || (y == gridY - 1 && x == 0) || (x == gridX - 1 && y == 0) || (y == gridY - 1 && x == gridX - 1))
                    {
                        cells_1d[(y * gridX) + x].cellBasePrefab = roadTurningPrefab;
                    }

                    if (y == 0 || y == gridSizeY + 1)
                    {
                        cells_1d[(y * gridX) + x].cellRotation = new Vector3(0, 90, 0);
                    }
                    if (x == 0 && y == 0)
                    {
                        cells_1d[(y * gridX) + x].cellRotation = new Vector3(0, 0, 0);
                    }
                    if (x == gridSizeX + 1 && y == 0)
                    {
                        cells_1d[(y * gridX) + x].cellRotation = new Vector3(0, 90, 0);
                    }
                    if (x == 0 && y == gridSizeY + 1)
                    {
                        cells_1d[(y * gridX) + x].cellRotation = new Vector3(0, 270, 0);
                    }
                    if (x == gridSizeX + 1 && y == gridSizeY + 1)
                    {
                        cells_1d[(y * gridX) + x].cellRotation = new Vector3(0, 180, 0);
                    }
                }
                else
                {
                    cells_1d[(y * gridX) + x] = new CellData(defaultGridColor, emptyInGrid, new Vector2Int(x, y));
                    cells_1d[(y * gridX) + x].cellPosition = spawnPosition;
                    cells_1d[(y * gridX) + x].cellBasePrefab = gridPrefab;
                }
                spawnPosition.x += gridOffset;
                spawnPosition.y = 0;

            }
            spawnPosition.z -= gridOffset;
            spawnPosition.x = 0;
        }

        exitCoordinate = Vector2Int.zero;
        Vector3Int exitRotation;
        switch (exitPoint)
        {
            case ExitPoint.TopRight:
                exitCoordinate.x = gridX - 1;
                exitCoordinate.y = 0;
                switch (exitPointAlignment)
                {
                    case ExitPointAlignment.RightLeft:
                        exitRotation = new Vector3Int(0, 90, 0);
                        break;
                    case ExitPointAlignment.TopDown:
                        exitRotation = new Vector3Int(0, 180, 0);
                        break;
                    default:
                        exitRotation = new Vector3Int(0, 90, 0);
                        break;
                }
                break;
            case ExitPoint.TopLeft:
                exitCoordinate.x = 0;
                exitCoordinate.y = 0;
                switch (exitPointAlignment)
                {
                    case ExitPointAlignment.RightLeft:
                        exitRotation = new Vector3Int(0, 90, 0);
                        break;
                    case ExitPointAlignment.TopDown:
                        exitRotation = new Vector3Int(0, 0, 0);
                        break;
                    default:
                        exitRotation = new Vector3Int(0, 90, 0);
                        break;
                }
                break;
            case ExitPoint.BottomLeft:
                exitCoordinate.x = 0;
                exitCoordinate.y = gridY - 1;
                switch (exitPointAlignment)
                {
                    case ExitPointAlignment.RightLeft:
                        exitRotation = new Vector3Int(0, 270, 0);
                        break;
                    case ExitPointAlignment.TopDown:
                        exitRotation = new Vector3Int(0, 0, 0);
                        break;
                    default:
                        exitRotation = new Vector3Int(0, 90, 0);
                        break;
                }
                break;
            case ExitPoint.BottomRight:
                exitCoordinate.x = gridX - 1;
                exitCoordinate.y = gridY - 1;
                switch (exitPointAlignment)
                {
                    case ExitPointAlignment.RightLeft:
                        exitRotation = new Vector3Int(0, 270, 0);
                        break;
                    case ExitPointAlignment.TopDown:
                        exitRotation = new Vector3Int(0, 180, 0);
                        break;
                    default:
                        exitRotation = new Vector3Int(0, 90, 0);
                        break;
                }
                break;
            default:
                exitCoordinate.x = 0;
                exitCoordinate.y = gridY - 1;
                exitRotation = Vector3Int.zero;
                break;
        }

        cells_1d[(exitCoordinate.y * gridX) + exitCoordinate.x].cellRotation = exitRotation;
        cells_1d[(exitCoordinate.y * gridX) + exitCoordinate.x].cellBasePrefab = roadIntersectionPrefab;
        cells_1d[(exitCoordinate.y * gridX) + exitCoordinate.x].cellColor = Color.white;
        cells_1d[(exitCoordinate.y * gridX) + exitCoordinate.x].cellValue = "E";
        resetDone = true;
    }



    /*private void OnValidate()
    {
        Undo.RecordObject(this, "Resize");
        EditorUtility.SetDirty(this);

    }*/
    public void ResetObjects()
    {
        cellObjects.Clear();
    }
    public CellData GetCellData(int x, int y)
    {
        return cells_1d[(y * gridX) + x];
    }
    public void SetCellData(int x, int y, CellData cell, bool occupied)
    {
        cells_1d[(y * gridX) + x].cellColor = cell.cellColor;
        cells_1d[(y * gridX) + x].cellValue = cell.cellValue;
        cells_1d[(y * gridX) + x].cellIndex = cell.cellIndex;
        cells_1d[(y * gridX) + x].occupied = occupied;
    }
    public CellData GetCellData(Vector2Int index)
    {
        return cells_1d[(index.y * gridX) + index.x];
    }


    public void FirstDrawCells()
    {
        if (cells_1d == null)
        {
            Debug.Log("here f");
            ResetCells();
            //ResetObjects();
        }
    }

    public void OnBeforeSerialize()
    {
        /* int counter = 0;

         for (int y = 0; y < gridSizeY + 2; y++)
         {
             for (int x = 0; x < gridSizeX + 2; x++)
             {
                 Debug.Log(counter + " " + gridSizeX + 2 * gridSizeY + 2);
                 cells_1d[counter] = cells[x, y];
                 counter++;
             }
         }*/
    }

    public void OnAfterDeserialize()
    {

    }
}
[System.Serializable]
public class CellData
{
    public Color cellColor = Color.white;
    public string cellValue;
    public GameObject cellBasePrefab;
    public Vector3 cellRotation;
    public Vector3 cellPosition;
    public Vector2Int cellIndex;
    public bool selectable = true; // Which grid is road
    public bool occupied = false;  // Does grid cell has object on it
    public ObjectOnCellData objectOnIt;

    public CellData(Color cellColor, string cellValue, Vector2Int cellIndex)
    {
        this.cellIndex = cellIndex;
        this.cellColor = cellColor;
        this.cellValue = cellValue;
    }
}

[System.Serializable]
public class ObjectOnCellData
{

    public GameObject objectPrefab; // And spawn object
    public Color objectColor;
    public Material objectMaterial;
    public ObjectDirection objectDirection;
    public Vector2Int[] occupiedCellsIndexes; // Holds occupied cells indexes 
    public enum ObjectDirection
    {
        None, Right, Left, Forward, Back,
    }

    public ObjectOnCellData(GameObject objectPrefab, Color objectColor, ObjectDirection objectDirection, Vector2Int[] occupiedCells)
    {
        this.objectPrefab = objectPrefab;
        this.objectColor = objectColor;
        this.objectDirection = objectDirection;
        this.occupiedCellsIndexes = occupiedCells;
    }
}

