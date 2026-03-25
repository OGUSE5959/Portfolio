using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using PO = PlayerOption;
public enum AudioType
{
    Master,
    BGM,
    SFX,

    Max
}
public class AudioManager : SingletonDontDestory<AudioManager>
{
    static Dictionary<AudioType, string> _keyList = new Dictionary<AudioType, string> {
        { AudioType.Master, "Master"},
        { AudioType.BGM, "BGM"},
        { AudioType.SFX, "SFX"} };
    [SerializeField] AudioMixer _audioMixer;
    [Space]
    [SerializeField] AudioMixerGroup _group_Master;
    [SerializeField] AudioMixerGroup _group_SFX;
    [SerializeField] AudioMixerGroup _group_BGM;
    [Space]
    [SerializeField] AS3DUnit _sfxUnitPrefab;
    GameObjectPool<AS3DUnit> _sfxUnitPool = new GameObjectPool<AS3DUnit>();
 
    public AudioMixer Mixer => _audioMixer;
    public AudioMixerGroup Group_Master => _group_Master;
    public AudioMixerGroup Group_SFX => _group_SFX;
    public AudioMixerGroup Group_BGM => _group_BGM;

    public bool SetVolume(AudioType type, float value)  // linear로 할까 말까 고민이었는데 이게 나을듯
    {
        string group = _keyList[type];
        bool boolean = _audioMixer.SetFloat(group, value);
        return boolean;
    }
    public void PlayOnSpot(AudioClip clip, Vector3 spot)
    {
        AS3DUnit audio = _sfxUnitPool.Get();
        audio.PlayOnSpot(clip, spot);
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        PO.AddFloatValueChangedCallback(FloatOptionType.Volume_Master, 
            value => SetVolume(AudioType.Master, value));
        PO.AddFloatValueChangedCallback(FloatOptionType.Volume_BGM,
            value => SetVolume(AudioType.BGM, value));
        PO.AddFloatValueChangedCallback(FloatOptionType.Volume_SFX,
            value => SetVolume(AudioType.SFX, value));

        _sfxUnitPool.CreatePool(2, () =>
        {
            // Debug.Log(_audioUnitPrefab == null);
            AS3DUnit unit = Instantiate(_sfxUnitPrefab, transform);
            unit.Initialize(_sfxUnitPool);
            unit.gameObject.SetActive(false);
            return unit;
        });
    }
    protected override void OnStart()
    {
        base.OnStart();
        // Debug.Log(PO._onFloatValueChangedCallbacks.Count);
        PO.InitFloatByKind(FloatOptionKind.Volume);
    }
}
