using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FloatOptionKind
{
    None = -1,

    Sensitivity,
    Volume,

    Max
}
public enum FloatOptionType
{
    _START_SENSITIVITY_,
    Sensitivity_Horizontal,
    Sensitivity_Vertical,
    Sensitivity_Scope,
    _END_SENSITIVITY_,

    _START_VOLUME_ = 10,
    Volume_Master,
    Volume_BGM,
    Volume_SFX,
    _END_VOLUME_,

    Max     // 안쓸것같다(꼭 1씩 증가하지 않음)
}
/*public enum OptionName
{

}*/
public static class PlayerOption    // PlayerPrebs의 각 Value에는 Linear값이 아닌 실제 쓰는 값을 넣습니다!!
{
    public readonly static float _MAX_SENSITIVITY_ = 300f;
    static float _defaultSensitivity = 180f;
    static string _horizontalSensitivity = "Sensitivity_Horizontal";
    static string _verticalSensitivity = "Sensitivity_Vertical";
    static string _scopeSensitivity = "Sensitivity_Scope";

    static float _defaultVolume = -3.522f; //Mathf.Log10(2f / 3f) * 20f;
    static string _masterVolume = "Volume_Master";
    static string _bgmVolume = "Volume_BGM";
    static string _sfxVolume = "Volume_SFX";

    static Dictionary<FloatOptionType, string> _floatKeyList = new Dictionary<FloatOptionType, string>
    {
        { FloatOptionType.Sensitivity_Horizontal, _horizontalSensitivity },
        { FloatOptionType.Sensitivity_Vertical, _verticalSensitivity },
        { FloatOptionType.Sensitivity_Scope, _scopeSensitivity },
        { FloatOptionType.Volume_Master, _masterVolume },
        { FloatOptionType.Volume_BGM, _bgmVolume },
        { FloatOptionType.Volume_SFX, _sfxVolume }
    };
    static Dictionary<FloatOptionKind, List<FloatOptionType>> _kindToTypes = new Dictionary<FloatOptionKind, List<FloatOptionType>>
    {
        { FloatOptionKind.Sensitivity, new List<FloatOptionType> { FloatOptionType.Sensitivity_Horizontal, FloatOptionType.Sensitivity_Vertical/*, FloatOptionType.Sensitivity_Scope*/ } },
        { FloatOptionKind.Volume, new List<FloatOptionType> { FloatOptionType.Volume_Master, FloatOptionType.Volume_BGM, FloatOptionType.Volume_SFX } }
    };
    static Dictionary<FloatOptionKind, float> _floatDefaultList = new Dictionary<FloatOptionKind, float>
    {
        {FloatOptionKind.Sensitivity, _defaultSensitivity },
        {FloatOptionKind.Volume, _defaultVolume }
    };

    private delegate float FloatValueToLinear(float value);
    private delegate float FloatLinearToValue(float linear);
    static Dictionary<FloatOptionKind, FloatValueToLinear> _floatValueToLinearList = new Dictionary<FloatOptionKind, FloatValueToLinear>
    {
        {FloatOptionKind.Sensitivity, value => value / _MAX_SENSITIVITY_ },
        {FloatOptionKind.Volume, value => Mathf.Pow(10f, value / 20f) }
    };
    static Dictionary<FloatOptionKind, FloatLinearToValue> _floatLinearToValueList = new Dictionary<FloatOptionKind, FloatLinearToValue>
    {
        {FloatOptionKind.Sensitivity, linear => linear * _MAX_SENSITIVITY_ },
        {FloatOptionKind.Volume, linear => Mathf.Log10(linear) * 20f }
    };

    public static readonly Dictionary<FloatOptionType, List<Action<float>>> _onFloatValueChangedCallbacks = new Dictionary<FloatOptionType, List<Action<float>>>
    {
        { FloatOptionType.Sensitivity_Horizontal,   new List<Action<float>>() },
        { FloatOptionType.Sensitivity_Vertical,     new List<Action<float>>() },
        { FloatOptionType.Sensitivity_Scope,        new List<Action<float>>() },
        { FloatOptionType.Volume_Master,            new List<Action<float>>() },
        { FloatOptionType.Volume_BGM,               new List<Action<float>>() },
        { FloatOptionType.Volume_SFX,               new List<Action<float>>() }
    };

