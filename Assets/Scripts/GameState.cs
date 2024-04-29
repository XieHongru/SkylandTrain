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

    //��¼�����������������ӵ���������
    Vector2Int setPos;
    Rail[] changeRails;

    //��¼�ݻٵ����꣬���������ϰ���ͼ����
    Vector2Int breakPos;
    Rail breakRail = null;
    Tile breakObstacle = null;

    //�����ƶ���״̬����
    public GameState(ActionType actionType, bool exploded, ArrayList bombs)
    {
        this.actionType = actionType;
        if(actionType == ActionType.Move)
        {
            //���û�д����κ��¼���ֻ��¼���ƶ�ǰ��λ��
            playerPos = GameManager.player.GetPosition();
            //����ᴥ����ը���洢��ͼ��Ϣ��
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

    //������������״̬����ֻ��Ҫ��¼����λ�ú����ڵ�����Ӱ�����
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

    //����ݻٷ����״̬����ֻ��Ҫ��¼�ݻ�λ�ú�����
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
            //�ָ����������״̬��ɾ�����õ�����
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
