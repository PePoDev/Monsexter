using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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
    #endregion

    #region Core Method
    private void Awake()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);
    }
    #endregion

    #region Utils Method
    public void CreateRoom()
    {
        StartCoroutine(Creating());
    }

    private IEnumerator Creating()
    {
        var roomToken = Utils.GetRandomToken();
        PlayerPrefs.SetString("RoomToken", roomToken);
        PlayerPrefs.SetString("PlayerName", CreateUI_PlayerNameText.text);
        PlayerPrefs.SetInt("PlayerIndex", 1);

        var isFinish = false;
        DatabaseReference roomReference = FirebaseDatabase.DefaultInstance.GetReference(roomToken);
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

    public void JoinRoom()
    {
        StartCoroutine(Joining());
    }
    private IEnumerator Joining()
    {
        PlayerPrefs.SetString("RoomToken", JoinUI_RoomNameText.text);
        PlayerPrefs.SetString("PlayerName", JoinUI_PlayerNameText.text);

        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.GetReference(JoinUI_RoomNameText.text);

        var hasSeat = false;
        var hasFinish = false;
        var hasRoom = true;
        var playerIndex = 0;
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
        }
        else if (hasSeat == false)
        {
            Debug.Log("Room is full");
        }
        else
        {
            PlayerPrefs.SetInt("PlayerIndex", playerIndex);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
    #endregion
}
