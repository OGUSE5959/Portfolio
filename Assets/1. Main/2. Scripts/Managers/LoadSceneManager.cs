using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public enum SceneType
{
    None = -1,

    Title,
    Training,

    Round_1vs1,
    Round_Team,

    DeathMatch_Solo,
    DeathMatch_Team,

    Max
}

public class LoadSceneManager : SingletonDontDestory<LoadSceneManager>
{
    [SerializeField] Image _background;
    [SerializeField] Slider _progressBar;
    [SerializeField] Text _progressLabel;
    [Space]
    [SerializeField] SceneType _currentScene;
    public SceneType CurrentScene { get { return _currentScene; } }
    [SerializeField] SceneType _loadScene = SceneType.None;
    AsyncOperation _loadSceneState;
    List<Action> _onSceneLoadedCallbacks_Eternal = new List<Action>();
    Queue<Action> _onSceneLoadedCallbacks_Once = new Queue<Action>();

    void ShowUI()
    {
        _background.gameObject.SetActive(true);
        _progressBar.gameObject.SetActive(true);
        _progressLabel.gameObject.SetActive(true);
    }
    void HideUI()
    {
        _background.gameObject.SetActive(false);
        _progressBar.gameObject.SetActive(false);
        _progressLabel.gameObject.SetActive(false);
    }

    public void AddCallbacks_Async(Action action) =>  _onSceneLoadedCallbacks_Once.Enqueue(action);
    public void AddCallbacks(UnityAction<Scene, LoadSceneMode> action) => SceneManager.sceneLoaded += action;
    public void LoadScene(SceneType sceneType)
    {
        SceneManager.LoadScene((int)sceneType);
        _currentScene = sceneType;
    }
    public void LoadSceneAsync(SceneType sceneType)
    {
        // Debug.Log(sceneType);
        _loadScene = sceneType;
        ShowUI();
        _loadSceneState = SceneManager.LoadSceneAsync((int)sceneType);       
    }
    public void ReloadScene(bool async)
    {
        if (async)
            LoadSceneAsync(_currentScene);
        else
            LoadScene(_currentScene);
    }
    void Callbacks()
    {
        /*foreach (var action in _onSceneLoadedCallbacks_Eternal)
            action();*/
        while (_onSceneLoadedCallbacks_Once.Count > 0)
        {
            Action action = _onSceneLoadedCallbacks_Once.Dequeue();
            action();
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _currentScene = (SceneType)SceneManager.GetActiveScene().buildIndex;
    }
    void Update()
    {
        if(_loadScene != SceneType.None)
        {
            float progress = _loadSceneState.progress;
            _progressBar.value = progress;
            _progressLabel.text = (progress * 100f).ToString("0.00") + "%";
            if(progress >= 1f)
            {
                _progressBar.value = 1f;
                _progressLabel.text = "100%";
                Invoke("HideUI", 0.4f);

                _currentScene = _loadScene;
                _loadScene = SceneType.None;
                // HideUI();

                Callbacks();
            }
        }
    }
}
