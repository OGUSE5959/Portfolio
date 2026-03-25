using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    RoomMenu _master;
    Player _player;
    public Player Player => _player;
    [SerializeField] Text _nickName;
    [SerializeField] Image _hostIcon;
    Notificator Notice => Notificator.Instance;

    public void Initialize(RoomMenu master) => _master = master;
    public void SetUp(Player player, Transform parent)
    {
        if (player == null)
            Debug.LogAssertion("Player is Null!!");
        _player = player;
        _nickName.text = player.NickName;
        _nickName.color = player == PhotonNetwork.LocalPlayer ? /*Utility.HexColor("#BFA400")*/Color.yellow : Color.white;
        _hostIcon.gameObject.SetActive(player == PhotonNetwork.MasterClient);

        transform.SetParent(parent);
    }
    void PlayerLeftRoomCallback(Player player)
    {
        if (player == _player)
        {
            // _master.InsertItem(this);
            return;
        }
        UpdateIfHost(true);
    }
    void UpdateIfHost(bool log = false)
    {
        bool isHost = _player == PhotonNetwork.MasterClient;
        if (!_hostIcon.gameObject.activeSelf && isHost/* && !_hostIcon.IsDestroyed()*/)
        {
            _hostIcon.gameObject.SetActive(true);
            if(log) Notice.Notice("호스트가 되었습니다!", Color.yellow);
        }
        else if (/*_hostIcon.gameObject.activeSelf && */!isHost)
        {
            _hostIcon.gameObject.SetActive(false);
            if(log && _player == PhotonNetwork.LocalPlayer) Notice.Notice(PhotonNetwork.MasterClient.NickName + "님이 호스트가 되었습니다!", Color.white);
        }
        Debug.Log("Master Client : " + PhotonNetwork.MasterClient.NickName);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerLeftRoomCallback(otherPlayer);
    }
    //public override void OnLeftRoom()
    //{
    //    Destroy(gameObject);
    //}
    // Start is called before the first frame update
    protected new void OnEnable()
    {
        if (PhotonNetwork.InRoom) UpdateIfHost();
    }
    void Start()
    {
        Launcher.Instance.AddPlayerLeftRoomCallback((Player player) =>
            PlayerLeftRoomCallback(player)
        );
    }
    private void OnDestroy()
    {
        Debug.Log("PlayerListItemDestoried");
    }
}
