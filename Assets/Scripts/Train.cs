using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Train : MonoBehaviour
{
    Vector2Int position;
    Vector2Int forwardDirection;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public void Move()
    {
        //如果前进方向有铁轨，则移动火车
        Rail nextRail = GameManager.railArray[position.x + forwardDirection.x, position.y + forwardDirection.y];
        if ( nextRail != null )
        {
            MoveToCell(nextRail);
        }
        else
        {
            Debug.Log("不允许继续前进！");
        }

    }

    void MoveToCell(Rail nextRail)
    {
        //播放火车移动动画
        StartCoroutine(MoveCoroutine(position));

        //更新火车位置坐标
        position += forwardDirection;

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
        GameManager.state = GameManager.States.等待操作;
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
