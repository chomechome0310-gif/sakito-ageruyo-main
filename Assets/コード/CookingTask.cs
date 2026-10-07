using UnityEngine;
using System.Collections.Generic;

public class CookingTask : MonoBehaviour
{
    [Header("料理のレシピ設定")]
    public List<string> correctRecipe = new List<string>() { "Bread", "Lettuce", "Cheese Thin", "Tomato Slice" };

    [Header("現在の状態")]
    public List<string> currentIngredients = new List<string>();
    public bool isCookingComplete = false;

    // お皿の上に置かれた「複製された食材オブジェクト」を覚えておくリスト
    private List<GameObject> spawnedIngredients = new List<GameObject>();

    // 食材をお皿に追加する（複製されたオブジェクトを受け取る）

    [Header("効果音")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startCookingSound; // 調理開始音(1個目の食材を置いたとき)

    [System.Serializable]
    public class IngredientSound
    {
        public string ingredientName;   // "Bread", "Lettuce" など
        public AudioClip[] clips;       // その食材専用の音(複数登録でランダム再生)
    }

    [SerializeField] private List<IngredientSound> ingredientSounds = new List<IngredientSound>();
    [SerializeField] private AudioClip defaultPlaceSound; // 登録忘れの食材用の予備音

    [SerializeField] private AudioClip mistakeSound;
    [SerializeField] private AudioClip completeSound;

    [Header("音のばらつき設定")]
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    // 高速検索用の辞書(インスペクターのListから自動生成)
    private Dictionary<string, AudioClip[]> soundDict;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;

        // Listから辞書を作成
        soundDict = new Dictionary<string, AudioClip[]>();
        foreach (var entry in ingredientSounds)
        {
            if (!soundDict.ContainsKey(entry.ingredientName))
            {
                soundDict.Add(entry.ingredientName, entry.clips);
            }
        }
    }

    public bool AddIngredient(string ingredientName, GameObject spawnedObj)
    {
        if (isCookingComplete) return false;

        // 追加する前に「これが最初の食材かどうか」を判定
        bool isFirstIngredient = currentIngredients.Count == 0;

        currentIngredients.Add(ingredientName);
        spawnedIngredients.Add(spawnedObj);

        Debug.Log("皿に追加された食材: " + ingredientName);

        if (isFirstIngredient)
        {
            PlaySingleShot(startCookingSound);
        }

        PlayPlaceSound(ingredientName);

        return CheckRecipe();
    }

    private bool CheckRecipe()
    {
        int currentIndex = currentIngredients.Count - 1;

        // 順番が違っていた場合
        if (currentIngredients[currentIndex] != correctRecipe[currentIndex])
        {
            Debug.Log("順番が違います！お皿の上の食材をリセットします！");
            PlaySingleShot(mistakeSound);
            ResetAllIngredients();
            return false;
        }

        // 完成チェック
        if (currentIngredients.Count == correctRecipe.Count)
        {
            isCookingComplete = true;
            Debug.Log("料理が完璧に完成しました！お母さんから逃げ切れる！");
            PlaySingleShot(completeSound);
        }

        return true;
    }

    // 間違えたときにお皿の上の食材を全部消してリセットする
    public void ResetAllIngredients()
    {
        foreach (GameObject obj in spawnedIngredients)
        {
            if (obj != null)
            {
                Destroy(obj); // お皿の上の複製を消す
            }
        }

        currentIngredients.Clear();
        spawnedIngredients.Clear();
    }

    // ==== 効果音関連 ====

    private void PlayPlaceSound(string ingredientName)
    {
        AudioClip[] clips = null;

        // 該当する食材の音があるか探す
        if (soundDict.TryGetValue(ingredientName, out AudioClip[] found) && found.Length > 0)
        {
            clips = found;
        }
        else if (defaultPlaceSound != null)
        {
            // 見つからなければ予備音を単発配列として扱う
            clips = new AudioClip[] { defaultPlaceSound };
        }

        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip);
    }

    private void PlaySingleShot(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(clip);
    }
}