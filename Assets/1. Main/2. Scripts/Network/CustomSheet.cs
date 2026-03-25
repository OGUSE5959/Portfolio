using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MatchType
{
    None,
    Train,
    Arcade,
    DeathMatch,
    Rounds,
    Max
}

public class CustomSheet : SingletonMonoBehaviour<CustomSheet>
{
    const float Max_Time = 150f;
    const float Min_Time = 30f;

    const int Max_Member = 5;
    const int Min_Member = 2;

    const int Max_Rounds = 5;
    const int Min_Rounds = 1;
    const int Max_Kill = 10;
    const int Min_Kill = 1;


    CustomModeMenu _master;
    [SerializeField] Image _bg;
    [SerializeField] GameObject _wnd;
    [Space]
    [SerializeField] Slider _timerSlider;
    [SerializeField] Text _timerLabel;
    [SerializeField] float _time = 90f;
    [Space]
    [SerializeField] /*InputField*/Slider _goalSlider;
    [SerializeField] Text _goalCount;
    [SerializeField] Text _goalLabel;
    [Space]
    [SerializeField] /*InputField*/Slider _personNumber;
    [SerializeField] Text _personLabel;
    [Space]
    [SerializeField] Button _teamToggle;
    [SerializeField] Text _teamLabel;
    [SerializeField] Button _publicToggle;
    [SerializeField] Text _publicLabel;
    [Space]
    [SerializeField] GameMode _gameMode;
    [SerializeField] int _goal;
    [SerializeField] int _person;
    [SerializeField] bool _hasTeam = false;
    [SerializeField] bool _isPublic = true;

    public GameMode Mode => _gameMode;
    public float TimeLimit => _time;
    public int GoalCount => _goal;
    public int PersonNumber => _person;
    public bool HasTeam => _hasTeam;
    public bool IsPublic => _isPublic;

