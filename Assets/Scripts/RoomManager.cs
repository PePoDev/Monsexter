using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    #region Variables
    [Header("Waiting UI")]
    [SerializeField] private TextMeshProUGUI WaitingUI_RoomTokenText;
    [SerializeField] PlayerData[] players;
    [Space]

    [Header("Mode Select UI")]
    [SerializeField] private TMP_InputField JoinUI_PlayerNameText;
    [SerializeField] private TMP_InputField JoinUI_RoomNameText;
    #endregion

    public struct PlayerData
    {
        private Image ProfilePicture;
        private TextMeshProUGUI Name;
    }

    #region Core Method
    private void Awake()
    {
        WaitingUI_RoomTokenText.SetText(PlayerPrefs.GetString("RoomToken"));

        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);
    }
    #endregion

    #region Utils Method
    public void Enter()
    {

    }
    #endregion
}
