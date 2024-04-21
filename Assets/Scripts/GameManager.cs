using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public GameObject train;

    //����������յ����꣬�Ƿ����ô�����TileMap����
    public Vector2Int startStation;
    public Vector2Int endStation;

    public static Train player;
    public static Rail[,] railArray = new Rail[8,8];

    public enum States
    { 
        �ȴ�����,
        ���Ŷ���
    };
    public static States state;
    // Start is called before the first frame update
    void Start()
    {
        Tilemap railMap = GameObject.Find("Rail").GetComponent<Tilemap>();

        //��TileMap��Ϣת��Ϊ״̬����
        TilemapToArray(railMap);

        //��ʼ����㡢�յ�ͻ�λ��

        //���TileMap����ת��������
        Vector3 startStationWorld = railMap.GetCellCenterWorld(new Vector3Int(startStation.x, startStation.y, 0));
        startStationWorld.y += 0.25f;

        //�����𳵶��󸱱�����ʼ����Ҷ���
        player = Instantiate(train, startStationWorld, Quaternion.identity).GetComponent<Train>();

        //���û𳵳�ʼ��������ͳ���
        player.SetPosition(startStation);

        Rail startRail = railArray[startStation.x, startStation.y];
        Vector2 dir1 = startStation + startRail.GetLinkDirection1();
        if(dir1.x < 0 || dir1.y < 0)
        {
            player.SetForwardDirection(startRail.GetLinkDirection2());
        }
        else
        {
            player.SetForwardDirection(startRail.GetLinkDirection1());
        }

        //��ʼ��������Ϣ
    }

    // Update is called once per frame
    void Update()
    {
        //�����ü����¼�����
        if(state==States.�ȴ�����)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                player.Move();
            }
        }
        //�������λ��

        //�����ͣ����Ƭ�ı䣬���½���Ԥ����
    }

    void TilemapToArray(Tilemap railMap)
    {
        //��TileMap��Ϣת��Ϊ״̬����
        //Tilemap gameMap = GameObject.Find("Ground").GetComponent<Tilemap>();

        //��鲢��ʼ�������������
        if (railMap != null)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Vector3Int cellPosition = new Vector3Int(i, j, 0);
                    if(railMap.HasTile(cellPosition))
                    {
                        railArray[i,j] = RailInitialize(i,j,railMap.GetTile(cellPosition).name);

                        //Debug.Log(railArray[i, j] + ",position:" + railArray[i, j].tilePosition + ",direction:" + railArray[i, j].linkDirection1 + "," + railArray[i, j].linkDirection2);
                    }
                }
            }
        }
        else
        {
            Debug.Log("û���ҵ�Tilemap�е�����ͼ��");
        }
    }

    Rail RailInitialize(int x, int y,string tileName)
    {
        return new Rail(new Vector2Int(x, y), tileName);
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
