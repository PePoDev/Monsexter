using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    #region Variables
    [Header("Waiting UI")]
    [SerializeField] private TextMeshProUGUI WaitingUI_RoomTokenText;
    [SerializeField] private PlayerData[] players;
    [Space]

    [Header("Mode Select UI")]
    [SerializeField] private TMP_InputField JoinUI_PlayerNameText;
    [SerializeField] private TMP_InputField JoinUI_RoomNameText;
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
        WaitingUI_RoomTokenText.SetText(PlayerPrefs.GetString("RoomToken"));

        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);
        var roomReference = FirebaseDatabase.DefaultInstance.GetReference(PlayerPrefs.GetString("RoomToken"));

    }
    #endregion

    #region Utils Method
    public void Enter()
    {

    }
    #endregion
}
