using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("掴める・置ける距離")]
    public float reachDistance = 5f;

    [Header("積み重ねる高さの間隔")]
    public float stackHeightOffset = 0.015f;

    private void Update()
    {
        Mouse currentMouse = Mouse.current;
        Keyboard currentKeyboard = Keyboard.current;
        if (currentMouse == null || currentKeyboard == null) return;

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        // 視線の先にあるものをチェックするレイキャスト
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        bool lookingAtValidObject = false;

        if (Physics.Raycast(ray, out hit, reachDistance))
        {
            // 食材か、お皿（CookingTask）を見ているか判定
            Ingredient ingredient = hit.collider.GetComponent<Ingredient>();
            CookingTask cookingTaskHit = hit.collider.GetComponent<CookingTask>();

            if (ingredient != null || cookingTaskHit != null)
            {
                lookingAtValidObject = true;
            }
        }

        // ----------------------------------------------------
        // ① まだゲームが始まっていない場合（お皿や食材に合わせた状態でEキー）
        // ----------------------------------------------------
        if (!gm.isGameStarted)
        {
            if (lookingAtValidObject && currentKeyboard.eKey.wasPressedThisFrame)
            {
                gm.StartCookingGame();
            }
            return; // ゲーム開始前は通常のクリック処理を行わない
        }

        // ----------------------------------------------------
        // ② ゲーム中の通常操作（左クリックで食材をお皿に置く）
        // ----------------------------------------------------
        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reachDistance))
        {
            Ingredient ingredient = hit.collider.GetComponent<Ingredient>();
            if (ingredient != null)
            {
                CookingTask cookingTask = FindObjectOfType<CookingTask>();
                if (cookingTask != null)
                {
                    // 食材を複製してお皿に積む
                    GameObject duplicatedObj = Instantiate(hit.collider.gameObject);
                    int currentCount = cookingTask.currentIngredients.Count;

                    Rigidbody rb = duplicatedObj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.useGravity = false;
                    }

                    duplicatedObj.transform.SetParent(cookingTask.transform);
                    duplicatedObj.transform.localPosition = new Vector3(0, stackHeightOffset * (currentCount + 1), 0);
                    duplicatedObj.transform.localRotation = Quaternion.identity;

                    cookingTask.AddIngredient(ingredient.ingredientName, duplicatedObj);
                }
            }
        }
    }
}