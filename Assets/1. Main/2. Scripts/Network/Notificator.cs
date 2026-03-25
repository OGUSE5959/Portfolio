using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

public class Notificator : SingletonDontDestory<Notificator>
{
    PhotonView _pv;
    [SerializeField] Text _noticeUnit;
    GameObjectPool<Text> _noticePool = new GameObjectPool<Text>();
    [SerializeField] Transform _gridTr;
    Queue<Text> _unitList = new Queue<Text>();
    [Space]
    [SerializeField] Text _tipTxt;
    [SerializeField] AnimationCurve _tipEasing;
    Vector3 _tipStartPos;
    Coroutine _coroutine_ToolTip;

    [PunRPC] public void Notice(string messege)
    {
        Text unit = null;
        _unitList.Enqueue(unit = _noticePool.Get());

        unit.text = messege;
        // Color newColor = unit.color; newColor.a = 1f;
        unit.color = Color.red;

        // unit.transform.SetParent(_gridTr);
        //unit.transform.position = Camera.main.WorldToScreenPoint(Vector3.zero);
        unit.rectTransform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        /*foreach (Text t in _unitList)
            t.rectTransform.DOMoveY(t.rectTransform.position.y + 40, 0.5f);*/

        unit.transform.SetAsFirstSibling();
        unit.gameObject.SetActive(true);
        StartCoroutine(Coroutine_DissapearUnit(unit, 5f));
    }
    public void Notice(string messege, Color color)
    {
        Text unit = null;
        _unitList.Enqueue(unit = _noticePool.Get());

        unit.text = messege;        
        unit.color = color;

        // unit.transform.SetParent(_gridTr);
        //unit.transform.position = Camera.main.WorldToScreenPoint(Vector3.zero);
        unit.rectTransform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        /*foreach (Text t in _unitList)
            t.rectTransform.DOMoveY(t.rectTransform.position.y + 40, 0.5f);*/

        unit.transform.SetAsFirstSibling();
        unit.gameObject.SetActive(true);
        StartCoroutine(Coroutine_DissapearUnit(unit, 5f));
    }
    public void ToolTip(string messege)
    {
        if(_coroutine_ToolTip != null) StopCoroutine(_coroutine_ToolTip);
        _coroutine_ToolTip = StartCoroutine(Coroutine_ShowTip(messege));
    }

    public void SyncedNotic(string messege, RpcTarget rpcTarget)
        => _pv.RPC("Notice", rpcTarget, messege);

    IEnumerator Coroutine_DissapearUnit(Text unit, float duration)
    {
        float timer = 0f;
        while(true)
        {
            float deltaTime = Time.deltaTime;
            timer += deltaTime;
            if(timer >= duration)
            {
                unit.transform.SetAsLastSibling();
                unit.gameObject.SetActive(false);
                // unit.transform.SetParent(transform);
                _noticePool.Set(unit);
                yield break;
            }
            
            if(timer >= duration * 4f / 5f)
            {
                Color newColor = unit.color; newColor.a = -1f + duration / timer;
                unit.color = newColor;
            }
            yield return null;
        }
    }
    IEnumerator Coroutine_ShowTip(string messege)
    {
        _tipTxt.transform.position = _tipStartPos;
        _tipTxt.text = messege;
        Color newColor = _tipTxt.color;
        newColor.a = 1f;
        _tipTxt.color = newColor;
        _tipTxt.gameObject.SetActive(true);
        // yield return Utility.GetWaitForSeconds(1f);
        float timer = 0f;
        while(true)
        {
            var y = _tipEasing.Evaluate(timer);
            timer += Time.deltaTime * 2f;
            _tipTxt.transform.position += Vector3.up * y;
            if (timer >= 3f)
            {
                _tipTxt.gameObject.SetActive(false);
                _coroutine_ToolTip = null;
                yield break;
            }
            /*newColor.a -= Time.deltaTime;
            if(newColor.a <= 0)
            {
                _tipTxt.gameObject.SetActive(false);
                _coroutine_ToolTip = null;
                yield break;
            }
            _tipTxt.color = newColor;*/
            yield return null;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        // _pv = GetComponent<PhotonView>();
        _noticePool.CreatePool(2, () =>
        {
            Text unit = Instantiate(_noticeUnit, _gridTr);
            unit.gameObject.SetActive(false);
            return unit;
        });
        _tipStartPos = _tipTxt.transform.position;
    }
    // protected override void OnStart(){ }
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            ToolTip("테스트 <color=yellow>메시지</color>");
    }*/
}
