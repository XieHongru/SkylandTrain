using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int timer;

    //��ը��־λ��������ɾ��
    public bool exist = true;
    Vector2Int position;

    Vector2Int[] directions = { new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(1, 1),
                                Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    public void MinusTime()
    {
        timer--;
        Transform textMesh = transform.GetChild(0).GetChild(0);
        textMesh.GetComponent<TextMeshProUGUI>().text = timer.ToString();
        if( timer == 0 )
        {
            Explosion();
        }
    }

    public void Explosion()
    {
        //��¼���Ƿ��ڱ�ը��Χ��
        bool flag = false;

        for(int i = 0; i < directions.Length; i++)
        {
            Vector2Int detectPos = position + directions[i];
            if (GameManager.MapBoundTest(detectPos))
            {
                GameManager.railArray[detectPos.x, detectPos.y] = null;
                GameManager.obstacleArray[detectPos.x, detectPos.y] = 0;
                GameManager.railMap.SetTile(new Vector3Int(detectPos.x, detectPos.y, 0), null);
                GameManager.obstacleMap.SetTile(new Vector3Int(detectPos.x, detectPos.y, 0), null);
                if(GameManager.player.GetPosition() == detectPos)
                {
                    flag = true;
                }
            }
        }
        if( flag )
        {
            //��Ϸ����
            Debug.Log("�𳵱���ը��������Ϸ������");
        }

        this.exist = false;
        Destroy(gameObject);
    }

    public void SetPosition(Vector2Int position)
    {
        this.position = position;
    }
}
