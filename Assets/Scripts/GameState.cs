using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class GameState
{
    public enum ActionType
    {
        Move,
        SetRail,
        Break
    }

    Vector2Int[] directions = { new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(1, 1),
                                Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down, Vector2Int.zero };

    ActionType actionType;

    int round;

    //��¼���ƶ�ǰ����
    Vector2Int playerPos;
    Vector2Int playerForward;
    Rail[,] railArray;
    int[,] obstacleArray;
    GameObject[,] propArray;
    List<Vector2Int> checkPoints = new List<Vector2Int>();
    List<Vector2Int> propPos = new List<Vector2Int>();

    //��¼�����������������ӵ���������
    Vector2Int setPos;
    Rail[] changeRails;

    //��¼�ݻٵ����꣬���������ϰ���ͼ����
    Vector2Int breakPos;
    Rail breakRail = null;
    Tile breakObstacle = null;

    //�����ƶ���״̬����Ҫ��objects�������ݼ�Ϊ����
    public GameState(ActionType actionType, Vector2Int playerPos, Vector2Int playerForward,
                        bool bomb, bool pickaxe, bool checkpoint, List<GameObject> objects, List<Vector2Int> checkPoints, int round,
                        Rail[,] railArray, int[,] obstacleArray)
    {
        this.actionType = actionType;
        if(actionType == ActionType.Move)
        {
            //���û�д����κ��¼���ֻ��¼���ƶ�ǰ��λ�úͳ���
            this.playerPos = playerPos;
            this.playerForward = playerForward;
            this.round = round;

            this.railArray = railArray;
            this.obstacleArray = obstacleArray;
            propArray = new GameObject[8, 8];
            //����ᴥ����ը
            if (bomb)
            {
                foreach (GameObject obj in objects)
                {
                    if (obj.CompareTag("Bomb"))
                    {
                        Vector2Int tilePos = obj.GetComponent<Bomb>().GetPosition();
                        propArray[tilePos.x, tilePos.y] = obj;
                        propPos.Add(tilePos);

                        //foreach (Vector2Int v in directions)
                        //{

                        //    Vector2Int visitPos = tilePos + v;

                        //    if (GameManager.MapBoundTest(visitPos))
                        //    {
                        //        if (GameManager.railArray[visitPos.x, visitPos.y] != null)
                        //            railArray[visitPos.x, visitPos.y] = (Rail)GameManager.railArray[visitPos.x, visitPos.y].Clone();
                        //        obstacleArray[visitPos.x, visitPos.y] = GameManager.obstacleArray[visitPos.x, visitPos.y];
                        //    }
                        //}
                    }
                }
            }
            //������ʮ�ָ�
            if(pickaxe)
            {
                foreach(GameObject obj in objects)
                {
                    if(obj.CompareTag("Pickaxe"))
                    {
                        Vector3Int tilePos = GameManager.groundMap.WorldToCell(obj.transform.position - new Vector3(0, 0.15f, 0));
                        propArray[tilePos.x, tilePos.y] = obj;
                        propPos.Add(new Vector2Int(tilePos.x, tilePos.y));
                    }
                }
            }
            //�����������
            if(checkpoint)
            {
                this.checkPoints = checkPoints;
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
        if(actionType == ActionType.Move)
        {
            GameManager.player.gameObject.SetActive(true);
            //�ָ���λ�úͷ���
            GameManager.player.SetPosition(playerPos);
            GameManager.player.SetForwardDirection(playerForward);
            Animator animator = GameManager.player.GetComponent<Animator>();
            animator.SetFloat("DirectionX", playerForward.x);
            animator.SetFloat("DirectionY", playerForward.y);
            Vector3 worldPos = GameManager.groundMap.GetCellCenterWorld(new Vector3Int(playerPos.x, playerPos.y, 0));
            worldPos.y += 0.25f;
            GameManager.player.transform.position = worldPos;
            GameManager.round = round;

            //�������
            if(propPos.Count > 0)
            {
                foreach(Vector2Int v in propPos)
                {
                    if (propArray[v.x, v.y].CompareTag("Pickaxe"))
                    {
                        GameManager.pickaxe--;
                        GameManager.UpdateUI();
                        GameManager.propArray[v.x, v.y] = propArray[v.x, v.y];
                        propArray[v.x, v.y].SetActive(true);
                    }
                    else if (propArray[v.x, v.y].CompareTag("Bomb"))
                    {
                        Bomb bomb = propArray[v.x, v.y].GetComponent<Bomb>();

                        for (int i = 0; i < directions.Length; i++)
                        {
                            Vector2Int detectPos = v + directions[i];
                            if (GameManager.MapBoundTest(detectPos))
                            {
                                //���ը����������
                                if (obstacleArray[detectPos.x, detectPos.y] == 1)
                                {
                                    GameManager.railArray[detectPos.x, detectPos.y] = railArray[detectPos.x, detectPos.y];
                                    GameManager.obstacleArray[detectPos.x, detectPos.y] = 1;
                                    GameManager.railArray[detectPos.x, detectPos.y].SetTile();
                                }
                                //���ը�������ϰ���
                                else if (obstacleArray[detectPos.x, detectPos.y] == 2)
                                {
                                    GameManager.obstacleArray[detectPos.x, detectPos.y] = 2;
                                    GameManager.obstacleMap.SetTile(new Vector3Int(detectPos.x, detectPos.y, 0), GameManager.rock);
                                }
                                //else if (obstacleArray[detectPos.x, detectPos.y] == 4)
                                //{
                                //    GameManager.obstacleArray[detectPos.x, detectPos.y] = 4;
                                //    GameManager.obstacleMap.SetTile(new Vector3Int(detectPos.x, detectPos.y, 0), GameManager.slime);
                                //}
                            }
                        }

                        bomb.exist = true;

                        GameManager.propArray[v.x, v.y] = propArray[v.x, v.y];
                        propArray[v.x, v.y].SetActive(true);
                    }
                }
            }

            //�������
            if(checkPoints.Count > 0)
            {
                foreach(Vector2Int v in checkPoints)
                {
                    GameManager.checkPointArray[v.x, v.y] = true;
                    GameManager.checkPoints++;
                    GameManager.groundMap.SetTile(new Vector3Int(v.x, v.y, 0), GameManager.checkPoint_true);
                }
            }

            foreach(Bomb o in GameManager.bombList)
            {
                if(o.exist)
                {
                    o.SetTime(round);
                }
            }
        }
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
            if(GameManager.groundMap.HasTile(new Vector3Int(setPos.x, setPos.y, 0)) && GameManager.groundMap.GetTile(new Vector3Int(setPos.x, setPos.y, 0)).name == "ground")
            {
                GameManager.groundMap.SetTile(new Vector3Int(setPos.x, setPos.y, 0), null);
            }
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
                if(breakObstacle.name == "rock")
                    GameManager.obstacleArray[breakPos.x, breakPos.y] = 2;
                else if(breakObstacle.name == "slime")
                    GameManager.obstacleArray[breakPos.x, breakPos.y] = 4;

                GameManager.obstacleMap.SetTile(new Vector3Int(breakPos.x, breakPos.y, 0), breakObstacle);
            }
            GameManager.pickaxe++;
            GameManager.UpdateUI();
        }
    }
}
