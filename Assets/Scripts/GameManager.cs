using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject BattleBackground;
    public GameObject Overworld;

    public TeamSetup team;

    public GameLoop gameloop;

    private RenderTexture rTexture;

    public RawImage rtImage;

    public GameObject rtCanvas;

    public Camera rtCam;

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
        //Invoke("LoadBattleScene", 5);
        rTexture = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 16);
        rTexture.Create();
        rTexture.Release();
        rtCanvas.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {

    }


    public void LoadBattleScene() 
    {
        SceneManager.LoadScene("BattleScene", LoadSceneMode.Additive);
        SceneManager.sceneLoaded += (scene, mode) => { Overworld.SetActive(false); };
    }

    public void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) { return; }
        BattleBackground = GameObject.FindWithTag("BattleBackground");
        Overworld = GameObject.FindWithTag("Overworld");
    }
    public void StartBattle(EncountersOhNo encounter)
    {
        StartCoroutine(BattleRoutine(encounter));
    }
    private IEnumerator BattleRoutine(EncountersOhNo encounter) 
    {
        rtCam.gameObject.SetActive(true);
        rtCam.targetTexture = rTexture;
        yield return null;
        rtCam.transform.position = Camera.main.transform.position;
        rtCam.transform.rotation = Camera.main.transform.rotation;
        yield return new WaitForEndOfFrame();
        rtImage.texture = rTexture;
        rtCanvas.SetActive(true);
        rtCam.gameObject.SetActive(false);
        LoadBattleScene();
        yield return new WaitForSeconds(1);
        gameloop.CreateBattle(team.allies, encounter.enemies, team.allyhealths);
        yield return new WaitForEndOfFrame();
        rtCanvas.SetActive(false);
    }
}
