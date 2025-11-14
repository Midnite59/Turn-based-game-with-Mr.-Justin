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

    private RenderTexture transRTexture;
    public RawImage transRTImage;
    public GameObject transRTCanvas;
    public Camera transRTCam;
    bool loaded = false;
    public AnimationCurve transCurve;
    public float transitionSeconds = 1;

    public Camera hiddenCam;
    private RenderTexture hiddenRTexture;


    private void Awake()
    {
        Application.targetFrameRate = 60;
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
        transRTexture = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 16);
        transRTexture.Create();
        transRTexture.Release();
        transRTCanvas.SetActive(false);
        hiddenRTexture = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 16);
        hiddenRTexture.Create();
        hiddenRTexture.Release();
        hiddenCam.targetTexture = hiddenRTexture;
        transRTImage.material.SetTexture("_NotMainTex", hiddenRTexture);
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Time.deltaTime);
    }


    public void LoadBattleScene() 
    {
        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        SceneManager.sceneLoaded += (scene, mode) => { Overworld.SetActive(false); loaded = true; };
    }

    public void UnloadBattleScene()
    {
        SceneManager.UnloadSceneAsync("BattleScene");
        SceneManager.sceneUnloaded += (scene) => { Overworld.SetActive(true); loaded = true; };
    }

    public void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) { return; }
        BattleBackground = GameObject.FindWithTag("BattleBackground");
        Overworld = GameObject.FindWithTag("Overworld");
    }
    public void StartBattle(EncountersOhNo encounter)
    {
        StartCoroutine(BattleRoutine(encounter, false));
    }
    public void EndBattle(EncountersOhNo encounter)
    {
        StartCoroutine(BattleRoutine(encounter, true));
    }
    private IEnumerator BattleRoutine(EncountersOhNo encounter, bool bEnd = false) 
    {
        loaded = false;
        transRTCam.gameObject.SetActive(true);
        transRTCam.CopyFrom(Camera.main);
        transRTCam.targetTexture = transRTexture;
        yield return null;
        transRTCam.transform.position = Camera.main.transform.position;
        transRTCam.transform.rotation = Camera.main.transform.rotation;
        //transRTCam.fieldOfView = Camera.main.fieldOfView;
        yield return new WaitForEndOfFrame();
        transRTImage.texture = transRTexture;
        transRTImage.material.SetColor("_Color", !bEnd ? Color.black : Color.white);
        transRTCanvas.SetActive(true);
        transRTCam.gameObject.SetActive(false);
        if (!bEnd)
        {
            LoadBattleScene();
        }
        else 
        {
            UnloadBattleScene();
        }
            float t = 0;
        while (t <= 1)
        {
            transRTImage.material.SetFloat("_ColorProgress", transCurve.Evaluate(t));
            t += Time.deltaTime / transitionSeconds;
            yield return null;
        }
        transRTImage.material.SetFloat("_ColorProgress", 1);
        if (!bEnd)
        {
            gameloop.CreateBattle(team.allies, encounter.enemies, team.allyhealths);
            gameloop.battleOverWin += () => { EndBattle(encounter); };
            encounter.gameObject.SetActive(false);
        }
        t = 0;
        while (!loaded) 
        {
            yield return null;
        }
        yield return new WaitForSeconds(bEnd ? 3.0f : 0.5f);
        while (t <= 1)
        {
            transRTImage.material.SetFloat("_AlphaProgress", transCurve.Evaluate(t));
            t += Time.deltaTime / transitionSeconds;
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        transRTImage.material.SetFloat("_ColorProgress", 0);
        transRTImage.material.SetFloat("_AlphaProgress", 0);
        transRTCanvas.SetActive(false);
    }
}
