using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public void ChangeStateToSetRail()
    {
        //Debug.Log("�������죡");
        GameManager.state = GameManager.States.��������;
    }

    public void ChangeStateToUsePickaxe()
    {
        //Debug.Log("ʹ��ʮ�ָ䣡");
        GameManager.state = GameManager.States.�ƻ�����;
    }

    public void RunStep()
    {
        if(GameManager.state != GameManager.States.���Ŷ���)
            GameObject.Find("GameManager").GetComponent<GameManager>().RunStep();
    }

    public void Run()
    {
        if (GameManager.state != GameManager.States.���Ŷ���)
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
