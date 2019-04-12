using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    #region Variables
    [Header("Create UI")]
    [SerializeField] private TextMeshProUGUI CreateUI_PlayerNameText;

    [Space]

    [Header("Join UI")]
    [SerializeField] private TextMeshProUGUI JoinUI_PlayerNameText;
    [SerializeField] private TextMeshProUGUI JoinUI_RoomNameText;

    private DatabaseReference databaseReference;
    #endregion

    #region Core Method
    private void Awake()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://monsexter.firebaseio.com/");
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    #endregion

    #region Utils Method
    public void CreateRoom()
    {
        var roomToken = Utils.GetRandomToken();
        PlayerPrefs.SetString("RoomToken", roomToken);

        Debug.Log(roomToken);

        databaseReference.SetValueAsync("This is root");
        databaseReference.Push().SetValueAsync("This is child");
    }

    public void JoinRoom()
    {

    }
    #endregion
}
