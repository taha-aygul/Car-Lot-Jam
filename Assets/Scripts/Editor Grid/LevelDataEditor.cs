using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    int sizeX = 0, sizeY = 0;
    private LevelData myTarget;
    private SerializedObject soTarget;


    SerializedProperty m_gridSizeX;
    SerializedProperty m_gridSizeY;
    SerializedProperty m_exitPoint;
    SerializedProperty m_exitPointAlignment;
    SerializedProperty m_colorEnum;
    SerializedProperty m_objectEnum;
    SerializedProperty m_direction;


    private void OnEnable()
    {
        myTarget = (LevelData)target;
        // soTarget = new SerializedObject(target); /////////////////////////////////////////////////////// Bundan çözüm çıkabilir
        //Debug.Log("onenable");
        // myTarget.ResetCells();
        myTarget.FirstDrawCells();

        /*m_gridSizeX = serializedObject.FindProperty("gridSizeX");
        m_gridSizeY = serializedObject.FindProperty("gridSizeY");
        m_exitPoint = serializedObject.FindProperty("exitPoint");
        m_exitPointAlignment = serializedObject.FindProperty("exitPointAlignment");
        m_colorEnum = serializedObject.FindProperty("colorEnum");
        m_objectEnum = serializedObject.FindProperty("objectEnu");
        m_direction = serializedObject.FindProperty("direction");*/
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Debug.Log(myTarget.cells==null);
       

        EditorGUI.BeginChangeCheck();
        sizeX = myTarget.gridSizeX;
        sizeY = myTarget.gridSizeY;

        sizeX = EditorGUILayout.IntField("Grid Size X", sizeX);
        if (sizeX > 5)
        {
            sizeX = 5;
        }
        else if (sizeX < 1)
        {
            sizeX = 1;
        }

        myTarget.gridSizeX = sizeX;
        GUILayout.Space(5);

        sizeY = EditorGUILayout.IntField("Grid Size Y", sizeY);
        GUILayout.Space(5);
       if (sizeY < 1)
        {
            sizeY = 1;
        }
        myTarget.gridSizeY = sizeY;

        myTarget.exitPoint = (LevelData.ExitPoint)EditorGUILayout.EnumPopup("Select Exit Point ", myTarget.exitPoint);
        GUILayout.Space(5);
        myTarget.exitPointAlignment = (LevelData.ExitPointAlignment)EditorGUILayout.EnumPopup("Select Exit Point Alignment", myTarget.exitPointAlignment);
        GUILayout.Space(5);



        if (EditorGUI.EndChangeCheck() && !EditorApplication.isPlaying)
        {
            myTarget.ResetCells();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }


        myTarget.colorEnum = (LevelData.ColorEnum)EditorGUILayout.EnumPopup("Select Color ", myTarget.colorEnum);
        GUILayout.Space(5);
        myTarget.objectEnum = (LevelData.ObjectEnum)EditorGUILayout.EnumPopup("Select Object ", myTarget.objectEnum);
        GUILayout.Space(5);
        myTarget.direction = (LevelData.Direction)EditorGUILayout.EnumPopup("Select Direction ", myTarget.direction);

        GUILayout.Space(30);

        GUI.color = Color.green;
        if (GUILayout.Button("RESET", GUILayout.Width(300)))
        {
            myTarget.cellObjects.Clear();
            myTarget.ResetCells();
        }

        GUILayout.Space(10);

        /* if (myTarget.cells == null)
         {
             return;
         }*/

        int gridY = myTarget.gridSizeY + 2;
        int gridX = myTarget.gridSizeX + 2;
        //Debug.Log("oninspectorgui " + gridX + " " + gridY);



        EditorGUILayout.BeginVertical();

        for (int y = 0; y < gridY; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridX; x++)
            {
                if (myTarget.resetDone)
                {
                    string cell = myTarget.GetCellData(x, y).cellValue;
                    GUI.color = myTarget.GetCellData(x, y).cellColor;

                    // Create a GUIContent with the cell valueaxasd
                    GUIContent content = new GUIContent(cell);
                    // Get the rect for the button
                    Rect buttonRect = GUILayoutUtility.GetRect(content, GUI.skin.button, GUILayout.Width(40), GUILayout.Height(40));

                    // Draw the button with the current color
                    if (GUI.Button(buttonRect, content))
                    {
                        if (myTarget.cells_1d[y * gridX + x].selectable)
                        {
                            // Button clicked, update the color
                            ClickCell(x, y);
                            serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(target);
                           
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(50);
        GUI.color = Color.white;
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }

    private void ClickCell(int x, int y)
    {
        int gridLenghtX = myTarget.gridX;

        if (myTarget.cells_1d[y * gridLenghtX + x].cellColor != myTarget.defaultGridColor)
        {
            Debug.Log("Eklemeden önce silmelisin");
            return;
        }




        switch (myTarget.colorEnum)
        {
            case LevelData.ColorEnum.None:
                myTarget.selectedColor = myTarget.cells_1d[y * gridLenghtX + x].cellColor;
                break;
            case LevelData.ColorEnum.Purple:
                myTarget.selectedColor = Color.magenta;
                break;
            case LevelData.ColorEnum.Black:
                myTarget.selectedColor = new Color(0.7f, 0.7f,0.7f);
                break;
            case LevelData.ColorEnum.Blue:
                myTarget.selectedColor = Color.blue;
                break;
            case LevelData.ColorEnum.Green:
                myTarget.selectedColor = Color.green;
                break;
            case LevelData.ColorEnum.Orange:
                myTarget.selectedColor = new Color(15, 2.1f, 0);
                break;
            case LevelData.ColorEnum.Pink:
                myTarget.selectedColor = new Color(251, 0, 210);
                break;
            case LevelData.ColorEnum.Red:
                myTarget.selectedColor = Color.red;
                break;
            case LevelData.ColorEnum.Yellow:
                //myTarget.selectedColor = new Color(3.3f, 3f, 1.3f);
                myTarget.selectedColor = Color.yellow;
                break;
            default:
                break;
        }

        Vector2Int[] occCells;
        ObjectOnCellData newObj;
        ObjectOnCellData.ObjectDirection dir = ObjectOnCellData.ObjectDirection.None;
        Material selectedMaterial = myTarget.colorData.colorOption[0].humanMaterial;
        string value = "";
        string value2 = "";
        string value3 = "";
        Color color = myTarget.barrierColor;

        bool cellAvaliable = false;
        switch (myTarget.objectEnum)
        {
            case LevelData.ObjectEnum.Human:
                myTarget.selectedValue = myTarget.humanInGrid;
                myTarget.cells_1d[y * gridLenghtX + x].cellValue = myTarget.selectedValue;
                myTarget.cells_1d[y * gridLenghtX + x].cellColor = myTarget.selectedColor;
                myTarget.cells_1d[y * gridLenghtX + x].occupied = true;
                occCells = new Vector2Int[1];
                occCells[0] = new Vector2Int(x, y);


                for (int i = 0; i < myTarget.colorData.colorOption.Length; i++)
                {
                    if (myTarget.colorEnum.ToString().Equals(myTarget.colorData.colorOption[i].displayName))
                    {
                        selectedMaterial = myTarget.colorData.colorOption[i].humanMaterial;
                    }
                }

                newObj = new ObjectOnCellData(myTarget.humanPrefab, myTarget.selectedColor, ObjectOnCellData.ObjectDirection.None, occCells);
                newObj.objectMaterial = selectedMaterial;
                myTarget.cellObjects.Add(newObj);
                break;

            case LevelData.ObjectEnum.Cone:

                value = myTarget.coneInGrid;
                color = myTarget.coneColor;

                value = myTarget.barrierInGrid;
                color = myTarget.barrierColor;
                CellData cell = new CellData(color, value, new Vector2Int(x, y));
                cell.cellBasePrefab = myTarget.gridPrefab;

                myTarget.SetCellData(x, y, cell, true);

                myTarget.cells_1d[y * gridLenghtX + x].cellValue = myTarget.coneInGrid;
                myTarget.cells_1d[y * gridLenghtX + x].cellColor = myTarget.coneColor;
                occCells = new Vector2Int[1];
                occCells[0] = new Vector2Int(x, y);
                newObj = new ObjectOnCellData(myTarget.conePrefab, myTarget.coneColor, ObjectOnCellData.ObjectDirection.None, occCells);
                myTarget.cellObjects.Add(newObj);

                break;
            case LevelData.ObjectEnum.Barrier:            /// RENK HATASI FORWARD  /////////////////////////////////////////////////////////////////////////////////////////

                occCells = new Vector2Int[2];
                int barX = 0, barY = 0;
                switch (myTarget.direction)
                {
                    case LevelData.Direction.Right:
                        if (myTarget.GetCellData(x - 1, y).selectable && myTarget.GetCellData(x - 1, y).cellColor == myTarget.defaultGridColor)
                        {
                            barX = x - 1;
                            barY = y;
                            dir = ObjectOnCellData.ObjectDirection.Forward;
                            cellAvaliable = true;

                        }
                        break;
                    case LevelData.Direction.Left:
                        if (myTarget.GetCellData(x + 1, y).selectable && myTarget.GetCellData(x + 1, y).cellColor == myTarget.defaultGridColor)
                        {
                            barX = x + 1;
                            barY = y;
                            dir = ObjectOnCellData.ObjectDirection.Forward;
                            cellAvaliable = true;

                        }
                        break;
                    case LevelData.Direction.Forward:
                        Debug.Log("--" + myTarget.GetCellData(x, y + 1).selectable);
                        Debug.Log((y + 1) * gridLenghtX + x);

                        if (myTarget.GetCellData(x, y + 1).selectable && myTarget.GetCellData(x, y + 1).cellColor == myTarget.defaultGridColor)
                        {
                            barX = x;
                            barY = y + 1;
                            dir = ObjectOnCellData.ObjectDirection.Right;
                            cellAvaliable = true;

                        }
                        break;
                    case LevelData.Direction.Back:
                        if (myTarget.GetCellData(x, y - 1).selectable && myTarget.GetCellData(x, y - 1).cellColor == myTarget.defaultGridColor)
                        {
                            barX = x;
                            barY = y - 1;
                            dir = ObjectOnCellData.ObjectDirection.Right;
                            cellAvaliable = true;

                        }
                        break;
                    default:
                        barX = 0;
                        barY = 0;
                        dir = ObjectOnCellData.ObjectDirection.None;
                        cellAvaliable = false;

                        break;
                }

                if (barX == 0 && barY == 0)
                    return;

                value = myTarget.barrierInGrid;
                color = myTarget.barrierColor;
                CellData barcell = new CellData(color, value, new Vector2Int(x, y));
                CellData barcell2 = new CellData(color, value, new Vector2Int(barX, barY));
                barcell.cellBasePrefab = myTarget.gridPrefab;
                barcell2.cellBasePrefab = myTarget.gridPrefab;

                myTarget.SetCellData(x, y, barcell, true);
                myTarget.SetCellData(barX, barY, barcell2, true);


                /* myTarget.cells_1d[y * gridLenghtX + x].cellValue = myTarget.barrierInGrid;
                myTarget.cells_1d[y * gridLenghtX + x].cellColor = myTarget.barrierColor;
                myTarget.cells_1d[barY * gridLenghtX + barX].cellValue = myTarget.barrierInGrid;
                myTarget.cells_1d[barY * gridLenghtX + barX].cellColor = myTarget.barrierColor;*/

                Debug.Log(myTarget.objectEnum);
                Debug.Log(myTarget.direction);
                Debug.Log(x + " " + y);
                Debug.Log((y * gridLenghtX) + x);

                Debug.Log(barX + " " + barY);
                Debug.Log((barY * gridLenghtX + barX));

                occCells[0] = new Vector2Int(x, y);
                occCells[1] = new Vector2Int(barX, barY);
                newObj = new ObjectOnCellData(myTarget.barrierPrefab, myTarget.barrierColor, dir, occCells);
                myTarget.cellObjects.Add(newObj);


                break;
            case LevelData.ObjectEnum.Delete:
                myTarget.selectedValue = x + "," + y;
                myTarget.selectedColor = myTarget.defaultGridColor;

                break;
            case LevelData.ObjectEnum.Car2x:


                for (int i = 0; i < myTarget.colorData.colorOption.Length; i++)
                {
                    if (myTarget.colorEnum.ToString().Equals(myTarget.colorData.colorOption[i].displayName))
                    {
                        selectedMaterial = myTarget.colorData.colorOption[i].carMaterial;
                    }
                }


                occCells = new Vector2Int[2];
                int car2X = 0, car2Y = 0;
                switch (myTarget.direction)
                {
                    case LevelData.Direction.Right:
                        if (myTarget.GetCellData(x - 1, y).selectable && myTarget.GetCellData(x - 1, y).cellColor == myTarget.defaultGridColor)
                        {
                            car2X = x - 1;
                            car2Y = y;
                            dir = ObjectOnCellData.ObjectDirection.Left;
                            cellAvaliable = true;


                        }
                        break;
                    case LevelData.Direction.Left:
                        if (myTarget.GetCellData(x + 1, y).selectable && myTarget.GetCellData(x + 1, y).cellColor == myTarget.defaultGridColor)
                        {
                            car2X = x + 1;
                            car2Y = y;
                            dir = ObjectOnCellData.ObjectDirection.Right;
                            cellAvaliable = true;

                        }
                        break;
                    case LevelData.Direction.Forward:
                        if (myTarget.GetCellData(x, y + 1).selectable && myTarget.GetCellData(x, y + 1).cellColor == myTarget.defaultGridColor)
                        {
                            car2X = x;
                            car2Y = y + 1;
                            dir = ObjectOnCellData.ObjectDirection.Forward;
                            cellAvaliable = true;

                        }
                        break;
                    case LevelData.Direction.Back:
                        if (myTarget.GetCellData(x, y - 1).selectable && myTarget.GetCellData(x, y - 1).cellColor == myTarget.defaultGridColor)
                        {
                            car2X = x;
                            car2Y = y - 1;

                            dir = ObjectOnCellData.ObjectDirection.Back;
                            cellAvaliable = true;

                        }
                        break;
                    default:
                        dir = ObjectOnCellData.ObjectDirection.None;
                        cellAvaliable = false;

                        break;

                }

                if (cellAvaliable)
                {
                    myTarget.carCount++;
                    color = myTarget.selectedColor;
                    value = myTarget.car2xInGridFront;
                    value2 = myTarget.car2xInGridBack;
                    CellData carCell = new CellData(color, value, new Vector2Int(x, y));
                    CellData carCell2 = new CellData(color, value2, new Vector2Int(car2X, car2Y));
                    carCell.cellBasePrefab = myTarget.gridPrefab;
                    carCell2.cellBasePrefab = myTarget.gridPrefab;

                    myTarget.SetCellData(x, y, carCell, true);
                    myTarget.SetCellData(car2X, car2Y, carCell2, true);


                    occCells[0] = new Vector2Int(x, y);
                    occCells[1] = new Vector2Int(car2X, car2Y);
                    newObj = new ObjectOnCellData(myTarget.car2xPrefab, myTarget.selectedColor, dir, occCells); // COLORosajdıfhecz0asfhoınl xnlksjopcjvk ncasjnklcas.l 
                    myTarget.cellObjects.Add(newObj);
                    newObj.objectMaterial = selectedMaterial;
                }


                break;
            case LevelData.ObjectEnum.Car3x:

                for (int i = 0; i < myTarget.colorData.colorOption.Length; i++)
                {
                    if (myTarget.colorEnum.ToString().Equals(myTarget.colorData.colorOption[i].displayName))
                    {
                        selectedMaterial = myTarget.colorData.colorOption[i].carMaterial;
                    }
                }

                occCells = new Vector2Int[3];
                int car3X = x, car3Y = y;
                int car3XX = x, car3YY = y;
                switch (myTarget.direction)
                {
                    case LevelData.Direction.Right:
                        if (myTarget.GetCellData(x - 1, y).selectable && myTarget.GetCellData(x - 1, y).cellColor == myTarget.defaultGridColor)
                        {
                            if (myTarget.cells_1d[y * gridLenghtX + x - 2].selectable && myTarget.cells_1d[y * gridLenghtX + x - 2].cellColor == myTarget.defaultGridColor)
                            {
                                car3X = x - 1;
                                car3XX = x - 2;
                                dir = ObjectOnCellData.ObjectDirection.Left;
                                cellAvaliable = true;
                            }
                        }
                        break;
                    case LevelData.Direction.Left:
                        if (myTarget.GetCellData(x + 1, y).selectable && myTarget.GetCellData(x + 1, y).cellColor == myTarget.defaultGridColor)
                        {
                            if (myTarget.cells_1d[y * gridLenghtX + x + 2].selectable && myTarget.cells_1d[y * gridLenghtX + x + 2].cellColor == myTarget.defaultGridColor)
                            {
                                car3X = x + 1;
                                car3XX = x + 2;
                                dir = ObjectOnCellData.ObjectDirection.Right;
                                cellAvaliable = true;
                            }
                        }
                        break;
                    case LevelData.Direction.Forward:
                        if (myTarget.GetCellData(x, y + 1).selectable && myTarget.GetCellData(x, y + 1).cellColor == myTarget.defaultGridColor)
                        {
                            if (myTarget.GetCellData(x, y + 2).selectable && myTarget.GetCellData(x, y + 2).cellColor == myTarget.defaultGridColor)
                            {
                                car3Y = y + 1;
                                car3YY = y + 2;
                                dir = ObjectOnCellData.ObjectDirection.Forward;
                                cellAvaliable = true;
                            }
                        }
                        break;
                    case LevelData.Direction.Back:
                        if (myTarget.GetCellData(x, y - 1).selectable && myTarget.GetCellData(x, y - 1).cellColor == myTarget.defaultGridColor)
                        {
                            if (myTarget.GetCellData(x, y - 2).selectable && myTarget.GetCellData(x, y - 2).cellColor == myTarget.defaultGridColor)
                            {
                                car3Y = y - 1;
                                car3YY = y - 2;
                                dir = ObjectOnCellData.ObjectDirection.Back;
                                cellAvaliable = true;
                            }
                        }
                        break;
                    default:
                        dir = ObjectOnCellData.ObjectDirection.None;
                        cellAvaliable = false;

                        break;

                }
                if (cellAvaliable)
                {
                    myTarget.carCount++;
                    color = myTarget.selectedColor;
                    value = myTarget.car3xInGridFront;
                    value2 = myTarget.car3xInGridBack;
                    value3 = myTarget.car3xInGridBack;
                    CellData carCell = new CellData(color, value, new Vector2Int(x, y));
                    CellData carCell2 = new CellData(color, value2, new Vector2Int(car3X, car3Y));
                    CellData carCell3 = new CellData(color, value3, new Vector2Int(car3XX, car3YY));
                    carCell.cellBasePrefab = myTarget.gridPrefab;
                    carCell2.cellBasePrefab = myTarget.gridPrefab;

                    myTarget.SetCellData(x, y, carCell, true);
                    myTarget.SetCellData(car3X, car3Y, carCell2, true);
                    myTarget.SetCellData(car3XX, car3YY, carCell3, true);

                    occCells[0] = new Vector2Int(x, y);
                    occCells[1] = new Vector2Int(car3X, car3Y);
                    occCells[2] = new Vector2Int(car3XX, car3YY);
                    newObj = new ObjectOnCellData(myTarget.car3xPrefab, myTarget.selectedColor, dir, occCells); 
                    myTarget.cellObjects.Add(newObj);
                    newObj.objectMaterial = selectedMaterial;
                }

                break;

            default:
                break;
        }

    }
}

