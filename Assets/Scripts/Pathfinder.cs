using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{

    public static Pathfinder Instance;

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
    LevelGenerator levelGenerator;
    private void Start()
    {
        levelGenerator = LevelGenerator.Instance;
    }
    public CellData[] CalculatePath(Vector2Int startCellIndex, Vector2Int targetCellIndex, bool forCars)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(startCellIndex);
        visited.Add(startCellIndex);

        while (queue.Count > 0)
        {
            Vector2Int currentCellIndex = queue.Dequeue();

            if (currentCellIndex == targetCellIndex)
            {
                // Hedefe ulaþýldý, yolu oluþtur
                List<CellData> path = new List<CellData>();
                Vector2Int current = targetCellIndex;

                while (current != startCellIndex)
                {
                    path.Add(levelGenerator.GetCellData(current.x, current.y));//  ClickController.levelData.GetCellData(current.x, current.y));
                    current = parentMap[current];
                }

                path.Reverse();
                return path.ToArray();
            }

            // Komþu hücrelere bak
            foreach (Vector2Int neighbor in GetNeighbors(currentCellIndex, forCars))
            {
                if (forCars)
                {
                    if (!visited.Contains(neighbor) && IsCellTraversableForCar(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                        parentMap[neighbor] = currentCellIndex;
                    }
                }
                else
                {
                    if (!visited.Contains(neighbor) && IsCellTraversable(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                        parentMap[neighbor] = currentCellIndex;
                    }
                }

            }
        }

        // Hedefe ulaþýlamadý
        return null;
    }

    public List<Vector2Int> GetNeighbors(Vector2Int cellIndex, bool forCars)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Sað, sol, yukarý, aþaðý komþular
        Vector2Int rightNeighbor = new Vector2Int(cellIndex.x + 1, cellIndex.y);
        Vector2Int leftNeighbor = new Vector2Int(cellIndex.x - 1, cellIndex.y);
        Vector2Int upNeighbor = new Vector2Int(cellIndex.x, cellIndex.y + 1);
        Vector2Int downNeighbor = new Vector2Int(cellIndex.x, cellIndex.y - 1);

        // Çapraz komþularý eklemeyin
        if (forCars)
        {
            if (IsCellTraversableForCar(rightNeighbor)) neighbors.Add(rightNeighbor);
            if (IsCellTraversableForCar(leftNeighbor)) neighbors.Add(leftNeighbor);
            if (IsCellTraversableForCar(upNeighbor)) neighbors.Add(upNeighbor);
            if (IsCellTraversableForCar(downNeighbor)) neighbors.Add(downNeighbor);
        }
        else
        {
            if (IsCellTraversable(rightNeighbor)) neighbors.Add(rightNeighbor);
            if (IsCellTraversable(leftNeighbor)) neighbors.Add(leftNeighbor);
            if (IsCellTraversable(upNeighbor)) neighbors.Add(upNeighbor);
            if (IsCellTraversable(downNeighbor)) neighbors.Add(downNeighbor);
        }

        return neighbors;
    }

    private bool IsCellTraversable(Vector2Int cellIndex)
    {
        //print(cellIndex);
        //print(levelGenerator.levelData.gridX);
        levelGenerator = LevelGenerator.Instance;
        /*print(levelGenerator == null);
        print(levelGenerator.levelData == null);
        print(levelGenerator.levelData.gridSizeX);
        print(levelGenerator.GridX());*/

        if (cellIndex.x < 0 || cellIndex.x >= levelGenerator.GridX()  || cellIndex.y < 0 || cellIndex.y >= levelGenerator.GridY())
        {
            return false;
        }

        CellData cell = levelGenerator.GetCellData(cellIndex);
        // print(cell.cellValue + " " + ClickController.levelData.emptyInGrid);
        return !cell.occupied && cell.selectable;//cellValue == levelGenerator.levelData.emptyInGrid;
    }
    bool IsCellTraversableForCar(Vector2Int cellIndex)
    {
        levelGenerator = LevelGenerator.Instance;

        if (cellIndex.x < 0 || cellIndex.x >= levelGenerator.levelData.gridSizeX + 2 || cellIndex.y < 0 || cellIndex.y >= levelGenerator.levelData.gridSizeY + 2)
        {
            // Hücre sýnýrlarý dýþýnda

            return false;
        }

        CellData cell = levelGenerator.GetCellData(cellIndex.x, cellIndex.y);

        // Örneðin, hücre deðeri "x" ise geçilebilir
        // print(cell.cellValue + " " + ClickController.levelData.emptyInGrid);
        return cell.cellValue == levelGenerator.levelData.roadInGrid || cell.cellValue == "E";
    }

}
