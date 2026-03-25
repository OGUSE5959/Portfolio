using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

public class GameManager_RoundTeam : GameManager
{
    [Space]
    [SerializeField] RoundWork _framework;
    int _playerCount;
    int _conpletedClient;
    PlayerController[] _playerCtrls;

    void SpawnByTeam()
    {
        List<PlayerController> plys_A = new List<PlayerController>();
        List<PlayerController> plys_B = new List<PlayerController>();
        foreach (PlayerController pc in _playerCtrls)
        {
            string team = GetTeam(pc);
            if (team.Equals("A")) plys_A.Add(pc);
            else if (team.Equals("B")) plys_B.Add(pc);
        }
        if(plys_A.Count > 0)
        {
            plys_A = Utility.Shuffle(plys_A);
            for (int i = 0; i < plys_A.Count; i++)
                SpawnPlayer(plys_A[i].PV.ViewID, _spawnPoints[i]);
        }
        if (plys_B.Count > 0)
        {
            plys_B = Utility.Shuffle(plys_B);
            for (int i = 0; i < plys_B.Count; i++)
                SpawnPlayer(plys_B[i].PV.ViewID, _spawnPoints[5 + i]);
        }
    }
    string GetTeam(PlayerController ctrl)
    {
        var playerList = PhotonNetwork.PlayerList;
        string userID = _viewAndPlayerTable[ctrl.PV.ViewID];
        foreach (var player in playerList)
            if (player.UserId.Equals(userID) && player.CustomProperties.TryGetValue("Team", out object team))
                return team.ToString();
            else{ Debug.LogWarning("No Team Info in This Player's properties"); return null; }
        return null;
    }
    public override void AddPlayerList(string playerID, int viewID) // PV가Mine일 때만
    {
        base.AddPlayerList(playerID, viewID);   // 이 인자값들은 모든 클라의 GM에게 공유될 ID
        if (++_playerCount >= PhotonNetwork.PlayerList.Length)
            if (IsMasterClient) RPC_TryStartGame();
            else _pv.RPC("RPC_TryStartGame", RpcTarget.MasterClient);
        Debug.Log("Call AddPlayerList cnt : " + _playerCount);
    }
    public override void SetAsOther(PlayerController clone)
    {
        base.SetAsOther(clone);

    }
    public override void SetMy(PlayerController player)
    {
        base.SetMy(player);

    }

    [PunRPC] void RPC_TryStartGame()
    {
        // if (!MyPlayer) return;
        if (PhotonNetwork.PlayerList.Length == 1)
            MyPlayer.Respawn();
        else if (++_conpletedClient == PhotonNetwork.PlayerList.Length)
        {
            _playerCtrls = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            for (int i = 0; i < _playerCtrls.Length; i++)
                SpawnPlayer(_playerCtrls[i].PV.ViewID, _spawnPoints[i]);
            _frameworkBG.SetActive(false);
            SpawnByTeam();
            // _pv.RPC("RPC_InitKDA", RpcTarget.All);

            Debug.Log("StartGame");
            SetTimer();
        }
        // _framework.SetWinCounts(0, 0);
        _frameworkBG.SetActive(false);
    }

    protected override void OnStart()
    {
        base.OnStart();
    }
}
