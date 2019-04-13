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
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);
        WaitingUI_RoomTokenText.SetText(PlayerPrefs.GetString("RoomToken"));
        StartCoroutine(Initialization());
    }

    private void OnApplicationQuit()
    {
        var refDB = FirebaseDatabase.DefaultInstance.GetReference(PlayerPrefs.GetString("RoomToken"));
        refDB.Child($"Player{PlayerPrefs.GetInt("PlayerIndex").ToString()}").RemoveValueAsync();
    }

    IEnumerator Initialization()
    {
        DatabaseReference roomReference = FirebaseDatabase.DefaultInstance.GetReference(PlayerPrefs.GetString("RoomToken"));

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
                completeDataNumber++;
            });
        }

        yield return new WaitUntil(()=> completeDataNumber == 6);

        for (var i = 0; i < Config.MaxPlayer; i++)
        {
            players[i].Name.SetText(playersName[i]);
        }

        roomReference.ChildAdded += HandleChildAdded;
        roomReference.ChildChanged += HandleChildChanged;
        roomReference.ChildRemoved += HandleChildRemoved;
        roomReference.ChildMoved += HandleChildMoved;
    }

    private void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
    }

    private void HandleChildChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
    }

    private void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
    }

    private void HandleChildMoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
    }
    #endregion

    #region Utils Method


    public void Enter()
    {

    }
    #endregion
}
