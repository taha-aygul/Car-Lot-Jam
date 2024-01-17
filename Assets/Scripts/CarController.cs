using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Transform rotationToForward;
    public float velocity, exitCarVelocity;
    public Vector3 leftEnterPoint = new Vector3(-3.65f, 0, -1.89f);
    public Vector3 rightEnterPoint = new Vector3(-3.65f, 0, -1.89f);
    public ParticleSystem smokeParticle, smokeTrail;
    public Transform seatPos;
    public ParticleSystem emoji;
    public Outline outline;

    private GameObject selectedHuman;
    List<Vector2Int> backCells = new List<Vector2Int>();
    List<Vector2Int> frontCells = new List<Vector2Int>();
    [SerializeField] Animator carController;
    Animator humanController;
    bool leftDoor;
    ObjectOnCellData myData;
    CellData[] path;
    Vector3[] totalPath;
    Pathfinder pathfinder;
    public CarState carState;
   // [HideInInspector]
    public enum CarState
    {
        Empty,  // does nothing
        Idle,   // waits for suitable exit road
        Moving, // moving to exit
        Exit,   // reached exit
    }


    private void Start()
    {
        pathfinder = Pathfinder.Instance;
        myData = GetComponent<GridObjectConnection>().myData;
        carState = CarState.Empty;
    }

    private void Update()
    {
        if (carState == CarState.Idle)
        {
            SearchWayToAsphalt();
        }
    }


    public void GetHumanData(GameObject human, bool isSelectedLeftDoor, Animator humanController)
    {
        selectedHuman = human;
        leftDoor = isSelectedLeftDoor;
        this.humanController = humanController;
    }

    public void EnterHuman(Tween tween)
    {
        StartCoroutine("GetHuman", tween);
    }
    public IEnumerator GetHuman(Tween tween)
    {
        LevelGenerator.Instance.DeOccupyCell(selectedHuman.GetComponent<GridObjectConnection>().myData.occupiedCellsIndexes[0]);
        for (int i = 0; i < selectedHuman.GetComponent<GridObjectConnection>().myData.occupiedCellsIndexes.Length; i++)
        {
            selectedHuman.GetComponent<GridObjectConnection>().myData.occupiedCellsIndexes[i] = Vector2Int.zero;
        }
        // Wait until human completes its move
        yield return tween.WaitForCompletion();

        outline.OutlineColor = Color.clear;

        // Human enters to car from left or right
        selectedHuman.transform.parent = transform;
        if (leftDoor)
        {
            selectedHuman.transform.DOLocalMove(leftEnterPoint, 0.2f);
            humanController.SetBool("isEnteringCarLeft", true);
            carController.SetBool("openLeftDoor", true);
        }
        else
        {
            selectedHuman.transform.DOLocalMove(rightEnterPoint, 0.2f);
            selectedHuman.transform.localEulerAngles = new Vector3(0, -90, 0);
            carController.SetBool("openRightDoor", true);
            humanController.SetBool("isEnteringCarRight", true);
        }

        // Human enter dotween
        var seq = DOTween.Sequence();
        seq.Append(selectedHuman.transform.DOLookAt(seatPos.position, 0.2f));
        seq.AppendInterval(0.3f);
        seq.Append(selectedHuman.transform.DOMove(seatPos.position, 1.5f));
        seq.Join(selectedHuman.transform.DOScale(Vector3.one * 0.1f, 1.5f));
        seq.Play();

       
    }

    private void InitializeFrontAndBackCells()
    {
        // Initializting car's front and back path.

        Vector2Int cellfront = myData.occupiedCellsIndexes[0];
        Vector2Int cellback = myData.occupiedCellsIndexes[myData.occupiedCellsIndexes.Length - 1];

        // According to direction
        // Front cells starts from the first cell the car occupied
        // Back cells starts from the last cell the car occupied

        switch (myData.objectDirection)
        {
            case ObjectOnCellData.ObjectDirection.Right:
                cellfront = myData.occupiedCellsIndexes[myData.occupiedCellsIndexes.Length - 1];
                cellback = myData.occupiedCellsIndexes[0];
                if (myData.occupiedCellsIndexes.Length == 3)
                {
                    cellfront = myData.occupiedCellsIndexes[0];
                    cellback = myData.occupiedCellsIndexes[myData.occupiedCellsIndexes.Length - 1];
                }
                cellfront.x--;
                cellback.x++;
                while (LevelGenerator.Instance.GetCellData(cellfront).selectable)
                {
                    cellfront.x--;
                    frontCells.Add(cellfront);
                }
                while (LevelGenerator.Instance.GetCellData(cellback).selectable)
                {
                    cellback.x++;
                    backCells.Add(cellback);
                }
                break;
            case ObjectOnCellData.ObjectDirection.Left:
                cellfront = myData.occupiedCellsIndexes[myData.occupiedCellsIndexes.Length - 1];
                cellback = myData.occupiedCellsIndexes[0];
                if (myData.occupiedCellsIndexes.Length == 3)
                {
                    cellfront = myData.occupiedCellsIndexes[0];
                    cellback = myData.occupiedCellsIndexes[myData.occupiedCellsIndexes.Length - 1];
                }

                cellfront.x++;
                cellback.x--;

                while (LevelGenerator.Instance.GetCellData(cellfront).selectable)
                {
                    cellfront.x++;
                    frontCells.Add(cellfront);
                }
                while (LevelGenerator.Instance.GetCellData(cellback).selectable)
                {
                    cellback.x--;
                    backCells.Add(cellback);
                }
                break;
            case ObjectOnCellData.ObjectDirection.Forward:

                while (LevelGenerator.Instance.GetCellData(cellfront).selectable)
                {
                    cellfront.y--;
                    frontCells.Add(cellfront);
                }
                while (LevelGenerator.Instance.GetCellData(cellback).selectable)
                {
                    cellback.y++;
                    backCells.Add(cellback);
                }
                break;
            case ObjectOnCellData.ObjectDirection.Back:
                while (LevelGenerator.Instance.GetCellData(cellfront).selectable)
                {
                    cellfront.y++;
                    frontCells.Add(cellfront);
                }
                while (LevelGenerator.Instance.GetCellData(cellback).selectable)
                {
                    cellback.y--;
                    backCells.Add(cellback);
                }
                break;
            default:
                break;
        }
    }

    private void SearchWayToAsphalt()
    {
        // First check front cells
        if (isPathAvaliable(frontCells))
        {

            carState = CarState.Moving;
           
            // Get path
            path = pathfinder.CalculatePath(frontCells[frontCells.Count - 1], LevelGenerator.Instance.levelData.exitCoordinate, true);

            // Initialize total path
            totalPath = new Vector3[frontCells.Count + path.Length+1];

            // Get path positions
            for (int i = 0; i < frontCells.Count; i++)
            {
                totalPath[i] = LevelGenerator.Instance.GetCellData(frontCells[i]).cellPosition;
            }
            for (int i = 0; i < path.Length; i++)
            {
                totalPath[frontCells.Count + i] = LevelGenerator.Instance.GetCellData(path[i].cellIndex).cellPosition;
            }

            totalPath[totalPath.Length - 1] = LevelGenerator.Instance.finishPoint;
            float arrivalTime = totalPath.Length / velocity;
            float exitTime = 20 / exitCarVelocity;

            // Deoccupy current cells
            for (int i = 0; i < myData.occupiedCellsIndexes.Length; i++)
            {
                LevelGenerator.Instance.DeOccupyCell(myData.occupiedCellsIndexes[i]);
            }

            var seq = DOTween.Sequence();
            seq.Append(transform.DOPath(totalPath, arrivalTime, PathType.Linear, PathMode.Full3D, 10, Color.black)
               .SetEase(Ease.Linear)
               .OnWaypointChange(OnWaypointReached));
           
            /*seq.Append(transform.DOLookAt(LevelGenerator.Instance.finishPoint, 0.4f));
            seq.Join(transform.DOMove(LevelGenerator.Instance.finishPoint, exitTime));*/
            seq.Play();
        }
        else if (isPathAvaliable(backCells))
        {
            // Check back cells

            carState = CarState.Moving;

            // Get path
            path = pathfinder.CalculatePath(backCells[backCells.Count - 1], LevelGenerator.Instance.levelData.exitCoordinate, true);
            
            // Initialize total path
            totalPath = new Vector3[backCells.Count + path.Length + 3];

            // Get path positions
            if (frontCells.Count > 1)
            {
                for (int i = 0; i < backCells.Count; i++)
                {
                    totalPath[i] = LevelGenerator.Instance.GetCellData(backCells[i]).cellPosition;
                }
            }

            List<Vector2Int> neighboors = pathfinder.GetNeighbors(backCells[backCells.Count - 1], true);
            Vector2Int add = Vector2Int.zero;
            for (int i = 0; i < neighboors.Count; i++)
            {
                if (neighboors[i] != path[0].cellIndex)
                {
                    add = neighboors[i];
                    break;
                }
            }
            totalPath[backCells.Count] = LevelGenerator.Instance.GetCellData(add).cellPosition;
            totalPath[backCells.Count + 1] = LevelGenerator.Instance.GetCellData(backCells[backCells.Count - 1]).cellPosition;
            for (int i = 0; i < path.Length; i++)
            {
                totalPath[backCells.Count + i + 2] = LevelGenerator.Instance.GetCellData(path[i].cellIndex).cellPosition;
            }

            float arrivalTime = totalPath.Length / velocity;
            float exitTime = 20 / exitCarVelocity;
            
            // Deoccupy current cells
            for (int i = 0; i < myData.occupiedCellsIndexes.Length; i++)
            {
                LevelGenerator.Instance.DeOccupyCell(myData.occupiedCellsIndexes[i]);
            }
            totalPath[totalPath.Length - 1] = LevelGenerator.Instance.finishPoint;

            // Dotween that moves the car 

            var seq = DOTween.Sequence();
            seq.Append(transform.DOPath(totalPath, arrivalTime, PathType.Linear, PathMode.Full3D, 10, Color.red)
               .SetEase(Ease.Linear)
               .SetOptions(AxisConstraint.Y)
               .OnWaypointChange(OnWaypointReachedReverse));
           
           /* seq.Join(transform.DOLookAt(LevelGenerator.Instance.finishPoint, 0.5f));
            seq.Append(transform.DOMove(LevelGenerator.Instance.finishPoint, exitTime).SetEase(Ease.Linear));*/
            seq.Play();

        }
    }

    void OnWaypointReached(int waypointIndex)
    {
        if (waypointIndex < totalPath.Length)
        {
            Vector3 lookAtPosition = totalPath[waypointIndex];
            transform.DOLookAt(lookAtPosition, 0.4f);
        }
        if (waypointIndex == totalPath.Length-2)
        {
            LevelGenerator.Instance.CarLeftParkingLot();
            ExitBarrierController.Instance.OpenExitBarrier();
            emoji.Play();
        }
        if (waypointIndex == 1)//frontCells.Count)
        {
            for (int i = 0; i < myData.occupiedCellsIndexes.Length; i++)
            {
                myData.occupiedCellsIndexes[i] = Vector2Int.zero;
            }
        }
    }
    void OnWaypointReachedReverse(int waypointIndex)
    {
        if (waypointIndex < totalPath.Length)
        {
            Vector3 lookAtPosition = totalPath[waypointIndex];
            if (waypointIndex <= backCells.Count)
            {
                lookAtPosition = totalPath[waypointIndex - 1];
            }
            lookAtPosition.y = transform.position.y;
            transform.DOLookAt(lookAtPosition, 0.4f);
        }

        if (waypointIndex == totalPath.Length - 2)
        {
            LevelGenerator.Instance.CarLeftParkingLot();
            ExitBarrierController.Instance.OpenExitBarrier();
            emoji.Play();
        }
        if (waypointIndex == 1)//backCells.Count)
        {
            for (int i = 0; i < myData.occupiedCellsIndexes.Length; i++)
            {
                myData.occupiedCellsIndexes[i] = Vector2Int.zero;
            }
        }
    }



    private bool isPathAvaliable(List<Vector2Int> path)
    {

        for (int i = 0; i < path.Count; i++)
        {
            if (LevelGenerator.Instance.GetCellData(path[i]).occupied)
            {
                return false;
            }
        }
        return true;
    }


    public void IdleState()
    {
        InitializeFrontAndBackCells();
        carState = CarState.Idle;
        smokeParticle.Play();
        smokeTrail.Play();
    }
}
