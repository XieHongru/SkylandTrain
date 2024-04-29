using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameState
{
    public enum ActionType
    {
        Move,
        SetRail,
        Break
    }

    ActionType actionType;

    Vector2Int playerPos;
    Rail[,] railArray;
    int[,] obstacleArray;
    GameObject[,] propArray;
    ArrayList bombList;
    bool[,] checkPointArray;

    //记录放置铁轨的坐标和连接的两节铁轨
    Vector2Int setPos;
    Rail[] changeRails;

    //记录摧毁的坐标，铁轨对象或障碍贴图类型
    Vector2Int breakPos;
    Rail breakRail = null;
    Tile breakObstacle = null;

    //构造移动的状态对象
    public GameState(ActionType actionType, bool exploded, ArrayList bombs)
    {
        this.actionType = actionType;
        if(actionType == ActionType.Move)
        {
            //如果没有触发任何事件，只记录火车移动前的位置
            playerPos = GameManager.player.GetPosition();
            //如果会触发爆炸，存储地图信息和
            if(exploded)
            {
                railArray = new Rail[8,8];
                obstacleArray = new int[8,8];
                bombList = bombs;
                GameManager.railArray.CopyTo(railArray, 0);
                GameManager.obstacleArray.CopyTo(obstacleArray, 0);
                
            }
        }
    }

    //构造放置铁轨的状态对象，只需要记录放置位置和相邻的两个影响对象
    public GameState(ActionType actionType, Vector2Int setPos, Rail link1, Rail link2)
    {
        this.actionType = actionType;
        if (actionType == ActionType.SetRail)
        {
            this.setPos = setPos;
            changeRails = new Rail[2];
            if(link1 != null)
                changeRails[0] = (Rail)link1.Clone();
            if(link2 != null)
                changeRails[1] = (Rail)link2.Clone();
        }
    }

    //构造摧毁方块的状态对象，只需要记录摧毁位置和类型
    public GameState(ActionType actionType, Vector2Int breakPos, Rail rail, string s)
    {
        this.actionType = actionType;
        if (actionType == ActionType.Break)
        {
            this.breakPos = breakPos;
            if(rail != null)
                breakRail = (Rail)rail.Clone();
            else
            {
                string path = "Palettes/" + s;
                breakObstacle = Resources.Load<Tile>(path);
            }
        }
    }

    public void Undo()
    {
        if(actionType == ActionType.SetRail)
        {
            //恢复连接铁轨的状态，删除放置的铁轨
            Vector2Int pos;
            if (changeRails[0] != null)
            {
                pos = changeRails[0].GetPosition();
                GameManager.railArray[pos.x, pos.y] = changeRails[0];
                changeRails[0].SetTile();
            }
            if (changeRails[1] != null)
            {
                pos = changeRails[1].GetPosition();
                GameManager.railArray[pos.x, pos.y] = changeRails[1];
                changeRails[1].SetTile();
            }
            GameManager.railArray[setPos.x, setPos.y] = null;
            GameManager.railMap.SetTile(new Vector3Int(setPos.x, setPos.y, 0), null);
            GameManager.obstacleArray[setPos.x, setPos.y] = 0;
            GameManager.rails++;
            GameManager.UpdateUI();
        }
        if(actionType == ActionType.Break)
        {
            if(breakRail != null)
            {
                GameManager.railArray[breakPos.x, breakPos.y] = breakRail;
                breakRail.SetTile();
                GameManager.obstacleArray[breakPos.x, breakPos.y] = 1;
            }
            else if(breakObstacle != null)
            {
                GameManager.obstacleArray[breakPos.x, breakPos.y] = 2;
                GameManager.obstacleMap.SetTile(new Vector3Int(breakPos.x, breakPos.y, 0), breakObstacle);
            }
            GameManager.pickaxe++;
            GameManager.UpdateUI();
        }
    }
}