    public void Initialize()
    {
        _master = GetComponentInParent<CustomModeMenu>(true);

        // 제한 시간 
        SetTime(_time, true);
        _timerSlider.onValueChanged.AddListener(call => SetTime((30f + call * (Max_Time - Min_Time)) 
            * (Mode == GameMode.DeathMatch_Solo || Mode == GameMode.DeathMatch_Team ? 2f : 1f), false));

        // goalCount는 게임 모드에 따라 달라지기에 상황에 따라 바꾸기
        _goalSlider.onValueChanged.AddListener(call =>
        {
            _goal = (int)call;
            _goalCount.text = _goal.ToString();
        });
        // 팀 당 인원 수
        // _personNumber.maxValue = Max_Member; _personNumber.minValue = Min_Member;
        SetPersonNumber(Min_Member);
        _personNumber.onValueChanged.AddListener(call => SetPersonNumber((int)call));

        // 팀전 여부 선택
        SetTeamToggle(false);
        _teamToggle.onClick.AddListener(() => { SetTeamToggle(!_hasTeam);});

        SetPublicToggle(true);
        _publicToggle.onClick.AddListener(() => SetPublicToggle(!_isPublic));
    }
    public void SetUp(GameMode mode)
    {
        _gameMode = mode;
        if(mode == GameMode.None)
        {
            _wnd.SetActive(false);
            SetBGColor(MatchType.None);
            return;
        }
        if(!_wnd.activeSelf) _wnd.SetActive(true);
        if(mode == GameMode.Rounds_1vs1)
        {
            SetTime(90f);
            SetBGColor(MatchType.Rounds);
            _goalLabel.text = "목표 라운드 수 : ";
            SetGoal(MatchType.Rounds);
        }
        else if(mode == GameMode.DeathMatch_Solo)
        {
            SetTime(180f);
            // _timerSlider.maxValue = 2f;/Min_Time * 2f; _timerSlider.maxValue = Max_Time * 2f;
            SetBGColor(MatchType.DeathMatch);
            _goalLabel.text = "목표 처치 수 : ";
            SetGoal(MatchType.DeathMatch);
        }
        // SetTeamToggle(false);
    }
    public void SetBGColor(MatchType matchType)
        => _bg.color = GetColor(matchType);
    Color GetColor(MatchType matchType)
    {
        Color newColor = Color.clear;
        if (matchType == MatchType.None)
            newColor = Color.black;
        if(matchType == MatchType.Rounds)
        {
            newColor = Color.cyan * 0.25f;
            newColor.a = 1f;
        }
        if (matchType == MatchType.DeathMatch)
        {
            newColor = Color.yellow * 0.25f;
            newColor.a = 1f;
        }
        return newColor;
    }
    public void SetUp(MatchType type)
    {
        if(type == MatchType.None)
        {
            ResetAll();
            return;
        }
        if (type == MatchType.Train)
        {
            _gameMode = GameMode.Training;
            return;
        }

        if (type == MatchType.DeathMatch)
        {
            _gameMode = GameMode.DeathMatch_Solo;   // 일단 개인전으로 초기화

            SetTime(180f);
            // _timerSlider.maxValue = 2f;/Min_Time * 2f; _timerSlider.maxValue = Max_Time * 2f;
            SetBGColor(MatchType.DeathMatch);
            _goalLabel.text = "목표 처치 수 : ";
            SetGoal(MatchType.DeathMatch);
        }
        else if (type == MatchType.Rounds)
        {
            _gameMode = GameMode.Rounds_1vs1;

            SetTime(90f);
            SetBGColor(MatchType.Rounds);
            _goalLabel.text = "목표 라운드 수 : ";
            SetGoal(MatchType.Rounds);
        }
        SetTeamToggle(_hasTeam);
        if (!_wnd.activeSelf) _wnd.SetActive(true);
    }
    public void ResetAll()
    {
        // SetUp(GameMode.None);
        _gameMode = GameMode.None;
        SetTime(_time = 90f, true);
        SetTeamToggle(false);
        SetPublicToggle(true);
        _wnd.SetActive(false);
        SetBGColor(MatchType.None);
    }
    public void SetGameMode(GameMode mode) => _gameMode = mode;
    void SetTime(float time, bool updateSlider = true)
    {
        _time = time;
        _timerLabel.text = "라운드 시간(<color=#ffff00>" + _time.ToString("0.0") + "</color>)";
        if (updateSlider)
            _timerSlider.value = (_time / (Mode == GameMode.DeathMatch_Solo 
                || Mode == GameMode.DeathMatch_Team ? 2f : 1f)/* - 30f*/) / Max_Time;
    }
    void SetPersonNumber(int eachTeam)
    {
        _person = eachTeam;// * (_hasTeam ? 2 : 1);
        _personLabel.text = _person.ToString();
    }
    void SetTeamToggle(bool hasTeam)
    {
        _hasTeam = hasTeam;
        if (hasTeam)
        {
            if (Mode == GameMode.Rounds_1vs1) OnRT();
            else if (Mode == GameMode.DeathMatch_Solo) OnTDM();

            // _personNumber.value = Min_Member;
            _personNumber.gameObject.SetActive(true);
            _teamLabel.text = "팀전";
        }
        else
        {
            if (Mode == GameMode.Rounds_Team) OnR1V1();
            else if (Mode == GameMode.DeathMatch_Team) OnDS();

            SetPersonNumber(1);
            _personNumber.gameObject.SetActive(false);
            _teamLabel.text = "개인전";
        }
    }
    void SetPublicToggle(bool isPublic)
    {
        _isPublic = isPublic;
        if (isPublic)
        {
            _publicLabel.text = "공개";
        }
        else
        {
            _publicLabel.text = "비공개";
        }
    }
    void SetGoal(MatchType type)
    {
        if (type == MatchType.Rounds)
        {
            _goalSlider.maxValue = Max_Rounds; 
            _goalSlider.minValue = Min_Rounds;
            _goalSlider.value = 3;
        }
        else if(type == MatchType.DeathMatch) 
        {
            _goalSlider.maxValue = Max_Kill; 
            _goalSlider.minValue = Min_Kill;
            _goalSlider.value = 5;
        }

    }
    private void OnR1V1()
    {
        // _personNumber.value = 1;
        _gameMode = GameMode.Rounds_1vs1;
    }
    private void OnDS()
    {
        // SetGoal(false);
        _gameMode = GameMode.DeathMatch_Solo;
    }
    private void OnRT()
    {
        // SetGoal(true);
        _gameMode = GameMode.Rounds_Team;
    }
    private void OnTDM()
    {
        // SetGoal(false);
        _gameMode = GameMode.DeathMatch_Team;
    }
}
