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
        //将TileMap信息转化为状态数组
        TilemapToArray();

        //初始化起点、终点和火车位置

        //初始化道具信息
    }

    // Update is called once per frame
    void Update()
    {
        //更新鼠标位置

        //如果悬停的瓦片改变，重新进行预计算
    }

    void TilemapToArray()
    {
        //将TileMap信息转化为状态数组
        //Tilemap gameMap = GameObject.Find("Ground").GetComponent<Tilemap>();

        //检查并初始化铁轨对象数组
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
            Debug.Log("没有找到Tilemap中的铁轨图层");
        }
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
