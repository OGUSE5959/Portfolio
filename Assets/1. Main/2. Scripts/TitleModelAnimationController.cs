using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TitleModelAnimationController : AnimationController
{
    public enum Motion
    {
        Stand,
        Crouch,
        Prone,

        Max
    }
    [SerializeField] Motion _currentMotion;
    public Motion GetMotion { get { return _currentMotion; } }
    Dictionary<Motion, int> _hashTable;

    StringBuilder _sb = new StringBuilder();

    public override void Initialize()
    {
        base.Initialize();
        _hashTable = new Dictionary<Motion, int>();
        for (int i = 0; i < (int)Motion.Max; i++)
        {
            Motion motion = (Motion)i;
            _sb.Clear();
            _sb.Append(motion);

            int hash = Animator.StringToHash(_sb.ToString());
            _hashTable.Add(motion, hash);
        }
    }
    public void Play(Motion motion, bool isBlend = true)
    {
        if (_hashTable.TryGetValue(motion, out int hash))
        {
            Play(hash, isBlend);
            _currentMotion = motion;
        }
        else
            Debug.LogWarning("GunAnimationController does not contain key of motion: " + motion);
    }
}
