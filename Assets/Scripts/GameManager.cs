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
    public static Camera mainCamera;

    //��¼�������
    Vector2 mousePos;
    Vector3Int mouseCellPos;

    public enum States
    { 
        �ȴ�����,
        ���Ŷ���,
        ��������
    };
    //��Ϸ�ĵ�ǰ����״̬
    public static States state;

    //�������Ԥ�������
    Rail previewRail;

    //Tilemap
    public static Tilemap railMap;
    public static Tilemap groundMap;
    public static Tilemap previewMap;

    //������ͼ��������ͼ���������ӷ������������������о����˷��ڴ棬�ŵ�ʲô�ط����һ�㣩
    public Tile horizontal;
    public Tile vertical;
    public Tile leftDown;
    public Tile rightDown;
    public Tile leftUp;
    public Tile rightUp;
    public static Tile rail_horizontal;
    public static Tile rail_vertical;
    public static Tile rail_leftDown;
    public static Tile rail_rightDown;
    public static Tile rail_leftUp;
    public static Tile rail_rightUp;

    // Start is called before the first frame update
    void Start()
    {
        //��ʼ�����
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        //��ʼ����ͼ
        rail_horizontal = horizontal;
        rail_vertical = vertical; 
        rail_leftDown = leftDown;
        rail_rightDown = rightDown;
        rail_leftUp = leftUp;
        rail_rightUp = rightUp;

        //��ʼ��Tilemap
        railMap = GameObject.Find("Rail").GetComponent<Tilemap>();
        groundMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        previewMap = GameObject.Find("Preview").GetComponent<Tilemap>();

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
        //�������λ��
        UpdateMousePos();

        switch (state)
        {
            case States.�ȴ�����:
                //�����ü����¼�����
                if (Input.GetKeyDown(KeyCode.R))
                {
                    player.Move();
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    state = States.��������;
                }
                break;
            case States.��������:
                CheckRail();
                if (Input.GetMouseButtonDown(0))
                {
                    SetRail();
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    state = States.�ȴ�����;
                }
                break;
        }

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

    void CheckRail()
    {
        //��������λ��
        Vector3Int newMouseCellPos = railMap.WorldToCell(mousePos);
        bool cellChange = false;
        if(newMouseCellPos!= mouseCellPos)
        {
            //���ָ������ı䣬�޸�״̬λ��ɾ����һ���Ԥ����Ϣ
            cellChange = true;
            previewMap.SetTile(mouseCellPos, null);
            mouseCellPos = newMouseCellPos;
        }
        //Debug.Log(mousePos+","+mouseCellPos);

        //����ָ����������ı�ʱԤ����������Ϣ
        if (!cellChange)
            return;
        if(MapBoundTest(new Vector2Int(mouseCellPos.x, mouseCellPos.y)))
        {
            previewRail = new Rail(new Vector2Int(mouseCellPos.x, mouseCellPos.y));
            previewRail.PreCalculate();
        }
    }

    void SetRail()
    {
        if (MapBoundTest(new Vector2Int(mouseCellPos.x, mouseCellPos.y)))
        {
            //��������
            previewRail.SetTile();
            //����railArray
            railArray[mouseCellPos.x, mouseCellPos.y] = previewRail;
            //��������������ܵ�ת������
            previewRail.LinkNeighbour();
        }
    }

    void BreakRail()
    {
        //�ƻ�����

        //����railArray
    }

    void BreakBarrier()
    {
        //�ƻ��ϰ���

        //����objectArray?
    }

    void UpdateMousePos()
    {
        //�������λ��
        Vector3 mouseScreenPos = Input.mousePosition;
        mousePos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));

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

    public static bool MapBoundTest(Vector2Int cellPos)
    {
        //��ͼ����Խ���жϣ����cellPosԽ�磬����false�����򷵻�true
        if(cellPos.x >= 0 && cellPos.x < 8 && cellPos.y >= 0 && cellPos.y < 8)
        {
            return true;
        }
        return false;
    }
}
