using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameLog : SingletonMonoBehaviourPunCallbacks<GameLog>
{
    [SerializeField] PlayerLogUnit _logPrefab;
    [SerializeField] Transform[] _verticalGroups;

    GameObjectPool<PlayerLogUnit> _pool;
    Queue<PlayerLogUnit> _showUnitList = new Queue<PlayerLogUnit>();

    void DestroyList()
    {
        while(_showUnitList.Count > 0)
        {
            PlayerLogUnit unit = _showUnitList.Dequeue();
            //  Destroy(tr.gameObject);
            unit.gameObject.SetActive(false);
            _pool.Set(unit);
        }
    }
    public void UpdateList()
    {
        // Debug.Log("UpdateLog, Player Count : " + PhotonNetwork.PlayerList.Length);
        DestroyList();
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            PlayerLogUnit unit = _pool.Get();
            unit.Initialize(player);
            unit.gameObject.SetActive(true);
            unit.transform.SetParent(_verticalGroups[_showUnitList.Count > 4 ? 1 : 0]);
            _showUnitList.Enqueue(unit);
        }
    }

    // GM에서 해주는게 나을듯 안쓰는 모드도 있으니
    /*public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        
    }*/

    // Start is called before the first frame update
    protected override void OnAwake()
    {
        base.OnAwake();
        _pool = new GameObjectPool<PlayerLogUnit>();
        _pool.CreatePool(2, () =>
        {
            PlayerLogUnit unit = Instantiate(_logPrefab, _verticalGroups[0]);
            unit.gameObject.SetActive(false);
            return unit;
        });
        /*if (_logPrefab.gameObject.activeSelf)
            _logPrefab.gameObject.SetActive(false);*/
    }
}
