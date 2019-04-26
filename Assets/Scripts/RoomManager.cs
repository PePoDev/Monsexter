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
	[SerializeField] private GameObject PanelPopup;
	[SerializeField] private Image[] characterUI;
	[SerializeField] private SpriteGroup[] PopupSprites;
	[SerializeField] private SpriteGroup[] CharacterSprites;
	[Space]

	[Header("Canvas Group")]
	[SerializeField] private Canvas canvasWaiting;
	[SerializeField] private Canvas canvasModeSelect;
	[SerializeField] private Canvas canvasSpin;

	private DatabaseReference roomReference;

	private int randomRole;
	private int playerNumber;
	private string roomToken;

	[Serializable]
	public struct SpriteGroup
	{
		[SerializeField] public Sprite[] sprite;
	}
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
	public void SelectMode(int modeIndex)
	{
		var getStatus = false;
		string StatusText = "";
		roomReference.Child("Status").GetValueAsync().ContinueWith(task =>
		{
			getStatus = true;
			StatusText = task.Result.Value.ToString();
		});

		StartCoroutine(WaitFirebase());
		IEnumerator WaitFirebase()
		{
			yield return new WaitUntil(()=> getStatus);

			if (StatusText.Equals("Waitting"))
			{
				for (var i = 0; i < characterUI.Length; i++)
				{
					characterUI[i].sprite = CharacterSprites[modeIndex - 1].sprite[i];
				}

				// TODO: Random with different character
				randomRole = UnityEngine.Random.Range(0, 8);
				characterUI[0].sprite = CharacterSprites[modeIndex - 1].sprite[randomRole];
				characterUI[randomRole].sprite = CharacterSprites[modeIndex - 1].sprite[0];

				PanelPopup.transform.GetChild(0).GetComponent<Image>().sprite = PopupSprites[modeIndex - 1].sprite[randomRole];

				Debug.Log("Random character index: " + randomRole);

				roomReference.Child("Status").SetValueAsync(modeIndex);
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

			PanelPopup.SetActive(true);
			yield return new WaitUntil(() => isPopupShowed);

			var hasSetData = false;
			roomReference.Child("Time").GetValueAsync().ContinueWith(taskGet =>
			{
				if (taskGet.IsCompleted && !taskGet.Result.Exists)
				{
					roomReference.Child("Time").SetValueAsync(DateTime.Now.Ticks).ContinueWith((taskSet) =>
					{
						if (taskSet.IsCompleted)
						{
							hasSetData = true;
						}
					});
				}
				else
				{
					hasSetData = true;
				}
			});

			yield return new WaitUntil(() => hasSetData);

			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
	}
	#endregion
}
