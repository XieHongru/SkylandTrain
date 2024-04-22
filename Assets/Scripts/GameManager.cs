using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public GameObject train;

    //测试用起点终点坐标，是否沿用待定，TileMap坐标
    public Vector2Int startStation;
    public Vector2Int endStation;

    public static Train player;
    public static Rail[,] railArray = new Rail[8,8];
    public static Camera mainCamera;

    //记录鼠标坐标
    Vector2 mousePos;
    Vector3Int mouseCellPos;

    public enum States
    { 
        等待操作,
        播放动画,
        放置铁轨
    };
    //游戏的当前操作状态
    public static States state;

    //铁轨放置预计算对象
    Rail previewRail;

    //Tilemap
    public static Tilemap railMap;
    public static Tilemap groundMap;
    public static Tilemap previewMap;

    //铁轨贴图，拐弯贴图以两个连接方向命名（大量副本感觉会浪费内存，放到什么地方会好一点）
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
        //初始化相机
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        //初始化贴图
        rail_horizontal = horizontal;
        rail_vertical = vertical; 
        rail_leftDown = leftDown;
        rail_rightDown = rightDown;
        rail_leftUp = leftUp;
        rail_rightUp = rightUp;

        //初始化Tilemap
        railMap = GameObject.Find("Rail").GetComponent<Tilemap>();
        groundMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        previewMap = GameObject.Find("Preview").GetComponent<Tilemap>();

        //将TileMap信息转化为状态数组
        TilemapToArray(railMap);

        //初始化起点、终点和火车位置

        //起点TileMap坐标转世界坐标
        Vector3 startStationWorld = railMap.GetCellCenterWorld(new Vector3Int(startStation.x, startStation.y, 0));
        startStationWorld.y += 0.25f;

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
                    player.Move();
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    state = States.放置铁轨;
                }
                break;
            case States.放置铁轨:
                CheckRail();
                if (Input.GetMouseButtonDown(0))
                {
                    SetRail();
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    state = States.等待操作;
                }
                break;
        }

        //如果悬停的瓦片改变，重新进行预计算
    }

    void TilemapToArray(Tilemap railMap)
    {
        //将TileMap信息转化为状态数组
        //Tilemap gameMap = GameObject.Find("Ground").GetComponent<Tilemap>();

        //检查并初始化铁轨对象数组
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
            Debug.Log("没有找到Tilemap中的铁轨图层");
        }
    }

    Rail RailInitialize(int x, int y,string tileName)
    {
        return new Rail(new Vector2Int(x, y), tileName);
    }

    void CheckRail()
    {
        //计算网格位置
        Vector3Int newMouseCellPos = railMap.WorldToCell(mousePos);
        bool cellChange = false;
        if(newMouseCellPos!= mouseCellPos)
        {
            //如果指向网格改变，修改状态位并删除上一格的预览信息
            cellChange = true;
            previewMap.SetTile(mouseCellPos, null);
            mouseCellPos = newMouseCellPos;
        }
        //Debug.Log(mousePos+","+mouseCellPos);

        //仅当指向网格坐标改变时预计算铁轨信息
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
            //放置铁轨
            previewRail.SetTile();
            //更新railArray
            railArray[mouseCellPos.x, mouseCellPos.y] = previewRail;
            //处理连接铁轨可能的转向问题
            previewRail.LinkNeighbour();
        }
    }

    void BreakRail()
    {
        //破坏铁轨

        //更新railArray
    }

    void BreakBarrier()
    {
        //破坏障碍物

        //更新objectArray?
    }

    void UpdateMousePos()
    {
        //更新鼠标位置
        Vector3 mouseScreenPos = Input.mousePosition;
        mousePos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));

    }

    void RunStep()
    {
        //运行一步
    }

    void RunFinish()
    {
        //连续运行
    }

    void StopGame()
    {
        //暂停游戏
    }

    void Restart()
    {
        //重新加载游戏
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
