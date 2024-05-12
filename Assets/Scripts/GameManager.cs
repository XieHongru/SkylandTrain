using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject train;

    //�غ������ƶ�һ����һ���غ�
    public static int round;

    //�������ʷ��ķǽ��
    public bool slime;
    public static bool active_slime;

    //����������յ����꣬�Ƿ����ô�����TileMap����
    public Vector2Int startStation;
    public Vector2Int endStation;
    public static Vector2Int start;
    public static Vector2Int end;

    public static Train player;
    public static Rail[,] railArray;
    //�ϰ����0��ʾ�յأ�1��ʾ���죬2��ʾɽ��3��ʾ����4��ʾʷ��ķǽ
    public static int[,] obstacleArray; 
    //���߱�
    public static GameObject[,] propArray;
    public static ArrayList bombList;
    //�����
    public static bool[,] checkPointArray;
    public static Camera mainCamera;

    //��¼�������
    Vector2 mousePos;
    Vector3Int mouseCellPos;

    //״̬ջ
    public static StateStack<GameState> stateStack;

    public enum States
    { 
        �ȴ�����,
        ���Ŷ���,
        ��������,
        �ƻ�����
    };
    //��Ϸ�ĵ�ǰ����״̬
    public static States state;
    public static bool continuouslyMove;

    //����仯�ж�
    bool cellChange = false;

    //�������Ԥ�������
    Rail previewRail;

    //Tilemap
    public static Tilemap railMap;
    public static Tilemap groundMap;
    public static Tilemap previewMap;
    public static Tilemap obstacleMap;

    //������ͼ��������ͼ���������ӷ�������
    public static Tile rail_horizontal;
    public static Tile rail_vertical;
    public static Tile rail_leftDown;
    public static Tile rail_rightDown;
    public static Tile rail_leftUp;
    public static Tile rail_rightUp;

    //��ʮ�ָ���ͼ
    public static Tile highlight;

    //������ͼ
    public static Tile ground;

    //������ͼ
    public static Tile checkPoint_true;
    public static Tile checkPoint_false;

    //�ϰ���ͼ
    public static Tile rock;
    //public static Tile slime;

    //ȫ�ֲ���
    public static int checkPoints;
    public int init_rails;
    public static int rails;
    public int init_pickaxe;
    public static int pickaxe;


    //״̬ѹջʹ�õ���ʱ����
    public static int state_round;
    public static bool state_bomb;
    public static bool state_pickaxe;
    public static bool state_checkpoint;
    public static List<GameObject> state_objects;
    public static List<Vector2Int> state_checkPoints;
    public static Vector2Int state_playerPos;
    public static Vector2Int state_playerForward;
    public static Rail[,] state_railArray;
    public static int[,] state_obstacleArray;

    // Start is called before the first frame update
    void Start()
    {
        //��ʼ�����
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        mainCamera.transparencySortAxis = new Vector3(0.49f, 2f, 0.49f);

        round = 0;
        active_slime = slime;

        //��ʼ��״̬ջ
        stateStack = new StateStack<GameState>(20);

        //��ʼ��Array
        railArray = new Rail[8, 8];
        obstacleArray = new int[8, 8];
        propArray = new GameObject[8, 8];
        bombList = new ArrayList();
        checkPointArray = new bool[8, 8];

        //��ʼ����ͼ
        rail_horizontal = Resources.Load<Tile>("Palettes/rail_horizontal");
        rail_vertical = Resources.Load<Tile>("Palettes/rail_vertical");
        rail_leftDown = Resources.Load<Tile>("Palettes/rail_leftDown");
        rail_rightDown = Resources.Load<Tile>("Palettes/rail_rightDown");
        rail_leftUp = Resources.Load<Tile>("Palettes/rail_leftUp");
        rail_rightUp = Resources.Load<Tile>("Palettes/rail_rightUp");
        highlight = Resources.Load<Tile>("Palettes/highlight");
        ground = Resources.Load<Tile>("Palettes/ground");
        checkPoint_false = Resources.Load<Tile>("Palettes/checkPoint_false");
        checkPoint_true = Resources.Load<Tile>("Palettes/checkPoint");
        rock = Resources.Load<Tile>("Palettes/rock");
        //slime = Resources.Load<Tile>("Palettes/slime");

        //��ʼ��Tilemap
        railMap = GameObject.Find("Rail").GetComponent<Tilemap>();
        groundMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        previewMap = GameObject.Find("Preview").GetComponent<Tilemap>();
        obstacleMap = GameObject.Find("Obstacle").GetComponent<Tilemap>();

        //������ʼ��
        continuouslyMove = false;
        rails = init_rails;
        pickaxe = init_pickaxe;
        checkPoints = 0;

        //��TileMap��Ϣת��Ϊ״̬����
        TilemapToArray();

        //��ʼ����㡢�յ�ͻ�λ��
        start = startStation;
        end = endStation;

        //���TileMap����ת��������
        Vector3 startStationWorld = railMap.GetCellCenterWorld(new Vector3Int(startStation.x, startStation.y, 0));
        startStationWorld.y += 0.15f;

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
        InitializeProps();

        //��ʼ��UI
        InitializeUI();
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
                    RunStep();
                }
                if (Input.GetKeyDown(KeyCode.T))
                {
                    RunFinish();
                }
                if (Input.GetKeyDown(KeyCode.B))
                {
                    if (pickaxe <= 0)
                    {
                        Debug.Log("û�п��õĿ󹤸�");
                    }
                    else
                    {
                        state = States.�ƻ�����;
                    }
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    if(rails <= 0)
                    {
                        Debug.Log("û�пɷ��õ�����");
                    }
                    else
                    {
                        state = States.��������;
                    }
                }
                break;
            case States.��������:
                CheckCellChange();
                CheckRail();
                if (Input.GetMouseButtonDown(0))
                {
                    SetRail();
                    UpdateUI();
                }
                if (Input.GetKeyDown(KeyCode.P) || Input.GetMouseButtonDown(1))
                {
                    previewMap.SetTile(mouseCellPos, null);
                    state = States.�ȴ�����;
                }
                break;
            case States.�ƻ�����:
                CheckCellChange();
                CheckBreak();
                if (Input.GetMouseButtonDown(0))
                {
                    BreakCell();
                    UpdateUI();
                }
                if (Input.GetKeyDown(KeyCode.B) || Input.GetMouseButtonDown(1))
                {
                    previewMap.SetTile(mouseCellPos, null);
                    state = States.�ȴ�����;
                }
                break;
        }

        //�����ͣ����Ƭ�ı䣬���½���Ԥ����
    }

    void TilemapToArray()
    {
        //��TileMap��Ϣת��Ϊ״̬����

        //�ؿ���ͼ�걸�Լ��
        if (groundMap == null || railMap == null || previewMap == null || obstacleMap == null)
        {
            Debug.Log("��ͼͼ������ȱʧ��");
            return;
        }

        //��ʼ�������������
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                //̽��(i,j)�����ϵ�������Ϣ
                Vector3Int cellPosition = new Vector3Int(i, j, 0);
                //����ͼ��̽��
                if(railMap.HasTile(cellPosition))
                {
                    obstacleArray[i,j] = 1;
                    railArray[i,j] = RailInitialize(i, j, railMap.GetTile(cellPosition).name);

                    //Debug.Log(railArray[i, j] + ",position:" + railArray[i, j].tilePosition + ",direction:" + railArray[i, j].linkDirection1 + "," + railArray[i, j].linkDirection2);

                }
                //����ͼ��̽��
                if(obstacleMap.HasTile(cellPosition))
                {
                    string name = obstacleMap.GetTile(cellPosition).name;
                    if ( name == "rock" || name == "rock2" || name == "rock3")
                        obstacleArray[i, j] = 2;
                    //else if (obstacleMap.GetTile(cellPosition).name == "slime")
                    //    obstacleArray[i, j] = 4;
                }
                if(groundMap.HasTile(cellPosition))
                {
                    string name = groundMap.GetTile(cellPosition).name;
                    if (name == "checkPoint")
                    {
                        checkPointArray[i, j] = true;
                        checkPoints++;
                    }
                    else if(name == "pool" || name == "pool_1" || name == "pool_2" || name == "pool_3" || name == "pool_4")
                    {
                        obstacleArray[i, j] = 3;
                    }
                }
            }
        }
    }

    void InitializeProps()
    {
        //��ʼ����ʮ�ָ�
        GameObject[] pickaxeObjects = GameObject.FindGameObjectsWithTag("Pickaxe");
        foreach(GameObject go in pickaxeObjects)
        {
            Vector3Int cellPosition = groundMap.WorldToCell(go.transform.position);
            go.transform.position = groundMap.GetCellCenterWorld(cellPosition) + new Vector3(0, 0.15f, 0);
            if(MapBoundTest(new Vector2Int(cellPosition.x, cellPosition.y)))
            {
                if (propArray[cellPosition.x, cellPosition.y] == null)
                {
                    propArray[cellPosition.x, cellPosition.y] = go;
                }
                else
                {
                    Debug.Log("���߰ڷŴ����ص���");
                }
            }
            else
            {
                Debug.Log("��ͼ���߰ڷŴ���!");
            }
        }

        //��ʼ���߱�ը��(����������)
        GameObject[] bombObjects = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (GameObject go in bombObjects)
        {
            Vector3Int cellPosition = groundMap.WorldToCell(go.transform.position);
            go.transform.position = groundMap.GetCellCenterWorld(cellPosition) + new Vector3(0, 0.15f, 0);
            if (MapBoundTest(new Vector2Int(cellPosition.x, cellPosition.y)))
            {
                if (propArray[cellPosition.x, cellPosition.y] == null)
                {
                    propArray[cellPosition.x, cellPosition.y] = go;
                    Bomb newBomb = go.GetComponent<Bomb>();
                    newBomb.Initialize(new Vector2Int(cellPosition.x, cellPosition.y));
                    bombList.Add(newBomb);
                }
                else
                {
                    Debug.Log("���߰ڷŴ����ص���");
                }
            }
            else
            {
                Debug.Log("��ͼ���߰ڷŴ���!");
            }
        }

    }

    void InitializeUI()
    {
        GameObject.Find("RailCountText").GetComponent<Text>().text = Convert.ToString(rails);
        GameObject.Find("PickaxeCountText").GetComponent<Text>().text = Convert.ToString(pickaxe);
        if (pickaxe == 0)
        {
            GameObject.Find("PickaxeButton").GetComponent<Button>().interactable = false;
        }
    }

    public static void UpdateUI()
    {
        GameObject.Find("RailCountText").GetComponent<Text>().text = Convert.ToString(rails);
        GameObject.Find("PickaxeCountText").GetComponent<Text>().text = Convert.ToString(pickaxe);
        if (rails == 0)
        {
            GameObject.Find("RailButton").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("RailButton").GetComponent<Button>().interactable = true;
        }
        if (pickaxe == 0)
        {
            GameObject.Find("PickaxeButton").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("PickaxeButton").GetComponent<Button>().interactable = true;
        }
    }

    Rail RailInitialize(int x, int y,string tileName)
    {
        return new Rail(new Vector2Int(x, y), tileName);
    }

    void CheckCellChange()
    {
        //��������λ��
        Vector3Int newMouseCellPos = railMap.WorldToCell(mousePos);
        if (newMouseCellPos != mouseCellPos)
        {
            //���ָ������ı䣬�޸�״̬λ��ɾ����һ���Ԥ����Ϣ
            cellChange = true;
            previewMap.SetTile(mouseCellPos, null);
            mouseCellPos = newMouseCellPos;
        }
        //Debug.Log(mousePos+","+mouseCellPos);
    }

    void CheckRail()
    {
        //����ָ����������ı�ʱԤ����������Ϣ
        if (!cellChange)
            return;
        if(MapBoundTest(new Vector2Int(mouseCellPos.x, mouseCellPos.y)))
        {
            if (obstacleArray[mouseCellPos.x,mouseCellPos.y] == 0)
            {
                previewMap.color = new Color(1, 1, 1, 0.5f);
                previewRail = new Rail(new Vector2Int(mouseCellPos.x, mouseCellPos.y));
                previewRail.PreCalculate();
            }
            else
            {
                previewMap.color = new Color(1, 0, 0, 0.5f);
                previewMap.SetTile(mouseCellPos, rail_horizontal);
            }
            cellChange = false;
        }
    }

    void SetRail()
    {
        
        if (MapBoundTest(new Vector2Int(mouseCellPos.x, mouseCellPos.y)))
        {
            if (rails > 0)
            {
                if ( obstacleArray[mouseCellPos.x, mouseCellPos.y ] != 0)
                {
                    Debug.Log("���ܽ�����������ϰ����ϣ�");
                }
                else
                {
                    //�ȶԵ�ǰ״̬ѹջ
                    Rail link1 = null;
                    Rail link2 = null;
                    Vector2Int linkDir1 = new Vector2Int(mouseCellPos.x, mouseCellPos.y) + previewRail.GetLinkDirection1();
                    Vector2Int linkDir2 = new Vector2Int(mouseCellPos.x, mouseCellPos.y) + previewRail.GetLinkDirection2();
                    if(MapBoundTest(linkDir1))
                        link1 = railArray[linkDir1.x, linkDir1.y];
                    if(MapBoundTest(linkDir2))
                        link2 = railArray[linkDir2.x, linkDir2.y];
                    GameState gameState = new GameState(GameState.ActionType.SetRail, new Vector2Int(mouseCellPos.x, mouseCellPos.y), link1, link2);
                    stateStack.Push(gameState);

                    //����
                    rails--;
                    //if( rails==0 )
                    //{
                    //    state = States.�ȴ�����;
                    //}

                    //��������
                    previewRail.SetTile();
                    if(!groundMap.HasTile(mouseCellPos))
                    {
                        groundMap.SetTile(mouseCellPos, ground);
                    }
                    //����railArray��obstacleArray
                    railArray[mouseCellPos.x, mouseCellPos.y] = previewRail;
                    obstacleArray[mouseCellPos.x, mouseCellPos.y] = 1;
                    //��������������ܵ�ת������
                    previewRail.LinkNeighbour();
                }
            }
            else
            {
                Debug.Log("û�пɷ��õ����죡");
            }
        }
    }

    void CheckBreak()
    {
        if (!cellChange)
            return;
        if (MapBoundTest(new Vector2Int(mouseCellPos.x, mouseCellPos.y)))
        {
            if (obstacleArray[mouseCellPos.x, mouseCellPos.y] == 0 || obstacleArray[mouseCellPos.x, mouseCellPos.y] == 3 || player.GetPosition() == new Vector2Int(mouseCellPos.x, mouseCellPos.y))
            {
                previewMap.color = new Color(1, 0, 0, 0.5f);
            }
            else
            {
                previewMap.color = new Color(0, 1, 0, 0.5f);
            }
            previewMap.SetTile(mouseCellPos, highlight);
            cellChange = false;
        }
    }

    void BreakCell()
    {
        if (MapBoundTest(new Vector2Int(mouseCellPos.x, mouseCellPos.y)))
        {
            if (pickaxe > 0)
            {
                if (player.GetPosition() == new Vector2Int(mouseCellPos.x, mouseCellPos.y))
                {
                    Debug.Log("�����ƻ������ڷ��飡");
                }
                else if (obstacleArray[mouseCellPos.x, mouseCellPos.y] == 0 || obstacleArray[mouseCellPos.x, mouseCellPos.y] == 3)
                {
                    Debug.Log("û�п����ƻ��Ķ���");
                }
                else
                {
                    pickaxe--;

                    previewMap.color = new Color(1, 0, 0, 0.5f);
                    //����railArray��obstacleArray
                    if (obstacleArray[mouseCellPos.x, mouseCellPos.y] == 1)
                    {
                        //�ƻ�ǰ��¼��ǰ״̬
                        GameState gameState = new GameState(GameState.ActionType.Break, new Vector2Int(mouseCellPos.x, mouseCellPos.y), 
                                                            railArray[mouseCellPos.x, mouseCellPos.y], "");
                        stateStack.Push(gameState);

                        //�ƻ�����
                        railArray[mouseCellPos.x, mouseCellPos.y] = null;
                        railMap.SetTile(mouseCellPos, null);
                    }
                    else
                    {
                        //�ƻ�ǰ��¼��ǰ״̬
                        GameState gameState = new GameState(GameState.ActionType.Break, new Vector2Int(mouseCellPos.x, mouseCellPos.y), null, 
                                                            obstacleMap.GetTile(mouseCellPos).name);
                        stateStack.Push(gameState);

                        //�ƻ��ϰ���
                        obstacleMap.SetTile(mouseCellPos, null);
                    }
                    obstacleArray[mouseCellPos.x, mouseCellPos.y] = 0;
                }
            }
            else
            {
                Debug.Log("û�п�ʮ�ָ����ʹ�ã�");
            }
        }
    }

    void UpdateMousePos()
    {
        //�������λ��
        Vector3 mouseScreenPos = Input.mousePosition;
        mousePos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
        mainCamera.transparencySortAxis = new Vector3(0.49f, 2f, 0.49f);
    }

    public void RunStep()
    {
        //��ʼ��״̬��ʱ����
        state_round = round;
        state_bomb = false;
        state_pickaxe = false;
        state_checkpoint = false;
        state_objects = new List<GameObject>();
        state_checkPoints = new List<Vector2Int>();
        state_playerPos = player.GetPosition();
        state_playerForward = player.GetForwardPosition();
        state_railArray = new Rail[8, 8];
        state_obstacleArray = new int[8, 8];

        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                if (railArray[i, j] != null)
                    state_railArray[i, j] = (Rail)railArray[i, j].Clone();
                state_obstacleArray[i, j] = obstacleArray[i, j];
            }
        }

        //����һ��
        player.Move();
    }

    public void RunFinish()
    {
        //��ʼ��״̬��ʱ����
        state_round = round;
        state_bomb = false;
        state_pickaxe = false;
        state_checkpoint = false;
        state_objects = new List<GameObject>();
        state_checkPoints = new List<Vector2Int>();
        state_playerPos = player.GetPosition();
        state_playerForward = player.GetForwardPosition();
        state_railArray = new Rail[8, 8];
        state_obstacleArray = new int[8, 8];

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if(railArray[i, j] != null)
                    state_railArray[i, j] = (Rail)railArray[i, j].Clone();
                state_obstacleArray[i, j] = obstacleArray[i, j];
            }
        }

        //��������
        continuouslyMove = true;
        player.Move();
    }

    void StopGame()
    {
        //��ͣ��Ϸ
    }

    void Restart()
    {
        //���¼�����Ϸ
    }

    //���ƶ�ʱ��Ӧ���ò�����ť
    public static void BanButtons()
    {
        GameObject.Find("RunStep").GetComponent<Button>().interactable = false;
        GameObject.Find("Run").GetComponent<Button>().interactable = false;
        GameObject.Find("Undo").GetComponent<Button>().interactable = false;
        GameObject.Find("RailButton").GetComponent<Button>().interactable = false;
        GameObject.Find("PickaxeButton").GetComponent<Button>().interactable = false;
    }

    //���ð�ť
    public static void ActiveButtons()
    {
        GameObject.Find("RunStep").GetComponent<Button>().interactable = true;
        GameObject.Find("Run").GetComponent<Button>().interactable = true;
        GameObject.Find("Undo").GetComponent<Button>().interactable = true;
        //ʹ�õ��ߵİ�ť��Ҫ���ж�ʣ������
        if (rails > 0)
            GameObject.Find("RailButton").GetComponent<Button>().interactable = true;
        if(pickaxe > 0)
            GameObject.Find("PickaxeButton").GetComponent<Button>().interactable = true;
    }

    public static bool CheckWin()
    {
        if(checkPoints == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
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
