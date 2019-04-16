using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
	#region Variables
	[Header("Waiting UI")]
	[SerializeField] private TextMeshProUGUI WaitingUI_RoomTokenText;
	[SerializeField] private PlayerData[] players;
	[Space]

	[Header("Spin UI")]
	[SerializeField] private Slider SpinUI_Slider;
	[Space]

	[Header("Canvas Group")]
	[SerializeField] private Canvas canvasWaiting;
	[SerializeField] private Canvas canvasModeSelect;
	[SerializeField] private Canvas canvasSpin;

	private DatabaseReference roomReference;

	private int playerNumber;
	private string roomToken;
	#endregion

	[Serializable]
	public struct PlayerData
	{
		public Image ProfilePicture;
		public TextMeshProUGUI Name;
	}

	#region Core Method
	private void Awake()
	{
		roomToken = PlayerPrefs.GetString("RoomToken");
		WaitingUI_RoomTokenText.SetText(roomToken);

		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);
		roomReference = FirebaseDatabase.DefaultInstance.GetReference(roomToken);

		roomReference.ChildAdded += HandleChildAdded;
		roomReference.ChildChanged += HandleChildChanged;
		roomReference.ChildRemoved += HandleChildRemoved;

		//StartCoroutine(Initialization());
	}

	private IEnumerator Initialization()
	{
		var completeDataNumber = 0;
		var playersName = new string[6];
		for (var i = 1; i <= Config.MaxPlayer; i++)
		{
			var x = i;
			roomReference.Child($"Player{x}").GetValueAsync().ContinueWith(playerTask =>
			{
				if (playerTask.IsCompleted && playerTask.Result.Exists)
				{
					playersName[x - 1] = playerTask.Result.Value as string;
				}
				else
				{
					playersName[x - 1] = "";
				}
				completeDataNumber++;
			});
		}

		yield return new WaitUntil(() => completeDataNumber == 6);

		for (var i = 0; i < Config.MaxPlayer; i++)
		{
			players[i].Name.SetText(playersName[i]);
		}
	}

	private void OnApplicationQuit()
	{
		roomReference.Child($"Player{PlayerPrefs.GetInt("PlayerIndex").ToString()}").RemoveValueAsync();
	}

	private void HandleChildAdded(object sender, ChildChangedEventArgs args)
	{
		if (args.DatabaseError != null)
		{
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		Debug.Log("Added: " + args.Snapshot.Key);
		if (args.Snapshot.Key.Contains("Player"))
		{
			players[int.Parse(args.Snapshot.Key.TrimStart('P', 'l', 'a', 'y', 'e', 'r')) - 1].Name.SetText(args.Snapshot.Value.ToString());
		}
	}

	private void HandleChildChanged(object sender, ChildChangedEventArgs args)
	{
		if (args.DatabaseError != null)
		{
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		Debug.Log("Changed: " + args.Snapshot.Key);
		if (args.Snapshot.Key.Equals("Status") && !args.Snapshot.Value.ToString().Equals("Waiting"))
		{
			canvasWaiting.gameObject.SetActive(false);
			canvasModeSelect.gameObject.SetActive(false);
			canvasSpin.gameObject.SetActive(true);
		}
	}

	private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
	{
		if (args.DatabaseError != null)
		{
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		Debug.Log("Removed: " + args.Snapshot.Key);
		if (args.Snapshot.Key.Contains("Player"))
		{
			players[int.Parse(args.Snapshot.Key.TrimStart('P', 'l', 'a', 'y', 'e', 'r')) - 1].Name.SetText("");
		}
	}
	#endregion

	#region Utils Method
	public void SelectMode(int i)
	{
		roomReference.Child("Status").SetValueAsync(i);
		foreach (PlayerData player in players)
		{
			if (string.IsNullOrEmpty(player.Name.text))
			{

			}
		}
	}

	public void Spinning()
	{

		if (SpinUI_Slider.value < 0.7f)
		{
			SpinUI_Slider.value = 0;
			return;
		}

		SpinUI_Slider.value = 0;
		SpinUI_Slider.enabled = false;


	}
	#endregion
}
