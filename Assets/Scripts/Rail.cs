using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail
{
    bool isValid;

    //贴图，拐弯贴图以两个连接方向命名（大量副本感觉会浪费内存，放到什么地方会好一点）
    public Sprite Horizontal;
    public Sprite Vertical;
    public Sprite leftDown;
    public Sprite rightDown;
    public Sprite leftUp;
    public Sprite rightUp;

    Vector2 tilePosition;
    Vector2 linkDirection1;
    Vector2 linkDirection2;

    public Rail(Vector2 pos, string tileName)
    {
        this.tilePosition = pos;
        switch(tileName) 
        {
            case "rail_horizontal":
                linkDirection1 = Vector2.left;
                linkDirection2 = Vector2.right;
                break;
            case "rail_vertical":
                linkDirection1 = Vector2.down;
                linkDirection2 = Vector2.up;
                break;
            case "rail_leftDown":
                linkDirection1 = Vector2.left;
                linkDirection2 = Vector2.down;
                break;
            case "rail_leftUp":
                linkDirection1 = Vector2.left;
                linkDirection2 = Vector2.up;
                break;
            case "rail_rightDown":
                linkDirection1 = Vector2.right;
                linkDirection2 = Vector2.down;
                break;
            case "rail_rightUp":
                linkDirection1 = Vector2.right;
                linkDirection2 = Vector2.up;
                break;
            default:
                Debug.Log("False Sprite!");
                break;
        }
    }

    public void PreCalculate()
    {
        //形态预计算
    }

    public Vector2 GetLinkDirection1()
    {
        return linkDirection1;
    }

    public Vector2 GetLinkDirection2()
    {
        return linkDirection2;
    }
}
