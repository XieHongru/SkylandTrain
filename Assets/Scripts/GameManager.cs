using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public GameObject player;

    public static Rail[,] railArray = new Rail[8,8];
    // Start is called before the first frame update
    void Start()
    {
        //��TileMap��Ϣת��Ϊ״̬����
        TilemapToArray();

        //��ʼ����㡢�յ�ͻ�λ��

        //��ʼ��������Ϣ
    }

    // Update is called once per frame
    void Update()
    {
        //�������λ��

        //�����ͣ����Ƭ�ı䣬���½���Ԥ����
    }

    void TilemapToArray()
    {
        //��TileMap��Ϣת��Ϊ״̬����
        //Tilemap gameMap = GameObject.Find("Ground").GetComponent<Tilemap>();

        //��鲢��ʼ�������������
        Tilemap railMap = GameObject.Find("Rail").GetComponent<Tilemap>();

        if (railMap != null)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Vector3Int cellPosition = new Vector3Int(i, j, 0);
                    //railArray[i, j] = railMap.GetTile(cellPosition);
                }
            }
        }
        else
        {
            Debug.Log("û���ҵ�Tilemap�е�����ͼ��");
        }
    }

    void SetRail()
    {
        //��������

        //����railMap
    }

    void BreakRail()
    {
        //�ƻ�����

        //����railMap
    }

    void BreakBarrier()
    {
        //�ƻ��ϰ���

        //����objectMap?
    }

    void RunStep()
    {
        //����һ��
    }

    void RunFinish()
    {
        //��������
    }

    void StopGame()
    {
        //��ͣ��Ϸ
    }

    void Restart()
    {
        //���¼�����Ϸ
    }
}
