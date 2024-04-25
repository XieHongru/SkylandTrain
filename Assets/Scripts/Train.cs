using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class Train : MonoBehaviour
{
    Vector2Int position;
    Vector2Int forwardDirection;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public bool Move()
    {
        if (!GameManager.MapBoundTest(position + forwardDirection))
        {
            return false;
        }
        else
        {
            //如果前进方向有铁轨，则移动火车
            Rail nextRail = GameManager.railArray[position.x + forwardDirection.x, position.y + forwardDirection.y];
            if (nextRail != null)
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
                Debug.Log("不允许继续前进！");
                return false;
            }
        }
    }

    void MoveToCell(Rail nextRail)
    {
        //播放期间禁用操作按钮
        GameManager.BanButtons();

        //播放火车移动动画
        StartCoroutine(MoveCoroutine(position));

        //更新火车位置坐标
        position += forwardDirection;

        GameObject detectGO = GameManager.propArray[position.x, position.y];
        //判断是否获得道具
        if (detectGO)
        {
            //十字镐
            if(detectGO.CompareTag("Pickaxe"))
            {
                GameManager.pickaxe++;
                GameManager.UpdateUI();
                Destroy(detectGO);
            }
        }
        //判断是否经过检查点
        if (GameManager.checkPointArray[position.x, position.y])
        {
            GameManager.checkPointArray[position.x, position.y] = false;
            GameManager.checkPoints--;
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
    private IEnumerator MoveCoroutine(Vector2Int startPos)
    {
        GameManager.state = GameManager.States.播放动画;

        //获取起止世界坐标
        Vector3 startPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(startPos.x, startPos.y, 0));
        startPosition.y += 0.25f;
        Vector3 endPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(startPos.x + forwardDirection.x, startPos.y + forwardDirection.y, 0));
        endPosition.y += 0.25f;
        float elapsedTime = 0f;

        //移动
        while (elapsedTime < .5f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / .5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保移动到目标位置
        transform.position = endPosition;
        
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
                GameManager.continuouslyMove = false;
                GameManager.state = GameManager.States.等待操作;
                GameManager.ActiveButtons();
            }
        }
        else
        {
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
    }

    public void SetForwardDirection(Vector2Int direction)
    {
        forwardDirection = direction;

        animator.SetFloat("DirectionX", forwardDirection.x);
        animator.SetFloat("DirectionY", forwardDirection.y);
    }
}
