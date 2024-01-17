using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ClickController : MonoBehaviour
{

    //[SerializeField] LevelData myLevelData;
    [SerializeField] float velocity;
    [SerializeField] float cellColorTime;
    // public static LevelData levelData;

    bool humanSelected;
    [SerializeField] GameObject selectedHuman;
    [SerializeField] GameObject selectedCell;
    [SerializeField] GameObject selectedCar;
    [SerializeField] ObjectOnCellData selectedHumanData;
    [SerializeField] CellData targetCell;
    ObjectOnCellData selectedCarData;
    Animator humanController;
    Vector3[] pathPoints;
    bool isSelectedDoorLeft;
    LevelGenerator levelGenerator;

    public static ClickController Instance;
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

    private void Start()
    {
        levelGenerator = LevelGenerator.Instance;
    }


    // Update is called once per frame
    void Update()
    {
        HandleClick();
    }

    public void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                // Human selected before
                if (selectedHuman != null)
                {
                    // Human goes to car
                    if (hit.collider.CompareTag("Car"))
                    {
                        SelectCar(hit.collider.gameObject);
                    }
                    else if (hit.collider.CompareTag("Cell"))
                    {
                        // Human goes to cell

                        // Check is cell occupied with a object
                        GameObject objectOnCell = levelGenerator.GetGameObject(hit.collider.GetComponent<CellController>().myData.cellIndex);
                        if (objectOnCell == null)
                        {
                            selectedCell = hit.collider.gameObject;
                            StartCoroutine(MoveHuman(hit.collider.GetComponent<CellController>().myData, 0));
                        }
                        else
                        {
                            // If cell occupied with car
                            if (objectOnCell.CompareTag("Car"))
                            {
                                SelectCar(objectOnCell);
                            }
                            else if (objectOnCell.CompareTag("Human"))
                            {
                                // If cell occupied with human
                                // If clicked same human deselect human

                                if (objectOnCell != selectedHuman)
                                {
                                    DeselectHuman();
                                    SelectHuman(objectOnCell);
                                }
                                else
                                {
                                    DeselectHuman();
                                }
                            }
                        }

                    }
                    else if (hit.collider.CompareTag("Human"))
                    {
                        // If clicked same human deselect human
                        // If clicked another human deselect currently selected human and select new human
                        if (selectedHuman != hit.collider.gameObject)
                        {
                            DeselectHuman();
                            SelectHuman(hit.collider.gameObject);
                        }
                        else
                        {
                            DeselectHuman();
                        }
                    }
                }
                else
                {
                    // Human not selected before
                    if (hit.collider.CompareTag("Human"))
                    {
                        SelectHuman(hit.collider.gameObject);
                    }
                    else if (hit.collider.CompareTag("Cell"))
                    {
                        // If clicked to the cell and cell is occupied with human
                        GameObject objectOnCell = levelGenerator.GetGameObject(hit.collider.GetComponent<CellController>().myData.cellIndex);
                        if (objectOnCell != null)
                        {
                            if (objectOnCell.CompareTag("Human"))
                            {
                                SelectHuman(objectOnCell);
                            }
                        }

                    }
                }
            }
        }

    }
    private void DeselectHuman()
    {
        selectedHuman.GetComponent<HumanEmojiController>().outline.OutlineColor = Color.clear;
        selectedHuman = null;
        selectedHumanData = null;
        targetCell = null;
    }

    private void SelectHuman(GameObject human)
    {
        selectedHuman = human;
        selectedHuman.GetComponent<HumanEmojiController>().outline.OutlineColor = Color.green;
        humanController = selectedHuman.GetComponent<Animator>();
        selectedHumanData = human.GetComponent<GridObjectConnection>().myData;
    }

    private IEnumerator MoveHuman(CellData cell, int moveType)
    {
        // moveType = 1  => moving to the car
        // moveType = 0  => moving to  cell

        targetCell = cell;
        Pathfinder pathfinder = Pathfinder.Instance;

        // Get path as indexes of cells
        CellData[] path = pathfinder.CalculatePath(selectedHumanData.occupiedCellsIndexes[0], targetCell.cellIndex, false);

        // Human can move to clicked position
        if (path != null)
        {
            // Succesful move effects
            selectedHuman.GetComponent<HumanEmojiController>().happy.Play();
            if (moveType == 0)
            {
                StartCoroutine(PaintCellGreen(selectedCell));
            }

            // Converting coordinates to exact positions
            pathPoints = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                pathPoints[i] = path[i].cellPosition;
            }
            float arrivalTime = pathPoints.Length / velocity;


            humanController.SetBool("isRunning", true);
            Animator controller = null;

            // DOtween human movement

            Tween runTween = selectedHuman.transform.DOPath(pathPoints, arrivalTime, PathType.Linear, PathMode.Full3D, 10, Color.black)
                .SetEase(Ease.Linear)
                .SetOptions(AxisConstraint.Y)
                .OnWaypointChange(OnWaypointReached)
                .SetLookAt(0.1f)
                .OnStart(() =>
                {
                    controller = humanController;
                })
                .OnComplete(() =>
                {
                    // When tween is done
                    if (controller != null && moveType == 0)
                    {
                        controller.SetBool("isRunning", false);
                    }
                });

            // Do necessary updates to the cells
            levelGenerator.DeOccupyCell(selectedHuman.GetComponent<GridObjectConnection>().myData.occupiedCellsIndexes[0]);
            selectedHuman.GetComponent<GridObjectConnection>().myData.occupiedCellsIndexes[0] = targetCell.cellIndex;
            levelGenerator.OccupyCell(targetCell.cellIndex);

            // Move to the car
            if (moveType == 1)
            {
                selectedCar.GetComponent<CarController>().GetHumanData(selectedHuman, isSelectedDoorLeft, humanController);
                selectedCar.GetComponent<CarController>().EnterHuman(runTween);
            }
            DeselectHuman();
            yield return runTween.WaitForCompletion();
        }
        else
        {
            // Human cannot move to selected pos
            selectedHuman.GetComponent<HumanEmojiController>().angry.Play();
            DeselectHuman();

            if (moveType == 0)
            {
                StartCoroutine(PaintCellRed(selectedCell));
            }
            else
            {
                selectedCar.GetComponent<CarController>().outline.OutlineColor = Color.clear;
            }
        }

    }
    void OnWaypointReached(int waypointIndex)
    {
        // Hedef noktaya doðru bak
        if (waypointIndex < pathPoints.Length)
        {
            Vector3 lookAtPosition = pathPoints[waypointIndex];
            selectedHuman.transform.DOLookAt(lookAtPosition, 0.1f);
        }
    }
    private IEnumerator PaintCellGreen(GameObject cell)
    {
        Color oldColor = cell.GetComponent<MeshRenderer>().material.color;
        cell.GetComponent<MeshRenderer>().material.color = Color.green;
        yield return new WaitForSeconds(cellColorTime);
        cell.GetComponent<MeshRenderer>().material.color = oldColor;

    }
    private IEnumerator PaintCellRed(GameObject cell)
    {
        Color oldColor = cell.GetComponent<MeshRenderer>().material.color;
        cell.GetComponent<MeshRenderer>().material.color = Color.red;
        yield return new WaitForSeconds(cellColorTime);
        cell.GetComponent<MeshRenderer>().material.color = oldColor;

    }


    private void SelectCar(GameObject car)
    {
        selectedCar = car;
        selectedCarData = car.GetComponent<GridObjectConnection>().myData;
        Pathfinder pathfinder = Pathfinder.Instance;


        // If colors are matched then get data of the car

        if (selectedCarData.objectColor == selectedHumanData.objectColor)
        {

            selectedCar.GetComponent<CarController>().outline.OutlineColor = Color.green;
            Vector2Int carOccupiedCell = selectedCarData.occupiedCellsIndexes[0];
            Vector2Int targetCell = Vector2Int.zero;
            Vector2Int humanCell = selectedHumanData.occupiedCellsIndexes[0];
            Vector2Int leftDoorCell, rightDoorCell;

            // Check car alignment
            switch (selectedCarData.objectDirection)
            {
                case ObjectOnCellData.ObjectDirection.Right:
                    leftDoorCell = new Vector2Int(carOccupiedCell.x, carOccupiedCell.y + 1);
                    rightDoorCell = new Vector2Int(carOccupiedCell.x, carOccupiedCell.y - 1);
                    break;
                case ObjectOnCellData.ObjectDirection.Left:
                    leftDoorCell = new Vector2Int(carOccupiedCell.x, carOccupiedCell.y - 1);
                    rightDoorCell = new Vector2Int(carOccupiedCell.x, carOccupiedCell.y + 1);
                    break;
                case ObjectOnCellData.ObjectDirection.Forward:
                    leftDoorCell = new Vector2Int(carOccupiedCell.x - 1, carOccupiedCell.y);
                    rightDoorCell = new Vector2Int(carOccupiedCell.x + 1, carOccupiedCell.y);
                    break;
                case ObjectOnCellData.ObjectDirection.Back:

                    leftDoorCell = new Vector2Int(carOccupiedCell.x + 1, carOccupiedCell.y);
                    rightDoorCell = new Vector2Int(carOccupiedCell.x - 1, carOccupiedCell.y);
                    break;
                default:
                    leftDoorCell = Vector2Int.zero;
                    rightDoorCell = Vector2Int.zero;
                    break;
            }

            // Check cars doors and find which one is closer and moveable

            if (Vector2.Distance(humanCell, leftDoorCell) <= Vector2.Distance(humanCell, rightDoorCell))
            {
                CellData[] path = pathfinder.CalculatePath(selectedHumanData.occupiedCellsIndexes[0], leftDoorCell, false);
                if (IsCellTraversable(leftDoorCell) && path != null)
                {
                    isSelectedDoorLeft = true;
                    targetCell = leftDoorCell;
                }
                else if (IsCellTraversable(rightDoorCell))
                {
                    isSelectedDoorLeft = false;
                    targetCell = rightDoorCell;
                }
                else
                {
                    selectedCar.GetComponent<CarController>().outline.OutlineColor = Color.clear;
                }
            }
            else
            {
                CellData[] path = pathfinder.CalculatePath(selectedHumanData.occupiedCellsIndexes[0], rightDoorCell, false);

                if (IsCellTraversable(rightDoorCell) && path != null)
                {
                    isSelectedDoorLeft = false;
                    targetCell = rightDoorCell;
                }
                else if (IsCellTraversable(leftDoorCell))
                {
                    isSelectedDoorLeft = true;
                    targetCell = leftDoorCell;
                }
                else
                {
                    selectedCar.GetComponent<CarController>().outline.OutlineColor = Color.clear;

                }

            }

            // Start to move human

            StartCoroutine(MoveHuman(levelGenerator.GetCellData(targetCell.x, targetCell.y), 1));

        }
        else
        {
            selectedCar = null;
        }

    }


    private bool IsCellTraversable(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= levelGenerator.levelData.gridSizeX + 2 || cellIndex.y < 0 || cellIndex.y >= levelGenerator.levelData.gridSizeY + 2)
        {
            return false;
        }

        CellData cell = levelGenerator.GetCellData(cellIndex.x, cellIndex.y);

        return cell.cellValue == levelGenerator.levelData.emptyInGrid;
    }
}
