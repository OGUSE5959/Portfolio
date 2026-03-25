using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ShootingTarget : MonoBehaviour, IDamagable
{
    TargetStatistics _master;
    [SerializeField] Transform _markerPrefab;
    [SerializeField] Transform _origin;

    GameObjectPool<Transform> _pool = new GameObjectPool<Transform>();
    List<Transform> _markers = new List<Transform>();

    bool IDamagable.IsDie => false;
    PhotonView IDamagable.PV => throw new System.NotImplementedException();
    float IDamagable.Health => 5959f;
    void IDamagable.SetHit(IAttackable attacker, float damage){}
    void IDamagable.SetHit(IAttackable attacker, float damage, string message) { }
    void IDamagable.SetHit(IAttackable attacker, float damage, Vector3 hitSpot)
    {
        AddHitSpot(hitSpot);
    }

    public void Initialize(TargetStatistics master) => _master = master;

    public void AddHitSpot(Vector3 hitSpot)
    {
        float dist = Vector3.Distance(_origin.position, hitSpot);
        _master.AddHitAvg(dist);

        Transform marker = _pool.Get();
        marker.position = hitSpot;
        marker.gameObject.SetActive(true);
        _markers.Add(marker);
    }
    public void ResetHitSpots()
    {
        foreach (Transform tr in _markers)
        {
            tr.gameObject.SetActive(false);
            _pool.Set(tr);
        }
        _markers.Clear();
    }
    // Start is called before the first frame update
    void Start()
    {
        _pool.CreatePool(12, () =>
        {
            GameObject marker = Instantiate(_markerPrefab, transform).gameObject;
            marker.gameObject.SetActive(false);
            return marker.transform;
        });
    }
    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
