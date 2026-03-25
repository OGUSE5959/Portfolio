using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    static PlayerManager _instance;
    public static PlayerManager Instance => _instance;

    PhotonView _pv;
    public PhotonView PV => _pv;


    Dictionary<string, PlayerController> _playerTable = new Dictionary<string, PlayerController>();
    public Dictionary<string, PlayerController> PlayerTable => _playerTable;
    [SerializeField] PlayerController _myPlayer;
    public PlayerController MyPlayer => _myPlayer;
    /*[SerializeField] PlayerController _enemy;
    public PlayerController Enemy => _enemy;*/

    void CreateController()
    {
        _myPlayer = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), Vector3.zero, Quaternion.identity).GetComponent<PlayerController>();
        _myPlayer.SetPM(this);
        // Debug.Log(_myPlayer.PV.ViewID);
        // Debug.Log(_myPlayer.name);       
        if (GameManager.Instance)
            GameManager.Instance.RegistPvInst(_myPlayer.PV);
    }

    private void Awake()
    {
        if(_instance == null)
            _instance = this;

        _pv = GetComponent<PhotonView>();
        // DebugLog(0);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (_pv.IsMine)
        {
            CreateController();
            
        }
        /*else 
            GameManager.Instance.SetEnemy(MyPlayer);*/    // PV가Mine이 아닌 쪽은 이미 인스턴스 뺏김!!
        /* if (_pv.IsMine)
             _pv.RPC("RPC_SetEnemy", RpcTarget.Others, _myPlayer.PV.ViewID);
         var players = FindObjectsByType<PlayerController>(0);
         Debug.Log(players.Length +", " + _pv.IsMine, gameObject);*/
        /*for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            _playerTable.Add(player.PV.Owner.UserId, players[i]);
            if (player.PV.ViewID != _myPlayer.PV.ViewID)
                _enemy = player;
        }*/
    }

    // Update is called once per frame
    /*void Update()
    {
        var players = FindObjectsByType<PlayerController>(0);
        Debug.Log(players.Length);
    }*/
}
