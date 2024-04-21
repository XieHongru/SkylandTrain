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

    public enum States
    { 
        等待操作,
        播放动画
    };
    public static States state;
    // Start is called before the first frame update
    void Start()
    {
        Tilemap railMap = GameObject.Find("Rail").GetComponent<Tilemap>();

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
        //测试用键盘事件监听
        if(state==States.等待操作)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                player.Move();
            }
        }
        //更新鼠标位置

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

    void SetRail()
    {
        //放置铁轨

        //更新railMap
    }

    void BreakRail()
    {
        //破坏铁轨

        //更新railMap
    }

    void BreakBarrier()
    {
        //破坏障碍物

        //更新objectMap?
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
}
