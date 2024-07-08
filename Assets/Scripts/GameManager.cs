using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject BattleBackground;
    public GameObject Overworld;
    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnLevelLoaded;
    }


    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Invoke("LoadBattleScene", 5);
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    public void LoadBattleScene() 
    {
        SceneManager.LoadScene("BattleScene", LoadSceneMode.Additive);
        Overworld.SetActive(false);
    }

    public void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) { return; }
        BattleBackground = GameObject.FindWithTag("BattleBackground");
        Overworld = GameObject.FindWithTag("Overworld");
    }
}
