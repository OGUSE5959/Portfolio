using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

public class DeathMatchUI : MonoBehaviour
{
    [Space]
    [SerializeField] Text _timer;
    [SerializeField] Text _myKillCnt;
    [Space]
    [SerializeField] Image _gameOverBG;
    [SerializeField] GameObject _congraturation;
    [Space]
    [SerializeField] Text _winnerName;
    [SerializeField] Text _matchResult;
    [SerializeField] Button _leaveMatch;
    [SerializeField] Text _leaveSoon;
    [Space]
    [SerializeField] Transform _finalBanner;
    [SerializeField] Text _finalDesc;
    string _winner;

    RectTransform _gameOverTr;
    RectTransform _congratureTr;

    float Width => Screen.width;

    public void SetTimer(float time) => _timer.text = time.ToString("0.0");
    public void SetTimer(string time) => _timer.text = time;
    public void SetKill(int kill, int goal)
    {
        _myKillCnt.text = kill + "/" + goal;
    }

    public void SetWinenrName(string winenrName)
    {
        _winner = winenrName;
        _winnerName.text = "Found Winner!";
        _winnerName.gameObject.SetActive(true);
        _winnerName.transform.localScale = Vector3.one * 6f;
        _winnerName.transform.DOScale(Vector3.one, 0.5f);
    }
    public void SetGameOver() { }
    public void SetFinalBanner() => StartCoroutine(Coroutine_SetFinalTime());
    public void SetVictoryOrDefeat(bool victory, float waitTime = 0f) 
        => StartCoroutine(Coroutine_SetVictoryOrDefeat(victory, waitTime));

    // IEnumerator Coroutine_SetGameOver() { }
    IEnumerator Coroutine_SetVictoryOrDefeat(bool victory, float waitTime)
    {
        _winnerName.fontSize /= 2;
        string hexa = victory ? "yellow" : "red"; //Utility.ToRGBHex(victory ? Color.yellow : Color.red);
        _winnerName.text = "Winner's NickName\n" + "<color=" + hexa + ">" + _winner + "</color>";
        Inventory.Instance.gameObject.SetActive(false);
        yield return Utility.GetWaitForSeconds(waitTime);
        _gameOverTr.sizeDelta = new Vector2(0f, _gameOverTr.sizeDelta.y);
        _congraturation.SetActive(victory);

        _gameOverTr.DOSizeDelta(new Vector2(Width, _gameOverTr.sizeDelta.y), 1.5f);
        _congratureTr.DOSizeDelta(new Vector2(Width, _congratureTr.sizeDelta.y), 1.5f);
        _gameOverBG.color = victory ? Color.blue : Color.red;
        _gameOverBG.gameObject.SetActive(true);
        _matchResult.text = victory ? "승리!" : "패배..";
        _matchResult.gameObject.SetActive(true);

        yield return Utility.GetWaitForSeconds(2f);
        // _leaveMatch.gameObject.SetActive(true);
        StartCoroutine(Coroutine_SetLeaveTimer());
        Cursor.lockState = CursorLockMode.None;
    }
    IEnumerator Coroutine_SetFinalTime()
    {
        _finalDesc.color = new Color(1f, 1f, 1f, 0f);
        _finalBanner.localScale = new Vector3(0f, 1f, 1f);
        _finalBanner.gameObject.SetActive(true);
        _finalBanner.DOScale(Vector3.one, 0.5f);
        yield return Utility.GetWaitForSeconds(0.75f);
        while(true)
        {
            Color newColor = _finalDesc.color;
            newColor.a += Time.deltaTime / 1.5f;
            if(newColor.a >= 1f)
            {
                _finalDesc.color = Color.white;
                break;
            }
            _finalDesc.color = newColor;
            yield return null;
        }
        _finalBanner.DOScale(Vector3.one, 0.5f);
        yield return Utility.GetWaitForSeconds(0.5f);
        _finalBanner.gameObject.SetActive(false);
    }
    IEnumerator Coroutine_SetLeaveTimer(float duration = 5)
    {
        _leaveSoon.gameObject.SetActive(true);
        float timer = 0f;
        while (true)
        {
            yield return null;
            timer += Time.deltaTime;
            if(timer >= duration)
            {
                _leaveSoon.text = "퇴장까지 : 0";
                if(PhotonNetwork.IsMasterClient)
                {
                    var gds = GameManager_DeathMatch_Solo.Instance as GameManager_DeathMatch_Solo;
                    gds.OnMasterLeave();
                }
                else
                {
                    yield return Utility.GetWaitForSeconds(0.5f);
                    _leaveSoon.text = "호스트가 자동 퇴장하길 기다리는 중..";
                }                
            }
            _leaveSoon.text = "퇴장까지 : " + Mathf.CeilToInt(duration - timer).ToString();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _gameOverTr = _gameOverBG.GetComponent<RectTransform>();
        _congratureTr = _congraturation.GetComponent<RectTransform>();
        _leaveMatch.onClick.AddListener(GameManager_DeathMatch_Solo.Instance.GoToTitle);
    }
}
