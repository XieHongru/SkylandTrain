using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

public class Train : MonoBehaviour
{
    Vector2Int position;
    Vector2Int forwardDirection;
    Vector2 worldposition;

    Animator animator;

    public float local_y;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float deltaY = 0f;
        float time = Time.time;
        if(Mod(time, 1f) < 0.5f)
        {
            deltaY = 2 * Mod(time, 1f);
        }
        else
        {
            deltaY = 2 - 2 * Mod(time, 1f);
        }
        gameObject.transform.position = new Vector3(worldposition.x, worldposition.y , 0) + new Vector3(0, deltaY / 10, 0);
        //gameObject.transform.position = GameManager.railMap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0)) + new Vector3(0, currentTime.Second / 20f + .25f, 0);
    }

    float Mod(float x, float y)
    {
        if (y == 0)
        {
            // ����������
            return float.NaN;
        }

        return x - y * Mathf.Floor(x / y);
    }

    public bool Move()
    {
        if(position == GameManager.start)
        {
            Rail start_rail = GameManager.railArray[position.x, position.y];
            if (start_rail.GetLinkDirection1() == Vector2Int.left)
            {
                SetForwardDirection(start_rail.GetLinkDirection2());
            }
            else
            {
                SetForwardDirection(start_rail.GetLinkDirection1());
            }
        }

        if (!GameManager.MapBoundTest(position + forwardDirection))
        {
            if(GameManager.active_slime)
            {
                SetForwardDirection(-forwardDirection);
                GameState gameState = new GameState(GameState.ActionType.Move, GameManager.state_playerPos, GameManager.state_playerForward,
                                                GameManager.state_bomb, GameManager.state_pickaxe, GameManager.state_checkpoint,
                                                GameManager.state_objects, GameManager.state_checkPoints, GameManager.state_round,
                                                GameManager.state_railArray, GameManager.state_obstacleArray);
                GameManager.stateStack.Push(gameState);

                GameManager.continuouslyMove = false;
                GameManager.state = GameManager.States.�ȴ�����;
                GameManager.ActiveButtons();
                return true;
            }
            return false;
        }
        else
        {
            //���ǰ�����������죬���ƶ���
            Rail nextRail = GameManager.railArray[position.x + forwardDirection.x, position.y + forwardDirection.y];
            if (nextRail != null && (nextRail.GetLinkDirection1() == -forwardDirection || nextRail.GetLinkDirection2() == -forwardDirection))
            {
                //�ж�ǰ���Ƿ����յ㣬����ǣ��ж��Ƿ��ܹ�����
                if (position + forwardDirection == GameManager.end)
                {
                    if (GameManager.CheckWin())
                    {
                        Debug.Log("�ؿ�ͨ����");
                        MoveToCell(nextRail);
                        return true;
                    }
                    else
                    {
                        Debug.Log("δ�������м��㣡");
                        return false;
                    }
                }
                else
                {
                    MoveToCell(nextRail);
                    return true;
                }
            }
            else
            {
                //ǰ�������������ǽ�ͺ���
                //Debug.Log("���������ǰ����");
                //if(!GameManager.continuouslyMove)
                //{
                //    MoveToBreak();
                //}
                return false;
            }
        }
    }

    void MoveToBreak()
    {
        //GameManager.round++;

        //�����ڼ���ò�����ť
        GameManager.BanButtons();

        //���Ż𳵱�ը����
        StartCoroutine(MoveExplosion());
    }

    private IEnumerator MoveExplosion()
    {
        GameManager.state = GameManager.States.���Ŷ���;

        Debug.Log("defeat");

        //��ȡ��ֹ��������
        Vector3 startPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        startPosition.y += 0.25f;
        Vector3 endPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(position.x + forwardDirection.x, position.y + forwardDirection.y, 0));
        endPosition.y += 0.25f;
        float elapsedTime = 0f;

        //�ƶ�
        while (elapsedTime < .2f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / .5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //��ը

        //��ʧ
        gameObject.SetActive(false);


        GameState gameState = new GameState(GameState.ActionType.Move, GameManager.state_playerPos, GameManager.state_playerForward,
                                                GameManager.state_bomb, GameManager.state_pickaxe, GameManager.state_checkpoint,
                                                GameManager.state_objects, GameManager.state_checkPoints, GameManager.state_round,
                                                GameManager.state_railArray, GameManager.state_obstacleArray);
        GameManager.stateStack.Push(gameState);

        GameManager.state = GameManager.States.�ȴ�����;
        GameObject.Find("Undo").GetComponent<Button>().interactable = true;

        //ʧ��
        GameManager.GameFail();
    }

    void MoveToCell(Rail nextRail)
    {
        GameManager.round++;

        //�����ڼ���ò�����ť
        GameManager.BanButtons();

        //ת��
        Vector2Int turnDir;
        if(forwardDirection == -nextRail.GetLinkDirection1())
            turnDir = nextRail.GetLinkDirection2();
        else
            turnDir = nextRail.GetLinkDirection1();

        //���Ż��ƶ�����
        StartCoroutine(MoveCoroutine(position, forwardDirection, turnDir));

        //���»�λ������
        position += forwardDirection;

        GameObject detectGO = GameManager.propArray[position.x, position.y];
        //�ж��Ƿ��õ���
        if (detectGO)
        {
            //ʮ�ָ�
            if(detectGO.CompareTag("Pickaxe"))
            {
                GameManager.state_pickaxe = true;
                GameObject pickaxe_clone = Instantiate(detectGO, detectGO.transform.position, Quaternion.identity);
                pickaxe_clone.SetActive(false);
                GameManager.state_objects.Add(pickaxe_clone);

                GameManager.pickaxe++;
                GameManager.UpdateUI();
                GameManager.propArray[position.x, position.y] = null;
                Destroy(detectGO);
            }
        }
        //�ж��Ƿ񾭹�����
        if (GameManager.checkPointArray[position.x, position.y])
        {
            GameManager.state_checkpoint = true;
            GameManager.state_checkPoints.Add(position);

            GameManager.checkPointArray[position.x, position.y] = false;
            GameManager.checkPoints--;
            GameManager.groundMap.SetTile(new Vector3Int(position.x, position.y, 0), GameManager.checkPoint_false);
            Debug.Log("�������㣡");
        }

        //���»�ǰ���������ͼ
        if (nextRail.GetLinkDirection1() == -1 * forwardDirection)
        {
            SetForwardDirection(nextRail.GetLinkDirection2());
        }
        else
        {
            SetForwardDirection(nextRail.GetLinkDirection1());
        }
    }

    //���ƶ�Э��
    private IEnumerator MoveCoroutine(Vector2Int startPos, Vector2Int forwardDir, Vector2Int turnDir)
    {
        GameManager.state = GameManager.States.���Ŷ���;

        //��Ҫת��
        if(turnDir != forwardDir)
        {
            if(forwardDir == Vector2Int.right && turnDir == Vector2Int.down)
            {
                animator.Play("1");
            }
            else if (forwardDir == Vector2Int.down && turnDir == Vector2Int.left)
            {
                animator.Play("2");
            }
            else if (forwardDir == Vector2Int.left && turnDir == Vector2Int.up)
            {
                animator.Play("3");
            }
            else if (forwardDir == Vector2Int.up && turnDir == Vector2Int.right)
            {
                animator.Play("4");
            }
            else if (forwardDir == Vector2Int.up && turnDir == Vector2Int.left)
            {
                animator.Play("5");
            }
            else if (forwardDir == Vector2Int.left && turnDir == Vector2Int.down)
            {
                animator.Play("6");
            }
            else if (forwardDir == Vector2Int.down && turnDir == Vector2Int.right)
            {
                animator.Play("7");
            }
            else if (forwardDir == Vector2Int.right && turnDir == Vector2Int.up)
            {
                animator.Play("8");
            }
        }

        //��ȡ��ֹ��������
        Vector3 startPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(startPos.x, startPos.y, 0));
        startPosition.y += 0.25f;
        Vector3 endPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(startPos.x + forwardDirection.x, startPos.y + forwardDirection.y, 0));
        endPosition.y += 0.25f;
        float elapsedTime = 0f;

        //�ƶ�
        while (elapsedTime < .8f)
        {
            worldposition = Vector3.Lerp(startPosition, endPosition, elapsedTime / .8f);
            //transform.position = startPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ȷ���ƶ���Ŀ��λ��
        worldposition = endPosition;
        
        // �ƶ����ִ��ը���ж�
        foreach(Bomb b in GameManager.bombList)
        {
            if(b.exist)
                b.MinusTime();
        }

        if(GameManager.continuouslyMove)
        {
            if(!Move())
            {
                GameState gameState = new GameState(GameState.ActionType.Move, GameManager.state_playerPos, GameManager.state_playerForward,
                                                    GameManager.state_bomb, GameManager.state_pickaxe, GameManager.state_checkpoint,
                                                    GameManager.state_objects, GameManager.state_checkPoints, GameManager.state_round,
                                                    GameManager.state_railArray, GameManager.state_obstacleArray);
                GameManager.stateStack.Push(gameState);

                GameManager.continuouslyMove = false;
                GameManager.state = GameManager.States.�ȴ�����;
                GameManager.ActiveButtons();
            }
        }
        else
        {
            GameState gameState = new GameState(GameState.ActionType.Move, GameManager.state_playerPos, GameManager.state_playerForward,
                                                GameManager.state_bomb, GameManager.state_pickaxe, GameManager.state_checkpoint,
                                                GameManager.state_objects, GameManager.state_checkPoints, GameManager.state_round,
                                                GameManager.state_railArray, GameManager.state_obstacleArray);
            GameManager.stateStack.Push(gameState);

            GameManager.state = GameManager.States.�ȴ�����;
            GameManager.ActiveButtons();
        }
    }

    GameObject GetItem()
    {
        //�жϳԵ��ĵ������Ͳ�����
        return null;
    }

    void PositionJudge()
    {
        //λ���¼��ж�
    }

    public Vector2Int GetPosition()
    {
        return position;
    }

    public void SetPosition(Vector2Int position)
    {
        this.position = position;
        this.worldposition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0)) + new Vector3(0, .25f, 0);
    }

    public Vector2Int GetForwardPosition()
    {
        return forwardDirection;
    }

    public void SetForwardDirection(Vector2Int direction)
    {
        forwardDirection = direction;

        animator.SetFloat("DirectionX", forwardDirection.x);
        animator.SetFloat("DirectionY", forwardDirection.y);
    }
}
