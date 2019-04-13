using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://monsexter.firebaseio.com/");
    }
    #endregion

    #region Utils Method
    public void CreateRoom()
    {
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        var roomToken = Utils.GetRandomToken();
        PlayerPrefs.SetString("RoomToken", roomToken);

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
        DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.GetReference(JoinUI_RoomNameText.text);
        databaseReference.Child("Status").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                var snapshot = task.Result;
            }
        });

    }
    #endregion
}
