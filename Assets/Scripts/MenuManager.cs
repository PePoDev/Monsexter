using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

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

    public AudioMixer audioMixer;
    public Image ui_bgm;
    public Image ui_sfx;
    public Sprite bgmMuted;
    public Sprite bgmUnMute;
    public Sprite sfxMuted;
    public Sprite sfxUnMute;

    public SpriteRenderer videoPlayerPanel;
    public VideoPlayer videoPlayer;
    public VideoClip[] videoTutorials;

    public UnityEvent OnEndedTutorial;

    private int currentTutorial = -1;
    private bool bgm, sfx;
    #endregion

    #region Core Method
    private void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Config.FirebaseURL);

        if (PlayerPrefs.HasKey("back"))
        {
            PlayerPrefs.DeleteKey("back");
            CanvasSplashscreen.SetActive(false);
            CanvasMenu.SetActive(true);
        }

        if (!PlayerPrefs.HasKey("bgm"))
        {
            PlayerPrefs.SetFloat("bgm", 1f);
        }
        bgm = PlayerPrefs.GetFloat("bgm") > 0f ? true : false;

        if (!PlayerPrefs.HasKey("sfx"))
        {
            PlayerPrefs.SetFloat("sfx", 1f);
        }
        sfx = PlayerPrefs.GetFloat("sfx") > 0f ? true : false;

        audioMixer.SetFloat("bgmVol", bgm ? 0f : -80f);
        audioMixer.SetFloat("sfxVol", sfx ? 0f : -80f);

        ui_bgm.sprite = bgm ? bgmUnMute : bgmMuted;
        ui_sfx.sprite = sfx ? sfxUnMute : sfxMuted;
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

            var isRoomStarted = false;
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
                    else if (!task.Result.Value.ToString().Equals("Waiting"))
                    {
                        isRoomStarted = true;
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
            else if (isRoomStarted)
            {
                Debug.Log("This room has started");
                LoadingComponet.StopLoading();
            }
            else
            {
                PlayerPrefs.SetInt("PlayerIndex", playerIndex);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
    public void ToggleBGM()
    {
        bgm = !bgm;
        ui_bgm.sprite = bgm ? bgmUnMute : bgmMuted;
        audioMixer.SetFloat("bgmVol", bgm ? 0f : -80f);
        PlayerPrefs.SetFloat("bgm", bgm ? 1f : 0f);
    }
    public void ToggleSFX()
    {
        sfx = !sfx;
        ui_sfx.sprite = sfx ? sfxUnMute : sfxMuted;
        audioMixer.SetFloat("sfxVol", sfx ? 0f : -80f);
        PlayerPrefs.SetFloat("sfx", sfx ? 1f : 0f);
    }

    public void Reset_RoomTest()
    {
        FirebaseDatabase.DefaultInstance.GetReference("aaaaaaaa").RemoveValueAsync().ContinueWith(task1 =>
        {
            FirebaseDatabase.DefaultInstance.RootReference.Child("aaaaaaaa").Child("Player1").SetValueAsync("Test1").ContinueWith(task2 =>
            {
                FirebaseDatabase.DefaultInstance.RootReference.Child("aaaaaaaa").Child("Player2").SetValueAsync("Test2").ContinueWith(task3 =>
                {
                    FirebaseDatabase.DefaultInstance.RootReference.Child("aaaaaaaa").Child("Status").SetValueAsync("Waiting").ContinueWith(task4 =>
                    {
                        print("Reset complete");
                    });
                });
            });
        });
    }

    public void NextTutorial()
    {
        SetAlphaOff(videoPlayerPanel);

        currentTutorial++;
        if (currentTutorial == videoTutorials.Length)
        {
            OnEndedTutorial.Invoke();
            return;
        }

        videoPlayer.clip = videoTutorials[currentTutorial];

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += _videoPlayer =>
        {
            SetAlphaOn(videoPlayerPanel);
            _videoPlayer.Play();
        };
    }

    public void SetAlphaOff(SpriteRenderer sr)
    {
        sr.color = new Color(1f, 1f, 1f, 0f);
    }

    public void SetAlphaOn(SpriteRenderer sr)
    {
        sr.color = new Color(1f, 1f, 1f, 1f);
    }
    #endregion
}
