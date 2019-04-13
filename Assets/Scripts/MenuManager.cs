using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Threading;
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
        var roomToken = Utils.GetRandomToken();
        PlayerPrefs.SetString("RoomToken", roomToken);
        PlayerPrefs.SetString("PlayerName", CreateUI_PlayerNameText.text);

        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        DatabaseReference roomReference = databaseReference.Child(roomToken);
        roomReference.Child("Player1").SetValueAsync(CreateUI_PlayerNameText.text).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                roomReference.Child("Status").SetValueAsync("Waiting").ContinueWith(task2 =>
                {
                    if (task2.IsCompleted)
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                    }
                });
            }
        });

    }

    public void JoinRoom()
    {
        PlayerPrefs.SetString("RoomToken", JoinUI_RoomNameText.text);
        PlayerPrefs.SetString("PlayerName", JoinUI_PlayerNameText.text);

        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference.Child(JoinUI_RoomNameText.text);
        databaseReference.Child("Status").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Exists == false)
                {
                    Debug.Log("Room not found !");
                    return;
                }

                var readLock = new SemaphoreSlim(1, 1);
                var hasSeat = false;
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
                                            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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

                if (hasSeat == false)
                {
                    Debug.Log("Room is fully");
                }
            }
        });
    }
    #endregion
}
