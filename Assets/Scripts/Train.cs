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
        //���ǰ�����������죬���ƶ���
        Rail nextRail = GameManager.railArray[position.x + forwardDirection.x, position.y + forwardDirection.y];
        if ( nextRail != null )
        {
            MoveToCell(nextRail);
        }
        else
        {
            Debug.Log("���������ǰ����");
        }

    }

    void MoveToCell(Rail nextRail)
    {
        //���Ż��ƶ�����
        StartCoroutine(MoveCoroutine(position));

        //���»�λ������
        position += forwardDirection;

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
    private IEnumerator MoveCoroutine(Vector2Int startPos)
    {
        GameManager.state = GameManager.States.���Ŷ���;

        //��ȡ��ֹ��������
        Vector3 startPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(startPos.x, startPos.y, 0));
        startPosition.y += 0.25f;
        Vector3 endPosition = GameManager.railMap.GetCellCenterWorld(new Vector3Int(startPos.x + forwardDirection.x, startPos.y + forwardDirection.y, 0));
        endPosition.y += 0.25f;
        float elapsedTime = 0f;

        //�ƶ�
        while (elapsedTime < .5f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / .5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ȷ���ƶ���Ŀ��λ��
        transform.position = endPosition;
        GameManager.state = GameManager.States.�ȴ�����;
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
    }

    public void SetForwardDirection(Vector2Int direction)
    {
        forwardDirection = direction;

        animator.SetFloat("DirectionX", forwardDirection.x);
        animator.SetFloat("DirectionY", forwardDirection.y);
    }
}
