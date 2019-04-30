using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	#region Variables
	[SerializeField] private TextMeshProUGUI TimeText;
	[SerializeField] private TextMeshProUGUI codeText;
	[SerializeField] private Image[] ImageChoiceObj;
	[SerializeField] private CharacterMode[] characterModesSprite;

	private DatabaseReference roomReference;

	private long startedTimeTick = 0;
	private string roomToken;

	[Serializable]
	public struct CharacterMode
	{
		[SerializeField] public Sprite[] characterChoice;
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
		
		roomReference.Child("Time").GetValueAsync().ContinueWith(taskGet =>
		{
			if (taskGet.IsCompleted && !taskGet.Result.Exists)
			{
				startedTimeTick = DateTime.Now.Ticks;
				roomReference.Child("Time").SetValueAsync(startedTimeTick);
			}
		});
	}

	private void Update()
	{
		if (startedTimeTick > 0)
		{
			print(TimeSpan.FromTicks(DateTime.Now.Ticks - startedTimeTick).TotalSeconds);
		}
	}

	private void HandleChildAdded(object sender, ChildChangedEventArgs args)
	{
		if (args.DatabaseError != null)
		{
			Debug.LogError(args.DatabaseError.Message);
			return;
		}
		Debug.Log(args.Snapshot.Key);

		switch (args.Snapshot.Key)
		{
			case "Status":
			{
				StartCoroutine(GetStatusAndUpdateChoice());
				break;
			}
			case "Time":
			{
				startedTimeTick = long.Parse(args.Snapshot.Value.ToString());
				break;
			}
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

	public void ClearCode()
	{
		codeText.text = "";
	}

	private IEnumerator GetStatusAndUpdateChoice()
	{
		int modeIndex = 0;
		roomReference.Child("Status").GetValueAsync().ContinueWith(task =>
		{
			if (task.IsCompleted && task.Result.Exists)
			{
				modeIndex = int.Parse(task.Result.Value.ToString());
			}
		});

		yield return new WaitUntil(() => modeIndex != 0);
		modeIndex--;
		for (int i = 0; i < ImageChoiceObj.Length; i++)
		{
			ImageChoiceObj[i].sprite = characterModesSprite[modeIndex].characterChoice[i];
		}
	}
	#endregion
}