    // static PlayerOption() { }
    /// <summary>
    /// 'linear'를 type에 따라 교정한 값을 리턴합니다!!
    /// </summary>
    /// <param name="type"></param>
    /// <param name="linear"></param>
    /// <param name="save"></param>
    /// <returns></returns>
    public static float SetFloatLinear(FloatOptionType type, float linear, bool save = true)    
        => SetFloatOption(type, _floatLinearToValueList[FindFloatKindByType(type)](linear), save);
    public static float SetFloatOption(FloatOptionType type, float value, bool save = true)
    {
        PlayerPrefs.SetFloat(_floatKeyList[type], value);
        foreach (Action<float> action in _onFloatValueChangedCallbacks[type])
            action?.Invoke(value);
        if (save) PlayerPrefs.Save();
        return value;
    }
    public static float GetFloatOption(FloatOptionType type)
    {
        string key = _floatKeyList[type];
        if (!PlayerPrefs.HasKey(key)) 
            PlayerPrefs.SetFloat(key, DefaultValue(type));
        /*if (FindFloatKindByType(type) == FloatOptionKind.Volume)
            Debug.Log(DefaultValue(type));*/
        return PlayerPrefs.GetFloat(key);
    }
    public static float GetFloatOptionNormalized(FloatOptionType type)
        => ValueToLinear(type, GetFloatOption(type));
    public static FloatOptionKind FindFloatKindByType(FloatOptionType type)
    {
        foreach (var pair in _kindToTypes)
            if (pair.Value.Contains(type)) return pair.Key;
        return FloatOptionKind.None;
    }
    /// <summary>
    /// 'type'에 맞는 kind가 없으면 Linear값일 수도 있고 아닐 수도 있는 1f를 반환합니다
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static float DefaultValue(FloatOptionType type)
    {
        FloatOptionKind kind = FindFloatKindByType(type);
        if (kind == FloatOptionKind.None)
            return 1f;
        return _floatDefaultList[kind];
    }
    public static float ValueToLinear(FloatOptionType type, float value)
    {
        FloatOptionKind kind = FindFloatKindByType(type);
        if (kind  == FloatOptionKind.None)
            return value;
        return ValueToLinear(kind, value);
    }
    public static float ValueToLinear(FloatOptionKind kind, float value)
        => _floatValueToLinearList[kind](value);
    public static float LinearToValue(FloatOptionType type, float value)
    {
        FloatOptionKind kind = FindFloatKindByType(type);
        if (kind == FloatOptionKind.None)
            return value;
        /*Debug.Log(value);
        Debug.Log(_floatLinearToValueList[kind](value));*/
        return LinearToValue(kind, value);
    }
    public static float LinearToValue(FloatOptionKind kind, float value)
        => _floatLinearToValueList[kind](value);
    public static void AddFloatValueChangedCallback(FloatOptionType type, Action<float> callback)
    {
        /*if (callback == null)
            throw new ArgumentNullException(nameof(callback));*/

        if (!_onFloatValueChangedCallbacks.TryGetValue(type, out var callbackList))
        {
            callbackList = new List<Action<float>>();
            _onFloatValueChangedCallbacks[type] = callbackList;
        }

        // 중복 등록 방지 (필요 시 주석 처리 가능)
        if (!callbackList.Contains(callback))
            callbackList.Add(callback);
    }

    public static void InitFloatByKind(FloatOptionKind kind)
    {
        foreach (FloatOptionType type in _kindToTypes[kind])
            SetFloatOption(type, DefaultValue(type));
    }

    #region 쓸까 말까 고민이다
    public static float GetHorizontalSensitivity()
    {
        if(!PlayerPrefs.HasKey(_horizontalSensitivity))
            SetHorizontalSensitivity(_defaultSensitivity);
        return PlayerPrefs.GetFloat(_horizontalSensitivity);
    }
    public static float GetVerticalSensitivity()
    {
        if (!PlayerPrefs.HasKey(_verticalSensitivity))
            SetVerticalSensitivity(_defaultSensitivity);
        return PlayerPrefs.GetFloat(_verticalSensitivity);
    }
    public static void SetHorizontalSensitivity(float sensitivity)
        => PlayerPrefs.SetFloat(_horizontalSensitivity, sensitivity);
    public static void SetVerticalSensitivity(float sensitivity)
        => PlayerPrefs.SetFloat (_verticalSensitivity, sensitivity);

    public static float GetMasterVolume()
    {
        if(!PlayerPrefs.HasKey(_masterVolume))
            SetMasterVolume(_defaultVolume);
        return PlayerPrefs.GetFloat(_masterVolume);
    }
    public static float GetBgmVolume()
    {
        if (!PlayerPrefs.HasKey(_bgmVolume))
            SetBgmVolume(_defaultVolume);
        return PlayerPrefs.GetFloat(_bgmVolume);
    }
    public static float GetSfxVolume()
    {
        if (!PlayerPrefs.HasKey(_sfxVolume))
            SetSfxVolume(_defaultVolume);
        return PlayerPrefs.GetFloat(_sfxVolume);
    }
    public static void SetMasterVolume(float linear)
        => PlayerPrefs.SetFloat(_masterVolume, linear);
    public static void SetBgmVolume(float linear)
        => PlayerPrefs.SetFloat(_bgmVolume, linear);
    public static void SetSfxVolume(float linear)
        => PlayerPrefs.SetFloat(_sfxVolume, linear);
    #endregion
}
