using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using PO = PlayerOption;
using UnityEngine.SceneManagement;

public class GameMenu : SingletonMonoBehaviour<GameMenu>
{
    [SerializeField] Button _btn_X;
    public const float Look_Sensitivity_Max = 300f;
    AudioManager _am;
    AudioMixer _audio;
    [Space]
    // [SerializeField] Slider[] _volumeSliders = new Slider[(int)AudioType.Max];
    [SerializeField] Slider _sfxVolumeSlider;
    [SerializeField] Slider _bgmVolumeSlider;
    [SerializeField] Slider _masterVolumeSlider;
    [SerializeField] Text _sfxNumber;
    [SerializeField] Text _bgmNumber;
    [SerializeField] Text _masterNumber;
    [Space]
    // [SerializeField] Slider[] _sensitivitySliders = new Slider[3];
    [SerializeField] Slider _horizontalSensitivity;
    [SerializeField] Slider _verticalSensitivity;
    [SerializeField] Slider _scopeSensitivity;
    [SerializeField] Text _horizonNumber;
    [SerializeField] Text _verticalNumber;
    [SerializeField] Text _scopeNumber;
    [Space]
    [SerializeField] Button _btn_Exit;

    Slider[] _sliders;
    Text[] _numbers;

    public void Initialize()
    {
        if (GameManager.Instance)
            _btn_X.onClick.AddListener(() => GameManager.Instance.SetMenu());
        else _btn_X.onClick.AddListener(() => gameObject.SetActive(!gameObject.activeSelf));

        _am = AudioManager.Instance;
        _audio = AudioManager.Instance.Mixer;

        // Debug.Log(PO.GetFloatOptionNormalized(FloatOptionType.Volume_Master));
        _masterVolumeSlider.value = PO.GetFloatOptionNormalized(FloatOptionType.Volume_Master);
        _bgmVolumeSlider.value = PO.GetFloatOptionNormalized(FloatOptionType.Volume_BGM);
        _sfxVolumeSlider.value = PO.GetFloatOptionNormalized(FloatOptionType.Volume_SFX);
        _masterVolumeSlider.onValueChanged.AddListener(linear => 
            _masterNumber.text = PO.ValueToLinear(FloatOptionKind.Volume
            , PO.SetFloatLinear(FloatOptionType.Volume_Master, linear)).ToString("0.0"));
        _bgmVolumeSlider.onValueChanged.AddListener(linear => 
            _bgmNumber.text = PO.ValueToLinear(FloatOptionKind.Volume
            , PO.SetFloatLinear(FloatOptionType.Volume_BGM, linear)).ToString("0.0"));
        _sfxVolumeSlider.onValueChanged.AddListener(linear => 
            _sfxNumber.text = PO.ValueToLinear(FloatOptionKind.Volume
            , PO.SetFloatLinear(FloatOptionType.Volume_SFX, linear)).ToString("0.0"));
        _masterNumber.text = _masterVolumeSlider.value.ToString("0.0");
        _bgmNumber.text = _bgmVolumeSlider.value.ToString("0.0");
        _sfxNumber.text = _masterVolumeSlider.value.ToString("0.0");
        /*_sfxVolumeSlider.onValueChanged.AddListener(call => { 
            _audio.SetFloat("SFX", (_sfxVolume = Mathf.Log10(call) * 20f)*//* * _masterVolume*//*); });*/ // 괄호를 추가하면 _sfxVolume은 마스터01의 영향을 안받는다 신기하네.. ((근데 안쓰는 코드

        _horizontalSensitivity.onValueChanged.AddListener(call =>
            _horizonNumber.text = PO.SetFloatLinear(FloatOptionType.Sensitivity_Horizontal, call).ToString("0.0"));
        _verticalSensitivity.onValueChanged.AddListener(call =>
            _verticalNumber.text = PO.SetFloatLinear(FloatOptionType.Sensitivity_Vertical, call).ToString("0.0"));
        _scopeSensitivity.onValueChanged.AddListener(call =>
            _scopeNumber.text = PO.SetFloatOption(FloatOptionType.Sensitivity_Scope, call).ToString("0.0"));
        _horizontalSensitivity.value = PO.GetFloatOptionNormalized(FloatOptionType.Sensitivity_Horizontal);
        _verticalSensitivity.value = PO.GetFloatOptionNormalized(FloatOptionType.Sensitivity_Vertical);

        _horizonNumber.text = PO.GetFloatOption(FloatOptionType.Sensitivity_Horizontal).ToString("0.0");
        _verticalNumber.text = PO.GetFloatOption(FloatOptionType.Sensitivity_Vertical).ToString("0.0");
        _scopeNumber.text = PO.GetFloatOption(FloatOptionType.Sensitivity_Scope).ToString("0.0");

        _btn_Exit.onClick.AddListener(() =>
        {
            GameManager.Instance.GoToTitle();
        });

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
    /*private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }*/
    protected override void OnStart()
    {
        base.OnStart();
        /*PO.InitFloatByKind(FloatOptionKind.Sensitivity);
        bool active = SceneManager.GetActiveScene().buildIndex != 0
            && LoadSceneManager.Instance.CurrentScene != SceneType.DeathMatch_Solo;
        Debug.Log(active);
        _btn_Exit.gameObject.SetActive(active);*/
    }
}
