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

    //回合数，移动一格算一个回合
    public static int round;

    //特殊规则，史莱姆墙壁
    public bool slime;
    public static bool active_slime;

    //测试用起点终点坐标，是否沿用待定，TileMap坐标
    public Vector2Int startStation;
    public Vector2Int endStation;
    public static Vector2Int start;
    public static Vector2Int end;

    public static Train player;
    public static Rail[,] railArray;
    //障碍物表，0表示空地，1表示铁轨，2表示山，3表示湖，4表示史莱姆墙
    public static int[,] obstacleArray; 
    //道具表
    public static GameObject[,] propArray;
    public static ArrayList bombList;
    //检查点表
    public static bool[,] checkPointArray;
    public static Camera mainCamera;

    //记录鼠标坐标
    Vector2 mousePos;
    Vector3Int mouseCellPos;

    //状态栈
    public static StateStack<GameState> stateStack;

    public enum States
    { 
        等待操作,
        播放动画,
        放置铁轨,
        破坏方块
    };
    //游戏的当前操作状态
    public static States state;
    public static bool continuouslyMove;

    //网格变化判定
    bool cellChange = false;

    //铁轨放置预计算对象
    Rail previewRail;

    //Tilemap
    public static Tilemap railMap;
    public static Tilemap groundMap;
    public static Tilemap previewMap;
    public static Tilemap obstacleMap;

    //铁轨贴图，拐弯贴图以两个连接方向命名
    public static Tile rail_horizontal;
    public static Tile rail_vertical;
    public static Tile rail_leftDown;
    public static Tile rail_rightDown;
    public static Tile rail_leftUp;
    public static Tile rail_rightUp;

    //矿工十字镐贴图
    public static Tile highlight;

    //地面贴图
    public static Tile ground;

    //检查点贴图
    public static Tile checkPoint_true;
    public static Tile checkPoint_false;

    //障碍贴图
    public static Tile rock;
    //public static Tile slime;

    //全局参数
    public static int checkPoints;
    public int init_rails;
    public static int rails;
    public int init_pickaxe;
    public static int pickaxe;


    //状态压栈使用的临时变量
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
        //初始化相机
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        mainCamera.transparencySortAxis = new Vector3(0.49f, 2f, 0.49f);

        round = 0;
        active_slime = slime;

        //初始化状态栈
        stateStack = new StateStack<GameState>(20);

        //初始化Array
        railArray = new Rail[8, 8];
        obstacleArray = new int[8, 8];
        propArray = new GameObject[8, 8];
        bombList = new ArrayList();
        checkPointArray = new bool[8, 8];

        //初始化贴图
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

        //初始化Tilemap
        railMap = GameObject.Find("Rail").GetComponent<Tilemap>();
        groundMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        previewMap = GameObject.Find("Preview").GetComponent<Tilemap>();
        obstacleMap = GameObject.Find("Obstacle").GetComponent<Tilemap>();

        //参数初始化
        continuouslyMove = false;
        rails = init_rails;
        pickaxe = init_pickaxe;
        checkPoints = 0;

        //将TileMap信息转化为状态数组
        TilemapToArray();

        //初始化起点、终点和火车位置
        start = startStation;
        end = endStation;

        //起点TileMap坐标转世界坐标
        Vector3 startStationWorld = railMap.GetCellCenterWorld(new Vector3Int(startStation.x, startStation.y, 0));
        startStationWorld.y += 0.15f;

        //拷贝火车对象副本并初始化玩家对象
        player = Instantiate(train, startStationWorld, Quaternion.identity).GetComponent<Train>();

        //设置火车初始网格坐标和朝向
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

        //初始化道具信息
        InitializeProps();

        //初始化UI
        InitializeUI();
    }

    // Update is called once per frame
    void Update()
    {
        //更新鼠标位置
        UpdateMousePos();

        switch (state)
        {
            case States.等待操作:
                //测试用键盘事件监听
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
                        Debug.Log("没有可用的矿工镐");
                    }
                    else
                    {
                        state = States.破坏方块;
                    }
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    if(rails <= 0)
                    {
                        Debug.Log("没有可放置的铁轨");
                    }
                    else
                    {
                        state = States.放置铁轨;
                    }
                }
                break;
            case States.放置铁轨:
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
                    state = States.等待操作;
                }
                break;
            case States.破坏方块:
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
                    state = States.等待操作;
                }
                break;
        }

        //如果悬停的瓦片改变，重新进行预计算
    }

    void TilemapToArray()
    {
        //将TileMap信息转化为状态数组

        //关卡地图完备性检查
        if (groundMap == null || railMap == null || previewMap == null || obstacleMap == null)
        {
            Debug.Log("地图图层数据缺失！");
            return;
        }

        //初始化铁轨对象数组
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                //探测(i,j)坐标上的网格信息
                Vector3Int cellPosition = new Vector3Int(i, j, 0);
                //铁轨图层探测
                if(railMap.HasTile(cellPosition))
                {
                    obstacleArray[i,j] = 1;
                    railArray[i,j] = RailInitialize(i, j, railMap.GetTile(cellPosition).name);

                    //Debug.Log(railArray[i, j] + ",position:" + railArray[i, j].tilePosition + ",direction:" + railArray[i, j].linkDirection1 + "," + railArray[i, j].linkDirection2);

                }
                //地形图层探测
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
        //初始化矿工十字镐
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
                    Debug.Log("道具摆放存在重叠！");
                }
            }
            else
            {
                Debug.Log("地图道具摆放错误!");
            }
        }

        //初始化高爆炸弹(看后续设置)
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
                    Debug.Log("道具摆放存在重叠！");
                }
            }
            else
            {
                Debug.Log("地图道具摆放错误!");
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
        //计算网格位置
        Vector3Int newMouseCellPos = railMap.WorldToCell(mousePos);
        if (newMouseCellPos != mouseCellPos)
        {
            //如果指向网格改变，修改状态位并删除上一格的预览信息
            cellChange = true;
            previewMap.SetTile(mouseCellPos, null);
            mouseCellPos = newMouseCellPos;
        }
        //Debug.Log(mousePos+","+mouseCellPos);
    }

    void CheckRail()
    {
        //仅当指向网格坐标改变时预计算铁轨信息
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
                    Debug.Log("不能将铁轨放置在障碍物上！");
                }
                else
                {
                    //先对当前状态压栈
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

                    //更新
                    rails--;
                    //if( rails==0 )
                    //{
                    //    state = States.等待操作;
                    //}

                    //放置铁轨
                    previewRail.SetTile();
                    if(!groundMap.HasTile(mouseCellPos))
                    {
                        groundMap.SetTile(mouseCellPos, ground);
                    }
                    //更新railArray和obstacleArray
                    railArray[mouseCellPos.x, mouseCellPos.y] = previewRail;
                    obstacleArray[mouseCellPos.x, mouseCellPos.y] = 1;
                    //处理连接铁轨可能的转向问题
                    previewRail.LinkNeighbour();
                }
            }
            else
            {
                Debug.Log("没有可放置的铁轨！");
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
                    Debug.Log("不能破坏火车所在方块！");
                }
                else if (obstacleArray[mouseCellPos.x, mouseCellPos.y] == 0 || obstacleArray[mouseCellPos.x, mouseCellPos.y] == 3)
                {
                    Debug.Log("没有可以破坏的对象！");
                }
                else
                {
                    pickaxe--;

                    previewMap.color = new Color(1, 0, 0, 0.5f);
                    //更新railArray和obstacleArray
                    if (obstacleArray[mouseCellPos.x, mouseCellPos.y] == 1)
                    {
                        //破坏前记录当前状态
                        GameState gameState = new GameState(GameState.ActionType.Break, new Vector2Int(mouseCellPos.x, mouseCellPos.y), 
                                                            railArray[mouseCellPos.x, mouseCellPos.y], "");
                        stateStack.Push(gameState);

                        //破坏铁轨
                        railArray[mouseCellPos.x, mouseCellPos.y] = null;
                        railMap.SetTile(mouseCellPos, null);
                    }
                    else
                    {
                        //破坏前记录当前状态
                        GameState gameState = new GameState(GameState.ActionType.Break, new Vector2Int(mouseCellPos.x, mouseCellPos.y), null, 
                                                            obstacleMap.GetTile(mouseCellPos).name);
                        stateStack.Push(gameState);

                        //破坏障碍物
                        obstacleMap.SetTile(mouseCellPos, null);
                    }
                    obstacleArray[mouseCellPos.x, mouseCellPos.y] = 0;
                }
            }
            else
            {
                Debug.Log("没有矿工十字镐可以使用！");
            }
        }
    }

    void UpdateMousePos()
    {
        //更新鼠标位置
        Vector3 mouseScreenPos = Input.mousePosition;
        mousePos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
        mainCamera.transparencySortAxis = new Vector3(0.49f, 2f, 0.49f);
    }

    public void RunStep()
    {
        //初始化状态临时变量
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

        //运行一步
        player.Move();
    }

    public void RunFinish()
    {
        //初始化状态临时变量
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

        //连续运行
        continuouslyMove = true;
        player.Move();
    }

    void StopGame()
    {
        //暂停游戏
    }

    void Restart()
    {
        //重新加载游戏
    }

    //火车移动时，应禁用操作按钮
    public static void BanButtons()
    {
        GameObject.Find("RunStep").GetComponent<Button>().interactable = false;
        GameObject.Find("Run").GetComponent<Button>().interactable = false;
        GameObject.Find("Undo").GetComponent<Button>().interactable = false;
        GameObject.Find("RailButton").GetComponent<Button>().interactable = false;
        GameObject.Find("PickaxeButton").GetComponent<Button>().interactable = false;
    }

    //启用按钮
    public static void ActiveButtons()
    {
        GameObject.Find("RunStep").GetComponent<Button>().interactable = true;
        GameObject.Find("Run").GetComponent<Button>().interactable = true;
        GameObject.Find("Undo").GetComponent<Button>().interactable = true;
        //使用道具的按钮需要先判定剩余数量
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
        //地图数组越界判断，如果cellPos越界，返回false，否则返回true
        if(cellPos.x >= 0 && cellPos.x < 8 && cellPos.y >= 0 && cellPos.y < 8)
        {
            return true;
        }
        return false;
    }
}
