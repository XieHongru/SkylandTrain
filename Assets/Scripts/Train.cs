using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    Vector2 position;
    Vector2 forwardDirection;

    Animator animator;

    void Start()
    {
        
    }
    
    public void Move()
    {
        //如果前进方向有铁轨，则移动火车

        //更新火车位置坐标

        //更新火车前进方向和贴图

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

    public void SetForwardDirection(Vector2 direction)
    {
        forwardDirection = direction;

        animator = GetComponent<Animator>();
        animator.SetFloat("DirectionX", forwardDirection.x);
        animator.SetFloat("DirectionY", forwardDirection.y);
    }
}
