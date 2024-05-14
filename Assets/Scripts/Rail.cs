using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Rail : ICloneable
{
    static Vector2Int[] directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    public Tile tile;
    Vector2Int tilePosition;
    Vector2Int linkDirection1;
    Vector2Int linkDirection2;

    public Rail(Vector2Int pos, string tileName)
    {
        tilePosition = pos;
        SetLinkDirection(tileName);
    }

    public Rail(Vector2Int pos)
    {
        tilePosition = pos;
    }

    public Rail()
    {

    }

    public object Clone()
    {
        Rail obj = (Rail)MemberwiseClone();
        obj.tile = tile;
        return obj;
    }

    public void PreCalculate()
    {
        //形态预计算，默认为横向
        tile = GameManager.rail_horizontal;
        linkDirection1 = Vector2Int.left;
        linkDirection2 = Vector2Int.right;

        //遍历四个相邻方向，记录有铁轨的方向
        ArrayList connection = new ArrayList();

        Vector2Int detectPos = tilePosition;
        for (int i = 0; i < 4; ++i)
        {
            detectPos = tilePosition + directions[i];
            if (GameManager.MapBoundTest(detectPos))
            {
                Rail detectRail = GameManager.railArray[detectPos.x, detectPos.y];
                if (detectRail != null)
                {
                    //该方向有铁轨，检查该铁轨是否还能参与连接
                    Vector2Int detectPos1 = detectRail.tilePosition + detectRail.GetLinkDirection1();
                    Vector2Int detectPos2 = detectRail.tilePosition + detectRail.GetLinkDirection2();
                    Rail linkedRail1 = null;
                    Rail linkedRail2 = null;
                    if (GameManager.MapBoundTest(detectPos1))
                    {
                        linkedRail1 = GameManager.railArray[detectPos1.x, detectPos1.y];
                    }
                    if (GameManager.MapBoundTest(detectPos2))
                    {
                        linkedRail2 = GameManager.railArray[detectPos2.x, detectPos2.y];
                    }
                    //如果连接方向出现没有铁轨的情况，说明该铁轨是可以连接的
                    if ( !CheckLink(linkedRail1, -detectRail.GetLinkDirection1()) || !CheckLink(linkedRail2, -detectRail.GetLinkDirection2()) )
                    {
                        connection.Add(i);
                    }
                }
            }
        }

        if (connection.Count == 1)
        {
            //如果铁轨只与一个方向连接，直接根据该方向设置铁轨为横向或纵向
            if ((int)connection[0] < 2)
            {
                SetLinkDirection("rail_horizontal");
                tile = GameManager.rail_horizontal;
            }
            else
            {
                SetLinkDirection("rail_vertical");
                tile = GameManager.rail_vertical;
            }
        }
        else if (connection.Count == 2)
        {
            //打表，需要注意编码和为3时对应两种情况
            int value = (int)connection[0] + (int)connection[1];
            switch (value)
            {
                case 1:
                    SetLinkDirection("rail_horizontal");
                    tile = GameManager.rail_horizontal; break;
                case 2:
                    SetLinkDirection("rail_leftUp");
                    tile = GameManager.rail_leftUp; break;
                case 3:
                    if ((int)connection[0] == 0)
                    {
                        SetLinkDirection("rail_leftDown");
                        tile = GameManager.rail_leftDown;
                    }
                    else
                    {
                        SetLinkDirection("rail_rightUp");
                        tile = GameManager.rail_rightUp;
                    }
                    break;
                case 4:
                    SetLinkDirection("rail_rightDown");
                    tile = GameManager.rail_rightDown; break;
                case 5:
                    SetLinkDirection("rail_vertical");
                    tile = GameManager.rail_vertical; break;
            }
        }
        else if (connection.Count == 3)
        {
            //三个方向有铁轨时必定连接为直轨
            int value = (int)connection[0] + (int)connection[1] + (int)connection[2];

            //打表，减去未连接方向的编码正好一一对应
            switch (value)
            {
                case 3:
                case 4:
                    SetLinkDirection("rail_horizontal");
                    tile = GameManager.rail_horizontal; break;
                case 5:
                case 6:
                    SetLinkDirection("rail_vertical");
                    tile = GameManager.rail_vertical; break;
            }
        }
        else if (connection.Count == 4)
        {
            Rail detectRail = null;
            //横向铁轨计数
            int count_horizontal = 0;
            //向左计数
            detectPos = tilePosition + Vector2Int.left;
            if (GameManager.MapBoundTest(detectPos))
                detectRail = GameManager.railArray[detectPos.x, detectPos.y];
            while (detectRail != null)
            {
                count_horizontal++;
                detectPos = detectRail.tilePosition + Vector2Int.left;
                if(GameManager.MapBoundTest(detectPos))
                {
                    detectRail = GameManager.railArray[detectPos.x, detectPos.y];
                    //还需要判断探测目标铁轨是否在该方向上是相连的
                    if (GameManager.railArray[detectPos.x, detectPos.y] == null || (GameManager.railArray[detectPos.x, detectPos.y].GetLinkDirection1() != Vector2Int.right &&
                        GameManager.railArray[detectPos.x, detectPos.y].GetLinkDirection2() != Vector2Int.right))
                    {
                        detectRail = null;
                    }
                }
                else
                {
                    detectRail = null;
                }
            }
            //向右计数
            detectRail = null;
            detectPos = tilePosition + Vector2Int.right;
            if (GameManager.MapBoundTest(detectPos))
                detectRail = GameManager.railArray[detectPos.x, detectPos.y];
            while (detectRail != null)
            {
                count_horizontal++;
                detectPos = detectRail.tilePosition + Vector2Int.right;
                if (GameManager.MapBoundTest(detectPos))
                {
                    detectRail = GameManager.railArray[detectPos.x, detectPos.y];
                    //还需要判断探测目标铁轨是否在该方向上是相连的
                    if (GameManager.railArray[detectPos.x, detectPos.y] == null || (GameManager.railArray[detectPos.x, detectPos.y].GetLinkDirection1() != Vector2Int.left &&
                        GameManager.railArray[detectPos.x, detectPos.y].GetLinkDirection2() != Vector2Int.left))
                    {
                        detectRail = null;
                    }
                }
                else
                {
                    detectRail = null;
                }
            }
            //纵向铁轨计数
            int count_vertical = 0;
            //向上计数
            detectRail = null;
            detectPos = tilePosition + Vector2Int.up;
            if (GameManager.MapBoundTest(detectPos))
                detectRail = GameManager.railArray[detectPos.x, detectPos.y];
            while (detectRail != null)
            {
                count_vertical++;
                detectPos = detectRail.tilePosition + Vector2Int.up;
                if (GameManager.MapBoundTest(detectPos))
                {
                    detectRail = GameManager.railArray[detectPos.x, detectPos.y];
                    //还需要判断探测目标铁轨是否在该方向上是相连的
                    if (GameManager.railArray[detectPos.x, detectPos.y] == null || (GameManager.railArray[detectPos.x, detectPos.y].GetLinkDirection1() != Vector2Int.down &&
                        GameManager.railArray[detectPos.x, detectPos.y].GetLinkDirection2() != Vector2Int.down))
                    {
                        detectRail = null;
                    }
                }
                else
                {
                    detectRail = null;
                }
            }
            //向下计数
            detectRail = null;
            detectPos = tilePosition + Vector2Int.down;
            if (GameManager.MapBoundTest(detectPos))
                detectRail = GameManager.railArray[detectPos.x, detectPos.y];
            while (detectRail != null)
            {
                count_vertical++;
                detectPos = detectRail.tilePosition + Vector2Int.down;
                if (GameManager.MapBoundTest(detectPos))
                {
                    detectRail = GameManager.railArray[detectPos.x, detectPos.y];
                    //还需要判断探测目标铁轨是否在该方向上是相连的
                    if (GameManager.railArray[detectPos.x, detectPos.y] == null || (GameManager.railArray[detectPos.x, detectPos.y].GetLinkDirection1() != Vector2Int.up &&
                        GameManager.railArray[detectPos.x, detectPos.y].GetLinkDirection2() != Vector2Int.up))
                    {
                        detectRail = null;
                    }
                }
                else
                {
                    detectRail = null;
                }
            }

            //仅当连接后的纵向铁轨数大于横向才设置为纵向
            if(count_vertical > count_horizontal)
            {
                SetLinkDirection("rail_vertical");
                tile = GameManager.rail_vertical;
            }
        }


        GameManager.previewMap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, 0), tile);
    }

    public void LinkNeighbour()
    {
        Vector2Int detectPos1 = tilePosition + linkDirection1;
        Vector2Int detectPos2 = tilePosition + linkDirection2;
        Rail linkedRail1 = null;
        Rail linkedRail2 = null;
        if(GameManager.MapBoundTest(detectPos1))
        {
            linkedRail1 = GameManager.railArray[detectPos1.x, detectPos1.y];
        }
        if (GameManager.MapBoundTest(detectPos2))
        {
            linkedRail2 = GameManager.railArray[detectPos2.x, detectPos2.y];
        }

        //如果相邻铁轨1已经连接或火车位于该处，什么也不做
        if (linkedRail1 == null || GameManager.player.GetPosition() == detectPos1 ||
            linkedRail1.GetLinkDirection1() == this.tilePosition - linkedRail1.tilePosition ||
            linkedRail1.GetLinkDirection2() == this.tilePosition - linkedRail1.tilePosition)
        {
            
        }
        else
        {
            //查找相邻铁轨1的连接方向1上是否有铁轨
            Vector2Int subDetectPos1 = detectPos1 + linkedRail1.GetLinkDirection1();
            Vector2Int subDetectPos2 = detectPos1 + linkedRail1.GetLinkDirection2();
            if (GameManager.MapBoundTest(subDetectPos1) && CheckLink( GameManager.railArray[subDetectPos1.x, subDetectPos1.y], -linkedRail1.GetLinkDirection1()))
            {
                //如果有，则查找相邻铁轨1的连接方向2上是否有铁轨
                if (!GameManager.MapBoundTest(subDetectPos2) || !CheckLink(GameManager.railArray[subDetectPos2.x, subDetectPos2.y], -linkedRail1.GetLinkDirection2()))
                {
                    //如果方向2上没有铁轨则连接，否则跳过处理该相邻铁轨
                    linkedRail1.SetLinkDirection2(this.tilePosition - linkedRail1.tilePosition);
                }
            }
            else
            {
                //如果没有，则将其连接方向1与新铁轨相连
                linkedRail1.SetLinkDirection1(this.tilePosition - linkedRail1.tilePosition);
            }

            //计算改变后铁轨贴图
            ArrayList connection = new ArrayList();
            for (int i = 0; i < 4; ++i)
            {
                if (linkedRail1.GetLinkDirection1() == directions[i] || linkedRail1.GetLinkDirection2() == directions[i])
                {
                    connection.Add(i);
                }
            }
            linkedRail1.CalRailSprite((int)connection[0] + (int)connection[1], (int)connection[0], linkedRail1);
            linkedRail1.SetTile();
        }

        //如果相邻铁轨2已经连接或火车位于该处，什么也不做
        if (linkedRail2 == null || GameManager.player.GetPosition() == detectPos2 ||
            linkedRail2.GetLinkDirection1() == this.tilePosition - linkedRail2.tilePosition ||
            linkedRail2.GetLinkDirection2() == this.tilePosition - linkedRail2.tilePosition)
        {
            
        }
        else
        {
            Vector2Int subDetectPos1 = detectPos2 + linkedRail2.GetLinkDirection1();
            Vector2Int subDetectPos2 = detectPos2 + linkedRail2.GetLinkDirection2();
            //查找相邻铁轨2的连接方向1上是否有铁轨
            if (GameManager.MapBoundTest(subDetectPos1) && CheckLink(GameManager.railArray[subDetectPos1.x, subDetectPos1.y], -linkedRail2.GetLinkDirection1()))
            {
                //如果有，则查找相邻铁轨2的连接方向2上是否有铁轨
                if (!GameManager.MapBoundTest(subDetectPos2) || !CheckLink(GameManager.railArray[subDetectPos2.x, subDetectPos2.y], -linkedRail2.GetLinkDirection2()))
                {
                    //如果方向2上没有铁轨则连接，否则跳过处理该相邻铁轨
                    linkedRail2.SetLinkDirection2(this.tilePosition - linkedRail2.tilePosition);
                }
            }
            else
            {
                //如果没有，则将其连接方向1与新铁轨相连
                linkedRail2.SetLinkDirection1(this.tilePosition - linkedRail2.tilePosition);
            }

            //计算改变后铁轨贴图
            ArrayList connection = new ArrayList();
            for (int i = 0; i < 4; ++i)
            {
                if (linkedRail2.GetLinkDirection1() == directions[i] || linkedRail2.GetLinkDirection2() == directions[i])
                {
                    connection.Add(i);
                }
            }
            linkedRail2.CalRailSprite((int)connection[0] + (int)connection[1], (int)connection[0], linkedRail2);
            linkedRail2.SetTile();
        }
    }

    void SetLinkDirection(string dir)
    {
        switch (dir)
        {
            case "rail_horizontal":
                linkDirection1 = Vector2Int.left;
                linkDirection2 = Vector2Int.right;
                tile = GameManager.rail_horizontal;
                break;
            case "rail_vertical":
                linkDirection1 = Vector2Int.down;
                linkDirection2 = Vector2Int.up;
                tile = GameManager.rail_vertical;
                break;
            case "rail_leftDown":
                linkDirection1 = Vector2Int.left;
                linkDirection2 = Vector2Int.down;
                tile = GameManager.rail_leftDown;
                break;
            case "rail_leftUp":
                linkDirection1 = Vector2Int.left;
                linkDirection2 = Vector2Int.up;
                tile = GameManager.rail_leftUp;
                break;
            case "rail_rightDown":
                linkDirection1 = Vector2Int.right;
                linkDirection2 = Vector2Int.down;
                tile = GameManager.rail_rightDown;
                break;
            case "rail_rightUp":
                linkDirection1 = Vector2Int.right;
                linkDirection2 = Vector2Int.up;
                tile = GameManager.rail_rightUp;
                break;
            default:
                Debug.Log("False Sprite!");
                break;
        }
    }

    bool CheckLink(Rail rail, Vector2Int linkdir)
    {
        if(rail == null) 
            return false;
        if(rail.GetLinkDirection1() == linkdir || rail.GetLinkDirection2() == linkdir)
            return true;
        return false;
    }

    public Vector2Int GetPosition()
    {
        return tilePosition;
    }

    public Vector2Int GetLinkDirection1()
    {
        return linkDirection1;
    }

    public Vector2Int GetLinkDirection2()
    {
        return linkDirection2;
    }

    public void SetLinkDirection1(Vector2Int direction)
    {
        linkDirection1 = direction;
    }

    public void SetLinkDirection2(Vector2Int direction)
    {
        linkDirection2 = direction;
    }

    public void SetTile()
    {
        GameManager.railMap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, 0), tile);
    }

    //对于连接方向编码值的和value，设置rail对象当前瓦片贴图形态
    void CalRailSprite(int value, int connection0, Rail rail)
    {
        //计算铁轨形态
        switch (value)
        {
            case 1:
                rail.tile = GameManager.rail_horizontal; break;
            case 2:
                rail.tile = GameManager.rail_leftUp; break;
            case 3:
                if (connection0 == 0)
                {
                    rail.tile = GameManager.rail_leftDown;
                }
                else
                {
                    rail.tile = GameManager.rail_rightUp;
                }
                break;
            case 4:
                rail.tile = GameManager.rail_rightDown; break;
            case 5:
                rail.tile = GameManager.rail_vertical; break;
        }
    }
}
