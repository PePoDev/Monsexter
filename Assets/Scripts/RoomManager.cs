using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
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
	[SerializeField] private GameObject PanelPopup;
	[SerializeField] private Image[] characterUI;
	[SerializeField] private SpriteGroup[] PopupSprites;
	[SerializeField] private SpriteGroup[] CharacterSprites;
	[Space]
	[Header("Canvas Group")]
	[SerializeField] private Canvas canvasWaiting;
	[SerializeField] private Canvas canvasModeSelect;
	[SerializeField] private Canvas canvasSpin;

	public Sprite spySprite;

	private DatabaseReference roomReference;
	private int playerNumber;
	private int modeNumber = -1;
	private string roomToken;
	private bool tempBool;

	[Serializable]
	public struct SpriteGroup
	{
		[SerializeField] public Sprite[] sprite;
	}

	[Serializable]
	public struct PlayerData
	{
		public Image ProfilePicture;
		public TextMeshProUGUI Name;
	}
	#endregion

	#region Core Method
	private void Awake()
	{
		roomToken = PlayerPrefs.GetString("RoomToken");
		WaitingUI_RoomTokenText.SetText(roomToken);

		playerNumber = 0;

		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);
		roomReference = FirebaseDatabase.DefaultInstance.GetReference(roomToken);

		roomReference.ChildAdded += HandleChildAdded;
		roomReference.ChildChanged += HandleChildChanged;
		roomReference.ChildRemoved += HandleChildRemoved;
	}
	private void OnApplicationQuit()
	{
		//roomReference.Child($"Player{PlayerPrefs.GetInt("PlayerIndex").ToString()}").RemoveValueAsync();
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
			var playerIdx = int.Parse(args.Snapshot.Key.TrimStart('P', 'l', 'a', 'y', 'e', 'r')) - 1;
			players[playerIdx].Name.SetText(args.Snapshot.Value.ToString());
			players[playerIdx].ProfilePicture.gameObject.SetActive(true);
			playerNumber++;
		}

		if (args.Snapshot.Key.Equals("Random"))
		{
			PlayerRandomRole randomData = JsonUtility.FromJson<PlayerRandomRole>(args.Snapshot.GetRawJsonValue());
			var myRole = randomData.playerRole[PlayerPrefs.GetInt("PlayerIndex") - 1];

			if (myRole == -1)
			{
				characterUI[0].sprite = spySprite;
				PanelPopup.transform.GetChild(0).GetComponent<Image>().sprite = spySprite;
			}
			else
			{
				characterUI[0].sprite = CharacterSprites[modeNumber - 1].sprite[myRole];
				characterUI[myRole].sprite = CharacterSprites[modeNumber - 1].sprite[0];

				PanelPopup.transform.GetChild(0).GetComponent<Image>().sprite = PopupSprites[modeNumber - 1].sprite[myRole];
			}
			tempBool = true;
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

		if (args.Snapshot.Key.Equals("Status") && args.Snapshot.Value.ToString().Equals("Waiting") == false)
		{
			var modeIndex = int.Parse(args.Snapshot.Value.ToString());
			for (var i = 0; i < characterUI.Length; i++)
			{
				characterUI[i].sprite = CharacterSprites[modeIndex - 1].sprite[i];
			}

			modeNumber = modeIndex;
			tempBool = false;

			StartCoroutine(waitToActiveSpinPage());
			IEnumerator waitToActiveSpinPage()
			{
				yield return new WaitUntil(() => tempBool);
				canvasWaiting.gameObject.SetActive(false);
				canvasModeSelect.gameObject.SetActive(false);
				canvasSpin.gameObject.SetActive(true);
			}
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
	public void SelectMode(int modeIndex)
	{
		// Need player more than 4 to start this game.
		if (playerNumber < 3)
		{
			return;
		}

		var getStatus = false;
		var StatusText = "";
		roomReference.Child("Status").GetValueAsync().ContinueWith(task =>
		{
			getStatus = true;
			StatusText = task.Result.Value.ToString();
		});

		StartCoroutine(WaitFirebase());
		IEnumerator WaitFirebase()
		{
			yield return new WaitUntil(() => getStatus);

			if (StatusText.Equals("Waiting"))
			{
				var playersRandom = new PlayerRandomRole
				{
					playerRole = new int[playerNumber]
				};

				var randomNumber = UnityEngine.Random.Range(0, 8);
				for (var i = 0; i < playerNumber; i++)
				{
					playersRandom.playerRole[i] = randomNumber;
				}

				playersRandom.playerRole[UnityEngine.Random.Range(0, playerNumber)] = -1;

				roomReference.Child("Status").SetValueAsync(modeIndex).ContinueWith(task =>
				{
					roomReference.Child("Random").SetRawJsonValueAsync(JsonUtility.ToJson(playersRandom));
				});
			}
		}
	}
	public bool isAnimationShowed { get; set; } = false;
	public bool isPopupShowed { get; set; } = false;
	public void Spinning()
	{

		if (SpinUI_Slider.value < 0.7f)
		{
			SpinUI_Slider.value = 0;
			return;
		}

		SpinUI_Slider.value = 0;
		SpinUI_Slider.enabled = false;

		GameObject.Find("Bone").GetComponent<Animator>().SetTrigger("Spinning");

		StartCoroutine(SetTimeAndLoadNextScene());
		IEnumerator SetTimeAndLoadNextScene()
		{
			yield return new WaitUntil(() => isAnimationShowed);
			yield return new WaitForSeconds(0.5f);

			PanelPopup.SetActive(true);
			yield return new WaitUntil(() => isPopupShowed);

			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
	}
	public void Back()
	{
		PlayerPrefs.SetString("back", "yes");
		DestroyRoom();
		SceneManager.LoadScene(0);
	}
	public void DestroyRoom()
	{
		roomReference.Child(roomToken).RemoveValueAsync();
	}
	#endregion
}

[Serializable]
public class PlayerRandomRole
{
	public int[] playerRole;
}
