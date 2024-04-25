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
            //���ǰ�����������죬���ƶ���
            Rail nextRail = GameManager.railArray[position.x + forwardDirection.x, position.y + forwardDirection.y];
            if (nextRail != null)
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
                Debug.Log("���������ǰ����");
                return false;
            }
        }
    }

    void MoveToCell(Rail nextRail)
    {
        //�����ڼ���ò�����ť
        GameManager.BanButtons();

        //���Ż��ƶ�����
        StartCoroutine(MoveCoroutine(position));

        //���»�λ������
        position += forwardDirection;

        GameObject detectGO = GameManager.propArray[position.x, position.y];
        //�ж��Ƿ��õ���
        if (detectGO)
        {
            //ʮ�ָ�
            if(detectGO.CompareTag("Pickaxe"))
            {
                GameManager.pickaxe++;
                GameManager.UpdateUI();
                Destroy(detectGO);
            }
        }
        //�ж��Ƿ񾭹�����
        if (GameManager.checkPointArray[position.x, position.y])
        {
            GameManager.checkPointArray[position.x, position.y] = false;
            GameManager.checkPoints--;
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
                GameManager.continuouslyMove = false;
                GameManager.state = GameManager.States.�ȴ�����;
                GameManager.ActiveButtons();
            }
        }
        else
        {
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
    }

    public void SetForwardDirection(Vector2Int direction)
    {
        forwardDirection = direction;

        animator.SetFloat("DirectionX", forwardDirection.x);
        animator.SetFloat("DirectionY", forwardDirection.y);
    }
}
