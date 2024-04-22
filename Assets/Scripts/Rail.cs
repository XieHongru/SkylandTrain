using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Rail
{
    bool isValid;

    static Vector2Int[] directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    Tile tile;
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

    public void PreCalculate()
    {
        //��̬Ԥ���㣬Ĭ��Ϊ����
        tile = GameManager.rail_horizontal;
        linkDirection1 = Vector2Int.left;
        linkDirection2 = Vector2Int.right;

        //�����ĸ����ڷ��򣬼�¼������ķ���
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
                    //�÷��������죬���������Ƿ��ܲ�������
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
                    //������ӷ������û������������˵���������ǿ������ӵ�
                    if (linkedRail1 == null || linkedRail2 == null)
                    {
                        connection.Add(i);
                    }
                }
            }
        }

        if (connection.Count == 1)
        {
            //�������ֻ��һ���������ӣ�ֱ�Ӹ��ݸ÷�����������Ϊ���������
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
            //�����Ҫע������Ϊ3ʱ��Ӧ�������
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
            //��������������ʱ�ض�����Ϊֱ��
            int value = (int)connection[0] + (int)connection[1] + (int)connection[2];

            //�����ȥδ���ӷ���ı�������һһ��Ӧ
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

        //�����������1�Ѿ����ӣ�ʲôҲ����
        if (linkedRail1 == null ||
            linkedRail1.GetLinkDirection1() == this.tilePosition - linkedRail1.tilePosition ||
            linkedRail1.GetLinkDirection2() == this.tilePosition - linkedRail1.tilePosition)
        {
            
        }
        else
        {
            //������������1�����ӷ���2���Ƿ������죬����У��������ӷ���1������������
            Vector2Int subDetectPos = detectPos1 + linkedRail1.GetLinkDirection2();
            if (subDetectPos.x >= 0 && subDetectPos.x < 8 && subDetectPos.y >= 0 && subDetectPos.y < 8 &&
                GameManager.railArray[subDetectPos.x, subDetectPos.y] != null)
            {
                linkedRail1.SetLinkDirection1(this.tilePosition - linkedRail1.tilePosition);
            }
            else
            {
                //���û�У��������ӷ���2������������
                linkedRail1.SetLinkDirection2(this.tilePosition - linkedRail1.tilePosition);
            }
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

        //�����������2�Ѿ����ӣ�ʲôҲ����
        if (linkedRail2 == null || 
            linkedRail2.GetLinkDirection1() == this.tilePosition - linkedRail2.tilePosition ||
            linkedRail2.GetLinkDirection2() == this.tilePosition - linkedRail2.tilePosition)
        {
            
        }
        else
        {
            //������������2�����ӷ���2���Ƿ������죬����У��������ӷ���1������������
            Vector2Int subDetectPos = detectPos2 + linkedRail2.GetLinkDirection2();
            if (GameManager.MapBoundTest(subDetectPos) &&
                GameManager.railArray[subDetectPos.x, subDetectPos.y] != null)
            {
                linkedRail2.SetLinkDirection1(this.tilePosition - linkedRail2.tilePosition);
            }
            else
            {
                //���û�У��������ӷ���2������������
                linkedRail2.SetLinkDirection2(this.tilePosition - linkedRail2.tilePosition);
            }
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
                break;
            case "rail_vertical":
                linkDirection1 = Vector2Int.down;
                linkDirection2 = Vector2Int.up;
                break;
            case "rail_leftDown":
                linkDirection1 = Vector2Int.left;
                linkDirection2 = Vector2Int.down;
                break;
            case "rail_leftUp":
                linkDirection1 = Vector2Int.left;
                linkDirection2 = Vector2Int.up;
                break;
            case "rail_rightDown":
                linkDirection1 = Vector2Int.right;
                linkDirection2 = Vector2Int.down;
                break;
            case "rail_rightUp":
                linkDirection1 = Vector2Int.right;
                linkDirection2 = Vector2Int.up;
                break;
            default:
                Debug.Log("False Sprite!");
                break;
        }
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

    void CalRailSprite(int value, int connection0, Rail rail)
    {
        //����������̬
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
