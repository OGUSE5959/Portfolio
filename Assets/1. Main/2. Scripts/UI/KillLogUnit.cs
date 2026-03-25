using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class KillLogUnit : MonoBehaviour
{
    // KillLog _master;
    [SerializeField] Image _bg;
    [SerializeField] Text _kill;
    [SerializeField] Text _vict;
    [SerializeField] Image _weapon;

    public float Height => _bg.rectTransform.sizeDelta.y;

    public void Initialize()
    {
        _kill.text = _vict.text = null;
        _weapon.color = Color.clear;
    }
    public void SetUp(string killer, string victim, Sprite icon = null)
    {
        // bool isLocal = PlayerUI.Instance.Master.PV.IsMine; // 쓸때가있나..
        _kill.text = killer;
        _vict.text = victim;
        _kill.color = killer.Equals(PhotonNetwork.LocalPlayer.NickName) ? Color.yellow : Color.red;
        _vict.color = victim.Equals(PhotonNetwork.LocalPlayer.NickName) ? Color.yellow : Color.white;
        _weapon.sprite = icon;
        _weapon.color = icon == null ? Color.clear : Color.white;
    }
    public void SetUp(IAttackable attacker, IDamagable hurter, Sprite icon = null)
        =>SetUp(attacker.PV.ViewID, hurter.PV.ViewID, icon);
    public void SetUp(int attacker, int hurter, Sprite icon = null)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("No GM For Making KillLogUnit");
            return;
        }
        GameManager gm = GameManager.Instance;
        string killer = gm.GetNickName(attacker);
        string victim = gm.GetNickName(hurter);
        _kill.text = killer;
        _vict.text = victim;
        bool isAttackerMine = PhotonNetwork.LocalPlayer.UserId == gm.ViewAndPlayerTable[attacker];
        bool isVictimMine = PhotonNetwork.LocalPlayer.UserId == gm.ViewAndPlayerTable[hurter];
        _kill.color = isAttackerMine ? Color.yellow : Color.red;
        _vict.color = isVictimMine ? Color.yellow : Color.white;
    }
    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/
}
