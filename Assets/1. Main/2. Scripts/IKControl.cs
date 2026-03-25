using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKControl : MonoBehaviour
{

	private Animator _animator;

	[SerializeField] bool _isIkActive = true;
	[SerializeField] Transform _rightHandTarget;
    [SerializeField] Transform _leftHandTarget;

	public void SetHandTargets(Transform right, Transform left)
	{
        _rightHandTarget = right;
        _leftHandTarget = left;
	}
	public void SetHandTargets()
	{
		SetHandTargets(null, null);
	}


	void Start()
	{
		_animator = GetComponent<Animator>();
	}

	//a callback for calculating IK
	void OnAnimatorIK(int layerIndex)
	{
		if (_animator)
		{
			
			float value = _isIkActive ? 1 : 0;

			_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, value);
			_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, value);
			_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, value);
			_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, value);

			if (_isIkActive)
			{
				// 둘 중 하나라도 할당되어있으면 IK를 적용시키고 또 없으면 무시하는 안전장치
				if(_rightHandTarget)
				{
					_animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandTarget.position);
					_animator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandTarget.rotation);
				}
				if(_leftHandTarget)
                {
					_animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandTarget.position);
					_animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandTarget.rotation);
				}				
			}
		}
	}
}
