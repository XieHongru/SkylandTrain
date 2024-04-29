using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateStack<T>
{
    //栈最大深度
    int depth;
    //当前栈顶，栈底
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
            Debug.Log("操作栈为空");
            return default(T);
        }
    }

    public void Push(T obj)
    {
        top = (top + 1) % depth;
        //如果栈溢出
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
