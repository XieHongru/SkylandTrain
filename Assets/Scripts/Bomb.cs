using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int init_timer;
    int timer;

    //��ը��־λ��������ɾ��
    public bool exist = true;
    Vector2Int position;

    Vector2Int[] directions = { new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(1, 1),
                                Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down, Vector2Int.zero };

    public void Initialize(Vector2Int cellPosition)
    {
        timer = init_timer;
        SetPosition(new Vector2Int(cellPosition.x, cellPosition.y));
        Transform textMesh = transform.GetChild(0).GetChild(0);
        textMesh.GetComponent<TextMeshProUGUI>().text = timer.ToString();
    }

    public void MinusTime()
    {
        timer--;
        Transform textMesh = transform.GetChild(0).GetChild(0);
        textMesh.GetComponent<TextMeshProUGUI>().text = timer.ToString();
        if( timer == 0 )
        {
            GameManager.state_bomb = true;
            GameManager.state_objects.Add(gameObject);

            Explosion();
        }
    }

    //���ݻغ������㵱ǰ����ʱ
    public void SetTime(int round)
    {
        timer = init_timer - round;
        Transform textMesh = transform.GetChild(0).GetChild(0);
        textMesh.GetComponent<TextMeshProUGUI>().text = timer.ToString();
    }

    public void Explosion()
    {
        GameManager.audioSource.clip = Resources.Load<AudioClip>("Audios/Explosion");
        GameManager.audioSource.Play();

        GameObject explosion = Instantiate(GameManager.explosion_animation, 
                                            GameManager.groundMap.CellToWorld(new Vector3Int(position.x, position.y, 0))
                                            + new Vector3(0,.5f,0), Quaternion.identity);

        //��¼���Ƿ��ڱ�ը��Χ��
        bool flag = false;

        for(int i = 0; i < directions.Length; i++)
        {
            Vector2Int detectPos = position + directions[i];
            if (GameManager.MapBoundTest(detectPos))
            {
                GameManager.railArray[detectPos.x, detectPos.y] = null;
                if (GameManager.obstacleArray[detectPos.x, detectPos.y] != 3)
                {
                    GameManager.obstacleArray[detectPos.x, detectPos.y] = 0;
                    GameManager.obstacleMap.SetTile(new Vector3Int(detectPos.x, detectPos.y, 0), null);
                }
                GameManager.railMap.SetTile(new Vector3Int(detectPos.x, detectPos.y, 0), null);
                if(GameManager.player.GetPosition() == detectPos)
                {
                    flag = true;
                }
            }
        }
        if( flag )
        {
            //��Ϸ����
            GameManager.player.gameObject.SetActive(false);
            Debug.Log("�𳵱���ը��������Ϸ������");
        }

        GameManager.propArray[position.x, position.y] = null;
        this.exist = false;
        gameObject.SetActive(false);
    }

    public void Undo()
    {

    }

    public void SetPosition(Vector2Int position)
    {
        this.position = position;
    }

    public Vector2Int GetPosition()
    {
        return position;
    }
}
