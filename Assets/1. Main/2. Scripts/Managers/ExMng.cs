using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExMng : SingletonMonoBehaviour<ExMng>
{
    [SerializeField] PlayerController _playerPrefab;
    PlayerController _player;
    public PlayerController Player => _player;

    // Start is called before the first frame update
    protected override void OnStart()
    {
        PhotonNetwork.OfflineMode = true;
        _player =Instantiate<PlayerController>(_playerPrefab, Vector3.zero, Utility.QI);// FindAnyObjectByType<PlayerController>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
