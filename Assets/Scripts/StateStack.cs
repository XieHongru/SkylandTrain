using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateStack<T>
{
    //ջ������
    int depth;
    //��ǰջ����ջ��
    int top;
    int bottom;

    T[] stack;

    public StateStack(int depth)
    {
        this.depth = depth;
        stack = new T[depth];
        top = 0;
        bottom = 0;
    }

    public T Pop()
    {
        if (stack[top] != null && top != bottom)
        {
            T ret = stack[top];
            stack[top] = default(T);
            --top;
            if(top<0)
            {
                top += depth;
            }
            return ret;
        }
        else
        {
            Debug.Log("����ջΪ��");
            return default(T);
        }
    }

    public void Push(T obj)
    {
        top = (top + 1) % depth;
        //���ջ���
        if(top == bottom)
        {
            bottom++;
        }

        stack[top] = obj;
    }

    public T Peek()
    {
        return stack[top];
    }
}
