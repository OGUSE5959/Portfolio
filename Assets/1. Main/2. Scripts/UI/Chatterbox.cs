using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ChatterBox : MonoBehaviourPunCallbacks
{
    PhotonView _pv;

    const string HexColor_Me = "#BFA400";
    const string HexColor_Other = "FFFFFF";
    const string HexColor_Announce = "#00E0FF";

    public GameObject _content;
    public GameObject Content => _content;
    public InputField _inputField;

    [SerializeField] Text _contentText;
    string _strUserName;
    List<string> _badWords = new List<string>()
    {
    "¾¾¹ß", "½Ã¹ß", "¤¶¤²", "¤µ¤²",
    "º´½Å", "ºéµü", "¤´",
    "»õ³¢", "¤µ¤¢",
    "°³»õ³¢", "°³¼¼³¢", "°³»ö±â",
    "´Ï¾Ö¹Ì", "´Ï¾Öºñ", "´À±Ý¸¶", "¤¤¤·¤±", "¤¤¤¡¤±", "¤¤¤·¤²", "¤¤¤¡¤²",
    "´Ï¾ö¸¶", "³×¾ö¸¶", "´Ï¾Æºü", "³×¾Æºü", "´À°Ë¸¶", "´À°³ºñ", "¿îÁö", "³ë¹«Çö", "³ëÂ¯",
    "¼½½º", "›®½º", "»ö½º",
    "¤µ¤µ",
    "Á¿", "¤¸¤¸", "¤¸°°", "Á¿°°", "Á¸³ª", "¤¸¤¤", "¤¸³ª",
    "ºüÅ¥", "fuck", "fuxk", "f*ck", "fcuk",
    "asshole", "bitch", "shit",
    "¾Ö¹Ì", "¾Öºñ",
    "Á×¾î", "µÚÁ®", "µðÁ®", "Á½±î",
    "¿°º´", "¿¼º´", "¿°º´ÇÒ", "¿°”·",
    "Áö¶ö", "¤¸¤©", "¤¸¤¸¤©",
    "°³³ë´ä", "³ë´ä", "¸ÛÃ»ÀÌ",
    "¹ÌÄ£³ð", "¹ÌÄ£³â", "¤±¤º", "¤±¤º¤¤", "¤±¤º³ð",
    "¸ÁÇÒ", "ºô¾î¸ÔÀ»", "²¨Á®", "²¨Á´", "²¨Áö¼¼¿ä"
    };

    void Start()
    {
        // PhotonNetwork.ConnectUsingSettings();
        /*if (!_contentText)
            _contentText = _content.transform.GetChild(0).GetComponent<Text>();*/
        _pv = GetComponent<PhotonView>();
        Launcher.Instance.AddLeftRoomCallback(ResetChat);
        Launcher.Instance.AddPlayerLeftRoomCallback((player) =>
        {
            AddChatMessage(player.NickName + " ´ÔÀÌ ¹æÀ» ³ª°¬½À´Ï´Ù", Utility.HexColor(HexColor_Announce));
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && _inputField.isFocused == false)
        {
            _inputField.ActivateInputField();
            SyncedChat();
            /*AddChatMessage(m_inputField.text);
            m_inputField.text = "";*/
        }
    }
    /*public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 5;

        int nRandomKey = Random.Range(0, 100);

        m_strUserName = "user" + nRandomKey;

        PhotonNetwork.LocalPlayer.NickName = m_strUserName;
        PhotonNetwork.JoinOrCreateRoom("Room1", options, null);
    }*/

    public override void OnJoinedRoom()
    {
        // AddChatMessage("connect user : " + PhotonNetwork.LocalPlayer.NickName);
        // SyncedAnnounce(PhotonNetwork.NickName + " ´ÔÀÌ µé¾î¿À¼Ì½À´Ï´Ù");
    }

    public void OnEndEditEvent()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            string strMessage = _strUserName + " : " + _inputField.text;

            _pv.RPC("RPC_Chat", RpcTarget.All, strMessage);
            _inputField.text = "";
        }
    }

    Text CreateText(string str)
    {
        Text goText = Instantiate<Text>(_contentText, _content.transform);
        goText.text = Filter(str);
        _content.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        return goText;
    }
    void AddChatMessage(string nickName, string message)
    {
        bool isMe = nickName == PhotonNetwork.LocalPlayer.NickName;
        string hexColor = isMe ? HexColor_Me : HexColor_Other;
        CreateText("<color=" + hexColor + ">[" + nickName + "]: " + message + "</color>").gameObject.SetActive(true);
    }
    void AddChatMessage(string message)
    {
        CreateText(message).gameObject.SetActive(true);
    }
    void AddChatMessage(string message, Color color)
    {
        string hexColor = Utility.ToRGBHex(color);
        CreateText("<color=#" + hexColor + ">" + message + "</color>").gameObject.SetActive(true);
    }

    void SyncedChat()
    {
        string message = _inputField.text;
        _pv.RPC("RPC_Chat", RpcTarget.All, PhotonNetwork.CurrentRoom.Name, PhotonNetwork.LocalPlayer.NickName, message);
        _inputField.text = "";
    }
    public void SyncedAnnounce(string announce, RpcTarget rpcTarget = RpcTarget.All)
    {
        if (!_pv) _pv = GetComponent<PhotonView>();
        _pv.RPC("RPC_Announce", rpcTarget, PhotonNetwork.CurrentRoom.Name, announce);
    }
    public void ResetChat()
    {
        Text[] ts = Content.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < ts.Length; i++) 
            Destroy(ts[i].gameObject);
    }
    string Filter(string sentens)
    {
        foreach(string bad in _badWords)
            sentens = sentens.Replace(bad, new string('*', bad.Length));
        return sentens;
    }
    [PunRPC] void RPC_Chat(string roomName, string nickName, string message)
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.CurrentRoom.Name.Equals(roomName)
            || message.Equals("")) return;
        AddChatMessage(nickName, message);
    }
    [PunRPC] void RPC_Announce(string roomName, string message)
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.CurrentRoom.Name.Equals(roomName)
            || message.Equals("")) return;
        AddChatMessage(message, Utility.HexColor(HexColor_Announce));
    }
}