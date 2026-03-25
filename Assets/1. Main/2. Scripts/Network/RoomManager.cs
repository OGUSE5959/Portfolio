using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using PM = PlayerManager;
using GM = GameManager;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    private static RoomManager _instance;
    List<RoomInfo> _rooms = new List<RoomInfo>();
    public bool isObserver;

    public static RoomManager Instance => _instance;
    public List<RoomInfo> Rooms => _rooms;
    float _timeLimit = 90f;
    int _goatPoints = 3;

    void SetRoomList(List<RoomInfo> rooms) => _rooms = rooms;
    public void SetProperties(float timeLimit, int goatPoints)
    {
        _timeLimit = timeLimit;
        _goatPoints = goatPoints;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Debug.Log("RM_OnSceneLoaded");
        GM gm = GM.Instance;
        int buildIdx = scene.buildIndex;
        SceneType sceneType = (SceneType)buildIdx;
        if(isObserver)
        {
            Debug.Log("Observer");
        }
        else
        {
            if (sceneType == SceneType.Training
            || sceneType == SceneType.Round_1vs1
            || sceneType == SceneType.DeathMatch_Solo)
            {
                if (sceneType == SceneType.Round_1vs1)
                    PhotonNetwork.AutomaticallySyncScene = true;
                else PhotonNetwork.AutomaticallySyncScene = true;
                GameObject pm = PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
                if (gm) gm.RegistPvInst(pm.GetComponent<PhotonView>());
            }
        }
        /*else if (scene.buildIndex == 2)
        {
            GameObject pm = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
            if (gm) gm._instPvTracker.Add(pm);
        }*/
        if (PhotonNetwork.IsMasterClient && gm)
            gm.SetProperties(_timeLimit, _goatPoints);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public override void OnDisable()
    {
        base.OnDisable();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        /*if (SceneManager.GetActiveScene().buildIndex != 0)
            LoadSceneManager.Instance.LoadSceneAsync(SceneType.Title);*/
        /*if (PhotonNetwork.IsConnectedAndReady)
            PhotonNetwork.LeaveRoom();*/
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("RoomManagerDestoried " + _rooms.Count);
    }
    private void Awake()
    {
        if (!_instance) _instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        // Launcher.Instance.AddRoomListUpdateCallback(SetRoomList);
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
