using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Grids
{
    public SpawnManager.GridState[] grid;
}
public class SpawnManager : MonoBehaviour
{
    public enum GridState
    {
        none,
        white,
        green,
        blue,
        yellow,
        purple
    }
    public enum RotState
    {
        up,
        left,
        down,
        right
    }
    public int column, row;
    public Grids[] grid;
    public GameObject[] blockPrefabs;
    public Image[] tile;
    [Space(10)]
    public float downTime = 0.5f;
    public int curPosX;
    public int curPosY;
    public RotState curRot;
    public GridState[] curBlocks;
    public float curTime;
    void Awake()
    {
        GenerateGrid();
    }
    public void GenerateGrid()
    {
        grid = new Grids[column];
        for (int i = 0; i < column; i++)
        {
            grid[i] = new Grids
            {
                grid = new GridState[row]
            };
        }
    }
    private void Update()
    {
        KeyInput();
        SpawnMove();
        UpdateGUI();
    }
    void KeyInput()
    {

        downTime = Input.GetKey(KeyCode.DownArrow) ? 0.1f : 0.75f;

        if (Input.GetKeyDown(KeyCode.LeftControl)) Rotate();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            var leftBlock = curRot == RotState.left ? 1 : 0;
            if(curPosX <= leftBlock) return;
            curPosX--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            var rightBlock = curRot == RotState.right ? 6 : 7;
            if(curPosX >= rightBlock) return;
            curPosX++;
        }
    }
    void Rotate()
    {
        var (nextRotX, nextRotY) = GetSecondBlockPos(new Vector2Int(curPosX, curPosY), (RotState)((int)curRot + 1));
        var nextPos = grid[nextRotY].grid[nextRotX];
        if (nextPos != GridState.none) return;
        if (nextRotX < 0)
        {
            if (grid[curPosY].grid[curPosX + 1] != GridState.none || curPosX + 1 > 7) return;
            curPosX++;
        }
        else if (nextRotX > 7)
        {
            if (grid[curPosY].grid[curPosX - 1] != GridState.none || curPosX - 1 < 0) return;
            curPosX--;
        }
        curRot = (RotState)((int)(curRot + 1) % 4);
    }
    void SpawnMove()
    {
        if (curTime <= 0)
        {
            if (curBlocks[0] == GridState.none)
            {
                for (int i = 0; i < curBlocks.Length; i++) curBlocks[i] = (GridState)Random.Range(2, 6);
                curPosX = 2;
                curPosY = 0;
                curRot = RotState.up;
            }
            var (secY, secX) = GetSecondBlockPos(new Vector2Int(curPosX, curPosY), curRot);
            var firstDownPivot = curRot == RotState.down ? curPosY + 2 : curPosY + 1;
            if (firstDownPivot >= grid.Length || grid[firstDownPivot].grid[curPosX] != GridState.none)
            {
                grid[curPosY].grid[curPosX] = curBlocks[0];
                grid[secY].grid[secX] = curBlocks[1];

                for (int i = 0; i < curBlocks.Length; i++) curBlocks[i] = GridState.none;
            }
            else
            {
                curPosY++;
            }

            curTime += downTime;
        }
        curTime -= Time.deltaTime;
        curTime = Mathf.Clamp(curTime, 0, downTime);
    }
    (int, int) GetSecondBlockPos(Vector2Int curPivot, RotState rotState)
    {
        Vector2Int returnValue = Vector2Int.zero;
        switch (rotState)
        {
            case RotState.up:
                returnValue = Vector2Int.down;
                break;
            case RotState.left:
                returnValue = Vector2Int.left;
                break;
            case RotState.down:
                returnValue = Vector2Int.up;
                break;
            case RotState.right:
                returnValue = Vector2Int.right;
                break;
        }
        return (returnValue.y + curPivot.y, returnValue.x + curPivot.x);
    }
    void UpdateGUI()
    {
        Color[] colors = { Color.gray, Color.white, Color.green, Color.blue, Color.yellow, Color.red };
        for (int i = 0; i < tile.Length; i++)
        {
            Color color = Color.white;
            var y = i / row;
            var x = i % row;
            tile[i].color = colors[(int)grid[y].grid[x]];
        }
        if (curBlocks[0] == GridState.none) return;
        tile[(curPosY * row) + curPosX].color = colors[(int)curBlocks[0]];
        var (secY, secX) = GetSecondBlockPos(new Vector2Int(curPosX, curPosY), curRot);
        tile[(secY * row) + secX].color = colors[(int)curBlocks[1]];

    }
}
