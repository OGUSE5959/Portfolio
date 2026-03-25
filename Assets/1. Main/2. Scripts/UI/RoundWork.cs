using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

public class RoundWork : MonoBehaviour
{
    public GameManager_1VS1 gm;
    [Space]
    [SerializeField] Text _timer;
    [SerializeField] Text _myWinCnt;
    [SerializeField] Text _enemyWinCnt;
    [SerializeField] Text _winCounts;
    [SerializeField] Image _banner;
    [SerializeField] Text _roundResult;
    [SerializeField] Text _roundCount;
    [Space]
    [SerializeField] Image _gameOverBG;
    [SerializeField] Text _matchResult;
    [SerializeField] Button _leaveMatch;

    RectTransform _bannerTr;
    RectTransform _gameOverTr;

    [Header("Observer Properties")]
    [Space]
    [SerializeField] Image _masterWinCountsBG;
    [SerializeField] Image _otherWinCountsBG;
    [Space]
    [SerializeField] RectTransform _masterBanner;
    [SerializeField] Text _masterLabel;
    [SerializeField] RectTransform _otherBanner;
    [SerializeField] Text _otherLabel;
    int _masterWinCount;
    int _otherWinCount;
    bool _isMatchEnded;
    [Space]
    public RectTransform masterNickname;
    public RectTransform otherNickname;

    float Width => Screen.width;

    public void SetTimer(string value) => _timer.text = value;
    public void SetWinCounts(int me, int enemy)
    {
        if (this.gm.IsObserver) return;
        // GameManager gm = GameManager.Instance;

        _myWinCnt.text = me.ToString();
        _enemyWinCnt.text = enemy.ToString();
    }
    void Ob_UpdateWinCounts(bool masterWin)
    {
        _masterWinCount += masterWin ? 1 : 0;
        _otherWinCount += masterWin ? 0 : 1;

        _myWinCnt.text = _masterWinCount.ToString();
        _enemyWinCnt.text = _otherWinCount.ToString();
    }
    string GetNickName(string userID)
    {
        foreach (var player in PhotonNetwork.PlayerList)
            if (player.UserId == userID)
                return player.NickName;
        return "";
    }
    public void SetDraw() => StartCoroutine(Coroutine_SetDraw());
    public void SetWinOrLose(bool win)
    {
        if (gm.IsObserver) return;  // 빅토리는 안하는데 얘만 하는 이유는 빅토리는 
        // 관전자가 마스터아 아니라는 전제 하예 true값으로 호출될 일이 없고
        // 또 RPC_SetDefeat(Other)는 받았을때 관전자면 리턴함
        StartCoroutine(Coroutine_SetLoseOrWin(win));
    }
    public void SetVictoryOrDefeat(bool victory, float waitTime = 0f) => StartCoroutine(Coroutine_SetVictoryOrDefeat(victory, waitTime));
    public void SetRoundCount(int round) => _roundCount.text =  "라운드 " + round.ToString();
    public void SetRoundCount(int round, int goal)
    {
        SetRoundCount(round);
        _roundCount.text += "  <color=#4a4a4a>((" + goal.ToString() +" 선승제</color>";
    }

    public void SetAsObserver()
    {
        _myWinCnt.color = Color.white;
        _enemyWinCnt.color= Color.black;
        _masterWinCountsBG.color = Color.black;
        _otherWinCountsBG.color = Color.white;
    }
    // public void Ob_SetDraw() { }
    public void Ob_SetWinOrLose(bool masterWin)
    {
        Ob_UpdateWinCounts(masterWin);
        if (_isMatchEnded) return;
        StartCoroutine(Coroutine_Ob_SetWinOrLose(masterWin));
    }
    public void Ob_SetVictoryOrDefeat(bool masterVictory)
    {
        _isMatchEnded = true;
        StartCoroutine(Coroutine_Ob_SetVictoryOrDefeat(masterVictory));
    }

