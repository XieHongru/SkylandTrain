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
            // 避免除零错误
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
                GameManager.state = GameManager.States.等待操作;
                GameManager.ActiveButtons();
                return true;
            }
            return false;
        }
        else
        {
            //如果前进方向有铁轨，则移动火车
            Rail nextRail = GameManager.railArray[position.x + forwardDirection.x, position.y + forwardDirection.y];
            if (nextRail != null && (nextRail.GetLinkDirection1() == -forwardDirection || nextRail.GetLinkDirection2() == -forwardDirection))
            {
                //判断前方是否是终点，如果是，判断是否能够进入
                if (position + forwardDirection == GameManager.end)
                {
                    if (GameManager.CheckWin())
                    {
                        Debug.Log("关卡通过！");
                        MoveToCell(nextRail);
                        return true;
                    }
                    else
                    {
                        Debug.Log("未经过所有检查点！");
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
                //前方无铁轨或者是墙和湖泊
                //Debug.Log("不允许继续前进！");
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

        //播放期间禁用操作按钮
        GameManager.BanButtons();

        //播放火车爆炸动画
        StartCoroutine(MoveExplosion());
    }

    private IEnumerator MoveExplosion()
    {
        GameManager.state = GameManager.States.播放动画;

        Debug.Log("defeat");

        //获取起止世界坐标
        Vector3 startPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        startPosition.y += 0.25f;
        Vector3 endPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(position.x + forwardDirection.x, position.y + forwardDirection.y, 0));
        endPosition.y += 0.25f;
        float elapsedTime = 0f;

        //移动
        while (elapsedTime < .2f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / .5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //爆炸

        //消失
        gameObject.SetActive(false);


        GameState gameState = new GameState(GameState.ActionType.Move, GameManager.state_playerPos, GameManager.state_playerForward,
                                                GameManager.state_bomb, GameManager.state_pickaxe, GameManager.state_checkpoint,
                                                GameManager.state_objects, GameManager.state_checkPoints, GameManager.state_round,
                                                GameManager.state_railArray, GameManager.state_obstacleArray);
        GameManager.stateStack.Push(gameState);

        GameManager.state = GameManager.States.等待操作;
        GameObject.Find("Undo").GetComponent<Button>().interactable = true;

        //失败
        GameManager.GameFail();
    }

    void MoveToCell(Rail nextRail)
    {
        GameManager.round++;

        //播放期间禁用操作按钮
        GameManager.BanButtons();

        //转弯
        Vector2Int turnDir;
        if(forwardDirection == -nextRail.GetLinkDirection1())
            turnDir = nextRail.GetLinkDirection2();
        else
            turnDir = nextRail.GetLinkDirection1();

        //播放火车移动动画
        StartCoroutine(MoveCoroutine(position, forwardDirection, turnDir));

        //更新火车位置坐标
        position += forwardDirection;

        GameObject detectGO = GameManager.propArray[position.x, position.y];
        //判断是否获得道具
        if (detectGO)
        {
            //十字镐
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
        //判断是否经过检查点
        if (GameManager.checkPointArray[position.x, position.y])
        {
            GameManager.state_checkpoint = true;
            GameManager.state_checkPoints.Add(position);

            GameManager.checkPointArray[position.x, position.y] = false;
            GameManager.checkPoints--;
            GameManager.groundMap.SetTile(new Vector3Int(position.x, position.y, 0), GameManager.checkPoint_false);
            Debug.Log("经过检查点！");
        }

        //更新火车前进方向和贴图
        if (nextRail.GetLinkDirection1() == -1 * forwardDirection)
        {
            SetForwardDirection(nextRail.GetLinkDirection2());
        }
        else
        {
            SetForwardDirection(nextRail.GetLinkDirection1());
        }
    }

    //火车移动协程
    private IEnumerator MoveCoroutine(Vector2Int startPos, Vector2Int forwardDir, Vector2Int turnDir)
    {
        GameManager.state = GameManager.States.播放动画;

        //需要转向
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

        //获取起止世界坐标
        Vector3 startPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(startPos.x, startPos.y, 0));
        startPosition.y += 0.25f;
        Vector3 endPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(startPos.x + forwardDirection.x, startPos.y + forwardDirection.y, 0));
        endPosition.y += 0.25f;
        float elapsedTime = 0f;

        //移动
        while (elapsedTime < .8f)
        {
            worldposition = Vector3.Lerp(startPosition, endPosition, elapsedTime / .8f);
            //transform.position = startPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保移动到目标位置
        worldposition = endPosition;
        
        // 移动完后执行炸弹判定
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
                GameManager.state = GameManager.States.等待操作;
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

            GameManager.state = GameManager.States.等待操作;
            GameManager.ActiveButtons();
        }
    }

    GameObject GetItem()
    {
        //判断吃到的道具类型并返回
        return null;
    }

    void PositionJudge()
    {
        //位置事件判定
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
