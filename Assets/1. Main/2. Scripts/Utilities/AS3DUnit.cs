using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AudioSource of 3D for pooling unit
[RequireComponent(typeof(AudioSource))]
public class AS3DUnit : MonoBehaviour
{
    GameObjectPool<AS3DUnit> _poolOfMaster = null;
    AudioSource _as;

    public void PlayOnSpot(AudioClip clip, Vector3 spot)
    {
        transform.position = spot;
        gameObject.SetActive(true);
        _as.clip = clip;
        _as.Play();
        StartCoroutine(Coroutine_InsertIntoPool(clip.length));
    }
    public void Initialize(GameObjectPool<AS3DUnit> poolToInsert)
    {
        _poolOfMaster = poolToInsert;
        if (!TryGetComponent<AudioSource>(out _as))
            if (!_as) _as = gameObject.AddComponent<AudioSource>();
        _as.playOnAwake = false;
        _as.spatialBlend = 1f;
        _as.outputAudioMixerGroup = AudioManager.Instance.Group_SFX;
        _as.volume = 1f;
    }
    IEnumerator Coroutine_InsertIntoPool(float length)
    {
        yield return Utility.GetWaitForSeconds(length);
        gameObject.SetActive(false);
        _poolOfMaster.Set(this);
    }
}