    IEnumerator Coroutine_SetDraw()
    {
        _bannerTr.sizeDelta = new Vector2(0f, _bannerTr.sizeDelta.y);
        _bannerTr.DOSizeDelta(new Vector2(Width, _bannerTr.sizeDelta.y), 1.5f);
        _banner.color = Color.yellow;
        _roundResult.text = "<color=#000000>-무승부-</color>";
        _roundResult.gameObject.SetActive(true);
        _banner.gameObject.SetActive(true);
        yield return Utility.GetWaitForSeconds(2f);
        _banner.gameObject.SetActive(false);
        _roundResult.gameObject.SetActive(false);
    }
    IEnumerator Coroutine_SetLoseOrWin(bool win)
    {
        // Vector3 os = _banner.transform.localScale;
        _bannerTr.sizeDelta = new Vector2(0f, _bannerTr.sizeDelta.y);
        _bannerTr.DOSizeDelta(new Vector2(Width, _bannerTr.sizeDelta.y), 1f);
        _banner.color = win ? Color.blue : Color.red;

        _roundResult.text = win ? "승리!!" : "패배..";
        _roundResult.gameObject.SetActive(true);
        _banner.gameObject.SetActive(true);

        yield return Utility.GetWaitForSeconds(2f);
        _banner.gameObject.SetActive(false);
        _roundResult.gameObject.SetActive(false);
    }
    IEnumerator Coroutine_SetVictoryOrDefeat(bool victory, float waitTime)
    {
        Inventory.Instance.gameObject.SetActive(false);
        yield return Utility.GetWaitForSeconds(waitTime);

        _gameOverTr.sizeDelta = new Vector2(0f, _gameOverTr.sizeDelta.y);
        _gameOverTr.DOSizeDelta(new Vector2(Width, _gameOverTr.sizeDelta.y), 1.5f);
        _matchResult.text = victory ? "Victory!!" : "Defeat..";
        _matchResult.gameObject.SetActive(true);
        _gameOverBG.gameObject.SetActive(true);

        yield return Utility.GetWaitForSeconds(2f);
        _leaveMatch.gameObject.SetActive(true);
    }

    IEnumerator Coroutine_Ob_SetWinOrLose(bool masterWin)
    {
        Debug.Log("Coroutine_Ob_SetWinOrLose");

        float sizeY = _masterBanner.sizeDelta.y;
        float winnerWidth = Width * 3f / 5f;
        float loserWidth = Width * 2f / 5f;
        _masterBanner.DOKill();
        _otherBanner.DOKill();
        _masterLabel.fontSize = masterWin ? 75 : 50;
        _otherLabel.fontSize = masterWin ? 50 : 75;
        _masterLabel.text = GameManager_1VS1.Instance.GetNickNameNotMe(true, PhotonNetwork.LocalPlayer.UserId)
            + "님 라운드 " + (masterWin ? "승리!" : "패배..");
        _otherLabel.text = GameManager_1VS1.Instance.GetNickNameNotMe(false, PhotonNetwork.LocalPlayer.UserId)
            + "님 라운드 " + (!masterWin ? "승리!" : "패배..");        
        _masterBanner.sizeDelta = new Vector2(!masterWin ? winnerWidth : loserWidth, sizeY);
        _otherBanner.sizeDelta = new Vector2(masterWin ? winnerWidth : loserWidth, sizeY);
        _masterBanner.gameObject.SetActive(true);
        _otherBanner.gameObject.SetActive(true);

        _masterBanner.DOSizeDelta(new Vector2(masterWin ? winnerWidth : loserWidth, sizeY), 1f);
        _otherBanner.DOSizeDelta(new Vector2(!masterWin ? winnerWidth : loserWidth, sizeY), 1f);

        yield return Utility.GetWaitForSeconds(5f);
        _masterBanner.gameObject.SetActive(false);
        _otherBanner.gameObject.SetActive(false);
        // yield break;
    }
    IEnumerator Coroutine_Ob_SetVictoryOrDefeat(bool masterVictory)
    {
        Debug.Log("Coroutine_Ob_SetVictoryOrDefeat");
        masterNickname.gameObject.SetActive(false);
        otherNickname.gameObject.SetActive(false);

        float sizeY = _masterBanner.sizeDelta.y * 1.25f;
        if (masterVictory)
        {
            _masterBanner.DOKill();
            _masterLabel.fontSize = 90;
            _masterLabel.color = Color.cyan;
            _masterLabel.text = GameManager_1VS1.Instance.GetNickNameNotMe(true, PhotonNetwork.LocalPlayer.UserId)
           + "님 매치 " + "승리!";
            _masterBanner.sizeDelta = new Vector2(0f, sizeY);
            _masterBanner.gameObject.SetActive(true);
            _masterBanner.DOSizeDelta(new Vector2(Width, sizeY), 1f);
        }
        else
        {
            _otherBanner.DOKill();
            _otherLabel.fontSize = 90;
            _otherLabel.color = Color.cyan;
            _otherLabel.text = GameManager_1VS1.Instance.GetNickNameNotMe(true, PhotonNetwork.LocalPlayer.UserId)
           + "님 매치 " + "승리!";
            _otherBanner.sizeDelta = new Vector2(0f, sizeY);
            _otherBanner.gameObject.SetActive(true);
            _otherBanner.DOSizeDelta(new Vector2(Width, sizeY), 1f);
        }
        yield break;
    }

    private void Awake()
    {
        _bannerTr = _banner.GetComponent<RectTransform>();
        _gameOverTr = _banner.GetComponent<RectTransform>();
    }
    void Start()
    {
        _leaveMatch.onClick.AddListener(() => GameManager.Instance.GoToTitle());
        /*{
            PhotonNetwork.LeaveRoom();
            Destroy(RoomManager.Instance.gameObject);
        });*/
    }
#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Ob_SetVictoryOrDefeat(Input.GetKey(KeyCode.LeftAlt));
    }
#endif
}
