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
        //���ǰ�����������죬���ƶ���

        //���»�λ������

        //���»�ǰ���������ͼ

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

    public void SetForwardDirection(Vector2 direction)
    {
        forwardDirection = direction;

        animator = GetComponent<Animator>();
        animator.SetFloat("DirectionX", forwardDirection.x);
        animator.SetFloat("DirectionY", forwardDirection.y);
    }
}
