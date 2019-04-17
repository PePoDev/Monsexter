using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinBehaviour : MonoBehaviour
{
	[SerializeField] private RoomManager roomManager;

	public void FinishAnimated()
	{
		roomManager.isAnimationShowed = true;
	}
}
