using System;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	#region Variables
	[SerializeField] private TextMeshProUGUI TimeText;
	[SerializeField] private TextMeshProUGUI codeText;
	[SerializeField] private Image[] ImageChoiceObj;
	[SerializeField] private CharacterMode[] characterModesSprite;

	private DatabaseReference roomReference;

	private string roomToken;

	[Serializable]
	public struct CharacterMode
	{
		[SerializeField] private Sprite[] characterChoice;
	}
	#endregion

	#region Core Method
	private void Awake()
	{
		roomToken = PlayerPrefs.GetString("RoomToken");

		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);
		roomReference = FirebaseDatabase.DefaultInstance.GetReference(roomToken);

		roomReference.ChildAdded += HandleChildAdded;
		roomReference.ChildChanged += HandleChildChanged;
		roomReference.ChildRemoved += HandleChildRemoved;
	}

	private void HandleChildAdded(object sender, ChildChangedEventArgs args)
	{
		if (args.DatabaseError != null)
		{
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

	}

	private void HandleChildChanged(object sender, ChildChangedEventArgs args)
	{
		if (args.DatabaseError != null)
		{
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

	}

	private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
	{
		if (args.DatabaseError != null)
		{
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

	}
	#endregion

	#region Utils Method
	public void EnterCode()
	{

	}

	public void AddCode(string letter)
	{
		codeText.text += letter;
	}
	#endregion
}
