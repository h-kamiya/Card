using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン内のすべてのViewとPresenterを初期化し、依存性を注入（紐付け）するコントローラー。
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    // Presenterのインスタンスを保持
    private CardInteractionPresenter _interactionPresenter;

    // 現在のコードの動作確認のため、Start()で実行
    void Start()
    {
        InitializeMVP();
    }

    private void InitializeMVP()
    {
        // 1. すべての CardDisplay (View) をシーンから取得
        CardDisplay[] displays = FindObjectsByType<CardDisplay>(FindObjectsSortMode.None);

        // 2. ICardDisplayインターフェースのリストを作成 (Presenterに渡すため)
        List<ICardDisplay> views = new List<ICardDisplay>();
        foreach (var display in displays)
        {
            // ここではCardDisplayがICardDisplayを継承していないため、そのままList<CardDisplay>で渡す
            // 今後の修正で CardDisplay : ICardDisplay に変更予定
            views.Add(display);
        }

        // 3. Presenterを初期化し、Viewのリストを渡す
        // _interactionPresenter = new CardInteractionPresenter(views);

        // ★重要★: 現状はロジック修正を行わないため、初期化処理をコメントアウトします。
        // 今後のステップでこのコメントアウトを解除し、ViewとPresenterを接続します。

        Debug.Log($"MVP Initializer: Found {displays.Length} CardDisplay(s). Ready for connection.");
    }
}