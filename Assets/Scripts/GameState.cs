using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    Vector2Int setPos;
    Rail[] changeRails;

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

    //������������״̬����ֻ��Ҫ��¼���ú����ڵ�����Ӱ�����
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

    public void Undo()
    {
        if(actionType == ActionType.SetRail)
        {
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
        }
    }
}
