using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public void ChangeStateToSetRail()
    {
        //Debug.Log("放置铁轨！");
        GameManager.state = GameManager.States.放置铁轨;
    }

    public void ChangeStateToUsePickaxe()
    {
        //Debug.Log("使用十字镐！");
        GameManager.state = GameManager.States.破坏方块;
    }

    public void RunStep()
    {
        if(GameManager.state != GameManager.States.播放动画)
            GameObject.Find("GameManager").GetComponent<GameManager>().RunStep();
    }

    public void Run()
    {
        if (GameManager.state != GameManager.States.播放动画)
            GameObject.Find("GameManager").GetComponent<GameManager>().RunFinish();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Undo()
    {
        GameState state = GameManager.stateStack.Pop();
        if(state != null )
        {
            state.Undo();
        }
    }
}
