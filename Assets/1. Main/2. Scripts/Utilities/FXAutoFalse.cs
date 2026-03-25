using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXAutoFalse : MonoBehaviour
{
    [SerializeField]
    float _lifeTime;
    float _time;
    ParticleSystem[] _particles;

    public void SetLifeTime(float lifeTime)
    {
        if (lifeTime <= 0) { return; }
        _lifeTime = lifeTime;
    }
    void OnEnable()
    {
        _time = Time.time;
    }
    // Start is called before the first frame update
    void Start()
    {
        _particles = GetComponentsInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_lifeTime > 0)
        {
            if (_time + _lifeTime < Time.time)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            bool isPlaying = false;
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i].isPlaying)
                {
                    isPlaying = true;
                    break;
                }
            }
            if (!isPlaying)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
