using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("ゲームの設定")]
    public int currentDay = 1;
    public float timeLimit = 60f;
    private float timer;

    public bool isGameStarted = false;
    private bool isPlaying = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 日付ごとに制限時間を短くする（最低15秒までは短縮）
        timeLimit = Mathf.Max(15f, 60f - (currentDay - 1) * 15f);
        timer = timeLimit;
        isGameStarted = false;
        isPlaying = false;

        // ゲーム開始前はマウスカーソルを表示して、画面中央のクロスヘアで狙いやすくする
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (!isGameStarted || !isPlaying) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0;
            TriggerMomAttack();
        }
    }

    // 外から呼ばれてゲームをスタートさせる関数
    public void StartCookingGame()
    {
        if (isGameStarted) return;

        isGameStarted = true;
        isPlaying = true;
        timer = timeLimit;

        // マウスをロックして消す（通常プレイへ）
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("【料理スタート！】制限時間: " + timer + "秒");
    }

    public void OnCookingComplete()
    {
        if (!isPlaying) return;
        isPlaying = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Invoke("LoadNextDay", 2f);
    }

    private void TriggerMomAttack()
    {
        isPlaying = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Invoke("RestartGame", 3f);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadNextDay()
    {
        currentDay++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}