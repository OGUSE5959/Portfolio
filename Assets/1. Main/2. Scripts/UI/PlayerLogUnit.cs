using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

[System.Serializable]
public struct KDA
{
    public KDA(int kill, int death, int assist)
    {
        this.kill = kill;
        this.death = death;
        this.assist = assist;
    }

    public int kill;
    public int death;
    public int assist;
}

public class PlayerLogUnit : MonoBehaviour
{
    StringBuilder _sb = new StringBuilder();
    [SerializeField] Text _nickName;
    [SerializeField] Text _kdaLabel;
    [Space] 
    [SerializeField] Text _kill;
    [SerializeField] Text _death;
    [SerializeField] Text _assist;
    KDA _kda;

    public void Initialize(string nickName, int kill, int death, int assist)
    {
        _nickName.color = nickName.Equals(PhotonNetwork.LocalPlayer.NickName) ? Color.yellow : Color.black;
        _nickName.text = nickName;
        _sb.Clear();
        // _sb.Append("KDA : " + kill + "/" + death + "/" + assist);
        // _kdaLabel.text = _sb.ToString();
        _kill.text = kill.ToString();
        _death.text = death.ToString();
        _assist.text = assist.ToString();
    }
    public void Initialize(string nickName, KDA kda) => Initialize(nickName, kda.kill, kda.death, kda.assist);

    public void Initialize(Player player)
    {
        _nickName.color = player.UserId.Equals(PhotonNetwork.LocalPlayer.UserId) ? Color.yellow : Color.white;
        _nickName.text = player.NickName;

        var cp = player.CustomProperties;
        // if (!cp.ContainsKey("Kill") || !cp.ContainsKey("Death")) continue;
        KDA kda = new KDA(0, 0, 0);
        if (cp.ContainsKey("Kill")) kda.kill = (int)cp["Kill"];
        if (cp.ContainsKey("Death")) kda.death = (int)cp["Death"];

        _kill.text = kda.kill.ToString();
        _death.text = kda.death.ToString();
        _assist.text = kda.assist.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
