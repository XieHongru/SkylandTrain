using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static int level = 1;
    public static AudioSource audioSource;

    void Awake()
    {
        if (audioSource == null)
        {
            GameObject audioObject = new GameObject("AudioPlayer");
            audioObject.AddComponent<AudioSource>();
            audioSource = audioObject.GetComponent<AudioSource>();
            DontDestroyOnLoad(audioObject);
        }
        else
        {
            
        }
    }

    public void ChangeStateToSetRail()
    {
        //Debug.Log("放置铁轨！");
        GameManager.state = GameManager.States.放置铁轨;
        GameManager.audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        GameManager.audioSource.Play();
    }

    public void ChangeStateToUsePickaxe()
    {
        //Debug.Log("使用十字镐！");
        GameManager.state = GameManager.States.破坏方块;
        GameManager.audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        GameManager.audioSource.Play();
    }

    public void RunStep()
    {
        if (GameManager.state != GameManager.States.播放动画)
            GameObject.Find("GameManager").GetComponent<GameManager>().RunStep();
        GameManager.audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        GameManager.audioSource.Play();
    }

    public void Run()
    {
        if (GameManager.state != GameManager.States.播放动画)
            GameObject.Find("GameManager").GetComponent<GameManager>().RunFinish();
        GameManager.audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        GameManager.audioSource.Play();
    }

    public void Retry()
    {
        GameManager.ActiveButtons();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        GameManager.audioSource.Play();
    }

    public void Undo()
    {
        GameState state = GameManager.stateStack.Pop();
        if(state != null )
        {
            GameManager.ActiveButtons();
            state.Undo();
        }
        GameManager.audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        GameManager.audioSource.Play();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameIntroduction");
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
    }

    public void SelectGame()
    {
        SceneManager.LoadScene("SelectScene");
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
    }

    public void Config()
    {
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
    }

    public void ExitGame()
    {
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
        Application.Quit();
    }

    public void RunGame()
    {
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
        string s = "Game" + level;
        GameObject.Find("LoadMask").GetComponent<Animator>().Play("loadscene");
        //SceneManager.LoadScene(s);
    }

    public void GoBack()
    {
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
        SceneManager.LoadScene("MainMenu");
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
    }

    public void PlusLevel()
    {
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
        string s = GameObject.Find("LevelText").GetComponent<Text>().text;
        int cur_level = int.Parse(s) + 1;
        level = cur_level;
        //Debug.Log(level);
        if (cur_level >= 13)
        {
            cur_level = 13;
            level = 13;
            GameObject.Find("PlusButton").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("MinusButton").GetComponent<Button>().interactable = true;
        }

        GameObject.Find("LevelText").GetComponent<Text>().text = cur_level.ToString();

        if(cur_level >= 10 && cur_level <= 13)
        {
            GameObject.Find("Slime").GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
        else
        {
            GameObject.Find("Slime").GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }
    }

    public void MinusLevel()
    {
        audioSource.clip = Resources.Load<AudioClip>("Audios/Click");
        audioSource.Play();
        string s = GameObject.Find("LevelText").GetComponent<Text>().text;
        int cur_level = int.Parse(s) - 1;
        level = cur_level;
        if (cur_level <= 1)
        {
            cur_level = 1;
            level = 1;
            GameObject.Find("MinusButton").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("PlusButton").GetComponent<Button>().interactable = true;
        }

        GameObject.Find("LevelText").GetComponent<Text>().text = cur_level.ToString();

        if (cur_level >= 10 && cur_level <= 13)
        {
            GameObject.Find("Slime").GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
        else
        {
            GameObject.Find("Slime").GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }
    }

}
