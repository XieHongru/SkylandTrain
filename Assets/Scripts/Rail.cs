using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    bool isValid;

    //��ͼ��������ͼ���������ӷ������������������о����˷��ڴ棬�ŵ�ʲô�ط����һ�㣩
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
        //��̬Ԥ����
    }
}
