using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    #region Variables
    [Header("Create UI")]
    [SerializeField] private TMP_InputField CreateUI_PlayerNameText;

    [Space]

    [Header("Join UI")]
    [SerializeField] private TMP_InputField JoinUI_PlayerNameText;
    [SerializeField] private TMP_InputField JoinUI_RoomNameText;

    [SerializeField] private GameObject CanvasSplashscreen;
    [SerializeField] private GameObject CanvasMenu;

    public Loading LoadingComponet;
    #endregion

    #region Core Method
    private void Awake()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);

        if (PlayerPrefs.HasKey("back"))
        {
            PlayerPrefs.DeleteKey("back");
            CanvasSplashscreen.SetActive(false);
            CanvasMenu.SetActive(true);
        }
    }
    #endregion

    #region Utils Method
    public void CreateRoom()
    {
        StartCoroutine(Creating());
        IEnumerator Creating()
        {
            var roomToken = Utils.GetRandomToken();
            PlayerPrefs.SetString("RoomToken", roomToken);
            PlayerPrefs.SetString("PlayerName", CreateUI_PlayerNameText.text);
            PlayerPrefs.SetInt("PlayerIndex", 1);

            DatabaseReference roomReference = FirebaseDatabase.DefaultInstance.GetReference(roomToken);

            var isFinish = false;
            LoadingComponet.StartLoading();
            roomReference.Child("Player1").SetValueAsync(CreateUI_PlayerNameText.text).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    roomReference.Child("Status").SetValueAsync("Waiting").ContinueWith(task2 =>
                    {
                        if (task2.IsCompleted)
                        {
                            isFinish = true;
                        }
                    });
                }
            });

            yield return new WaitUntil(() => isFinish);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
    public void JoinRoom()
    {
        StartCoroutine(Joining());
        IEnumerator Joining()
        {
            PlayerPrefs.SetString("RoomToken", JoinUI_RoomNameText.text.ToLower());
            PlayerPrefs.SetString("PlayerName", JoinUI_PlayerNameText.text);

            DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.GetReference(JoinUI_RoomNameText.text.ToLower());

            var hasSeat = false;
            var hasFinish = false;
            var hasRoom = true;
            var playerIndex = 0;

            LoadingComponet.StartLoading();
            databaseReference.Child("Status").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    if (task.Result.Exists == false)
                    {
                        hasRoom = false;
                    }
                    else
                    {
                        var readLock = new SemaphoreSlim(1, 1);
                        for (var i = 1; i <= Config.MaxPlayer + 1; i++)
                        {
                            databaseReference.Child($"Player{i}").GetValueAsync().ContinueWith(async playerTask =>
                            {
                                if (playerTask.IsCompleted && playerTask.Result.Exists == false)
                                {
                                    await readLock.WaitAsync();
                                    try
                                    {
                                        if (hasSeat == false && i != 7)
                                        {
                                            await databaseReference.Child($"Player{i}").SetValueAsync(JoinUI_PlayerNameText.text).ContinueWith(putTask =>
                                            {
                                                if (putTask.IsCompleted)
                                                {
                                                    hasSeat = true;
                                                    playerIndex = i - 1;
                                                }
                                            });
                                        }
                                    }
                                    finally
                                    {
                                        readLock.Release();
                                    }
                                }
                            }).Wait();
                        }
                    }
                    hasFinish = true;
                }
            });

            yield return new WaitUntil(() => hasFinish);

            if (hasRoom == false)
            {
                Debug.Log("Room not found !");
                LoadingComponet.StopLoading();
            }
            else if (hasSeat == false)
            {
                Debug.Log("Room is full");
                LoadingComponet.StopLoading();
            }
            else
            {
                PlayerPrefs.SetInt("PlayerIndex", playerIndex);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
    #endregion
}
