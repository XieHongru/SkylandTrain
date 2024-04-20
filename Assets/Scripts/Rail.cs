using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
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

    public void PreCalculate()
    {
        //形态预计算
    }
}
