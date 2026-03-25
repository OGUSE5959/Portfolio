using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

// 콜백 등록 저장 기능을 위한 시도
public enum PlayerInputType
{
    Move,
    Jump,
    Sprint,
    Crouch,
    MouseWheel,
    MouseX,
    MouseY,
    LeanLeft,
    LeanRight,
    Interact,
    Item1,
    Item2,
    Item3,
    Item4,
    ThrowItem,
    ChangeView
}
public enum InputTiming
{
    Started,
    Performed,
    Canceled,

    Max
}

[RequireComponent(typeof(PlayerCameraController))]
public class PlayerInputController : MonoBehaviour
{
    PlayerController _me;
    PlayerActionMaps Control;
    Dictionary<PlayerInputType, InputAction> _inputActions;
    Dictionary<InputAction, Action<InputAction.CallbackContext>[]> _callbackList;// = new Dictionary<InputAction, Action<InputAction.CallbackContext>[]>();

    public PlayerActionMaps ActionMaps => Control;
    public PlayerActionMaps.PlayerActions Actions => Control.Player;
    public PlayerActionMaps.WeaponActions Weapon => Control.Weapon;
    public bool CanInput => _me.IsMe && !_me.IsGameOver;

    public float MouseX => Actions.MouseX.ReadValue<float>() / 20f;//Input.GetAxisRaw("Mouse X");
    public float MouseY => Actions.MouseY.ReadValue<float>() / 20f; // Input.GetAxisRaw("Mouse Y");
    public bool Sprint => Actions.Sprint.IsPressed(); //Input.GetKey(KeyCode.LeftShift);

    public bool IsLeanInputLeft => Actions.Lean_Left.IsPressed();
    public bool IsLeanInputRight => Actions.Lean_Right.IsPressed();

    void CreateInput()
    {
        if (!_me.IsMe) return;
        Control = new PlayerActionMaps();
        Control.Enable();
        _inputActions = new Dictionary<PlayerInputType, InputAction>
        {
            { PlayerInputType.Move, Actions.Move },
            { PlayerInputType.Jump, Actions.Jump },
            { PlayerInputType.Sprint, Actions.Sprint },
            { PlayerInputType.Crouch, Actions.Crouch },
            { PlayerInputType.MouseWheel, Actions.MouseWheel },
            { PlayerInputType.MouseX, Actions.MouseX },
            { PlayerInputType.MouseY, Actions.MouseY },
            { PlayerInputType.LeanLeft, Actions.Lean_Left },
            { PlayerInputType.LeanRight, Actions.Lean_Right },
            { PlayerInputType.Interact, Actions.Interact },
            { PlayerInputType.Item1, Actions.Item1 },
            { PlayerInputType.Item2, Actions.Item2 },
            { PlayerInputType.Item3, Actions.Item3 },
            { PlayerInputType.Item4, Actions.Item4 },
            { PlayerInputType.ThrowItem, Actions.ThrowItem },
            { PlayerInputType.ChangeView, Actions.ChangeView }
        };
    }
    public void RegistAction(PlayerInputType inputType, InputTiming timing, Action<InputAction.CallbackContext> action)
    {
        InputAction input = _inputActions[inputType];
        int idx = (int)timing;
        if (!_callbackList.TryGetValue(input, out Action<InputAction.CallbackContext>[] list))
            _callbackList.Add(input, new Action<InputAction.CallbackContext>[3]);
        _callbackList[input][idx] = action;        
    }
    void OnInputEnabled()
    {
        if (Control == null) return;
        Control.Player.Enable();
        /*foreach(var pair in _callbackList)
        {
            InputAction input = pair.Key;
            if(pair.Value != null) 
                for(int i = 0; i < pair.Value.Length; i++)
                {
                    var action = pair.Value[i];
                    if (action == null) continue;
                    InputTiming timing = (InputTiming)i;
                    switch (timing)
                    {
                        case InputTiming.Started:
                            input.started += action; break;
                        case InputTiming.Performed:
                            input.performed += action; break;
                        case InputTiming.Canceled:
                            input.canceled += action; break;
                    }
                }
        }*/
    }
    void OnInputDisabled()
    {
        if (Control == null) return;
        Control.Player.Disable();
        /*foreach (var pair in _callbackList)
        {
            InputAction input = pair.Key;
            if (pair.Value != null)
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    var action = pair.Value[i];
                    if (action == null) continue;
                    InputTiming timing = (InputTiming)i;
                    switch (timing)
                    {
                        case InputTiming.Started:
                            input.started -= action; break;
                        case InputTiming.Performed:
                            input.performed -= action; break;
                        case InputTiming.Canceled:
                            input.canceled -= action; break;
                    }
                }
        }*/
    }
    private void OnEnable()
    {
        if (!_me.IsMe) return;
        OnInputEnabled();
    }
    private void OnDisable()
    {
        if (!_me.IsMe) return;
        OnInputDisabled();
    }
    private void OnDestroy()
    {
        if (!_me.IsMe) return;
        // Debug.Log("OnDestroy PlayerInputCtrl");
        // UnsubscribeAll();  // 모든 이벤트 구독 해제
        if (Control != null) Control.Dispose();
    }
    private void Awake()
    {
        _me = GetComponent<PlayerController>();
        CreateInput();
    }
}
