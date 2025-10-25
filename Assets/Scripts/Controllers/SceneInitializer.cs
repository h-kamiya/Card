using System.Collections.Generic;
using UnityEngine;
using Game.Presenters;
using Game.Views;

namespace Game.Controllers
{
    /// <summary>
    /// MVPパターンにおけるController/Initializer層。
    /// シーン開始時にViewとPresenterを生成し、お互いを紐付け（依存性の注入）する役割を担います。
    /// このクラスはUnityのライフサイクル（MonoBehaviour）に依存しますが、ロジックは含みません。
    /// </summary>
    public class SceneInitializer : MonoBehaviour
    {
        // Presenterのインスタンスを保持
        private CardInteractionPresenter _interactionPresenter;

        /// <summary>
        /// Unityの起動時にMVPの初期化を実行します。
        /// </summary>
        void Start()
        {
            InitializeMVP();
        }

        /// <summary>
        /// シーン内のViewを検出し、Presenterを生成して接続するメイン処理。
        /// </summary>
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
            // ★修正ポイント★: Presenterの初期化を有効化
            _interactionPresenter = new CardInteractionPresenter(views);

            Debug.Log($"MVP Initializer: Found {displays.Length} CardDisplay(s). Connected to Presenter.");

        }
    }
}