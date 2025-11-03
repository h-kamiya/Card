using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Models;    // ★新規追加: CardDataへの参照（CS0246解消）
using Game.Views;     // ★新規追加: ICardDisplayへの参照（CS0246解消）

namespace Game.Presenters
{
    /// <summary>
    /// MVPパターンにおけるPresenter層。
    /// Viewからの入力イベントを購読し、Model（CardData）の状態を変更するゲームロジックの中核を担います。
    /// カードの選択、裏返し、グループ移動などの相互作用ロジックを管理します。
    /// </summary>
    public class CardInteractionPresenter
    {
        // ★重要フィールド★: Viewのリスト
        private readonly List<ICardDisplay> _views;

        // ★修正点 1: 単一のCardStackから、IDをキーとするDictionaryに変更★
        /// <summary>
        /// Presenterが管理する全てのCardStack（山札、捨て札など）のコレクション。
        /// </summary>
        private readonly Dictionary<string, CardStack> _cardStacks;

        // Presenterが管理する選択中カードのリスト (以前の静的リストに代わるもの)
        /// <summary>
        /// Presenterがゲームロジックに基づいて管理する、現在選択中のCardDataのリスト。
        /// このリストはViewの静的リストに代わるものであり、テスタブルなロジックを提供します。
        /// </summary>
        private readonly List<CardData> _selectedCardData = new List<CardData>();

        /// <summary>
        /// ポップアップメニューViewへの参照（インターフェースで保持）。メニューの表示・非表示を指示します。
        /// </summary>
        private readonly IPopupMenu _popupMenu;

        /// <summary>
        /// 現在右クリックによりメニューが開かれているCardStackのIDを保持します。
        /// </summary>
        private string _currentMenuStackId;


        /// <summary>
        /// CardInteractionPresenterの新しいインスタンスを初期化します。
        /// 全てのViewからの入力イベントを購読し、MVPの接続を完了します。
        /// </summary>
        /// <param name="views">シーン内に存在するすべてのICardDisplay実装（View）のリスト。</param>
        public CardInteractionPresenter(List<ICardDisplay> views)
        {
            _views = views;

            // Viewからの入力イベントを購読
            foreach (var view in _views)
            {
                view.OnCardSingleClick += HandleSingleClick;
                view.OnCardDoubleClick += HandleDoubleClick;
                view.OnCardRightClick += HandleRightClick;

                // ★新規購読★: ドラッグイベント
                view.OnCardDragStart += HandleDragStart;
                view.OnCardDragging += HandleDragging;
                view.OnCardEndDrag += HandleEndDrag;
            }
        }

        /// <summary>
        /// Presenterの新しいインスタンスを初期化し、View、CardStack、PopupMenuを接続します。
        /// </summary>
        /// <param name="views">シーン内に存在する全てのICardDisplay実装（View）のリスト。</param>
        /// <param name="initialStacks">Presenterが管理すべき全てのCardStackのリスト。</param>
        /// <param name="popupMenu">ポップアップメニューViewのインスタンス。</param>
        public CardInteractionPresenter(List<ICardDisplay> views, List<CardStack> initialStacks, IPopupMenu popupMenu)
        {
            _views = views;

            // Dictionaryを初期化し、リストからデータを登録
            _cardStacks = initialStacks.ToDictionary(stack => stack.Id, stack => stack);

            _popupMenu = popupMenu;
            _popupMenu.OnCancel += HandleMenuCancel;

            // Viewからの入力イベントを購読
            foreach (var view in _views)
            {
                // ★修正箇所：クリック、ダブルクリック、ドラッグイベントの購読を追加
                view.OnCardSingleClick += HandleSingleClick;
                view.OnCardDoubleClick += HandleDoubleClick;
                view.OnCardRightClick += HandleRightClick;
                view.OnCardDragStart += HandleDragStart;
                view.OnCardDragging += HandleDragging;
                view.OnCardEndDrag += HandleEndDrag;
            }

            Debug.Log($"Presenter initialized. Managing {_cardStacks.Count} card stacks.");
        }

        // ★新規追加★: 特定のCardStackを取得するためのヘルパー（privateのまま維持）
        private CardStack GetStack(string id)
        {
            if (_cardStacks.TryGetValue(id, out CardStack stack))
            {
                return stack;
            }
            Debug.LogWarning($"CardStack with ID '{id}' not found.");
            return null;
        }

        /// <summary>
        /// Viewからのシングルクリックイベントを処理し、選択状態をトグルするロジックを実行します。
        /// </summary>
        /// <param name="clickedView">クリックされたCardDisplay (View)。</param>
        public void HandleSingleClick(CardDisplay clickedView)
        {
            // 1. Model (CardData) の状態を変更
            bool isSelected = !clickedView.CardData.isSelected;
            clickedView.CardData.isSelected = isSelected;

            // 2. 選択リストの状態を更新 (Viewの静的リストに代わるもの)
            if (isSelected)
            {
                _selectedCardData.Add(clickedView.CardData);
            }
            else
            {
                _selectedCardData.Remove(clickedView.CardData);
            }

            // 3. Viewに描画更新を命令 (Viewの持つ責務をPresenterが実行指示)
            clickedView.UpdateVisuals();

            Debug.Log($"Presenter: Card {clickedView.CardData.Id} Toggled Selection. Count: {_selectedCardData.Count}");
        }

        /// <summary>
        /// Viewからのダブルクリックイベントを処理し、カードの裏返しロジックを実行します。
        /// </summary>
        /// <param name="clickedView">ダブルクリックされたCardDisplay (View)。</param>
        public void HandleDoubleClick(CardDisplay clickedView)
        {
            // 1. 必ず先にHandleSingleClickが呼ばれ、意図した選択状態が反転しているため、再度反転させる
            //bool isSelected = !clickedView.CardData.isSelected;
            //clickedView.CardData.isSelected = isSelected;
            HandleSingleClick(clickedView);

            // 2. Model (CardData) の状態を変更 (裏返しロジック)
            if (clickedView.CardData.State == CardData.CardState.FACE_UP)
            {
                clickedView.CardData.State = CardData.CardState.FACE_DOWN_ALL;
            }
            else
            {
                clickedView.CardData.State = CardData.CardState.FACE_UP;
            }

            // 3. Viewに描画更新を命令 (裏返ったことを反映)
            clickedView.UpdateVisuals();

            Debug.Log($"Presenter: Card {clickedView.CardData.Id} Flipped. New State: {clickedView.CardData.State}");
        }

        /// <summary>
        /// 右クリックイベントを処理し、該当するCardStackを特定してメニュー表示を指示します。
        /// </summary>
        public void HandleRightClick(CardDisplay clickedView)
        {
            // 1. クリックされたCardDataを取得
            CardData clickedData = clickedView.cardData; // CardDisplayからCardDataを取得

            // 2. Model層のDictionaryを検索し、このCardDataを含むCardStackを見つける
            var targetStackEntry = _cardStacks
                .FirstOrDefault(kv => kv.Value.Cards.Contains(clickedData));

            // 3. スタックの存在確認と、特殊操作（シャッフル）の実行条件の確認
            // Keyがnullの場合、一致するCardStackが存在しない
            // 要件: スタックでありさえすればシャッフル/ドロー可能 (スタックの定義は2枚以上)
            if (targetStackEntry.Key == null)
            {
                Debug.LogWarning($"Right Click: Card {clickedData.Id} is not part of any CardStack (単体カード)。Menu not displayed.");
                return; // ★スタックが見つからなかったため、ここで処理を終了
            }

            CardStack targetStack = targetStackEntry.Value;

            // 2. メニュー表示の準備
            _currentMenuStackId = targetStack.Id; // 現在操作対象のスタックIDを保持

            // 3. メニュー項目を定義
            MenuItem shuffleItem = new MenuItem
            {
                label = "シャッフル",
                // シャッフルがクリックされたら、Presenterのシャッフルメソッドを実行するActionを定義
                onClick = () => HandleShuffleStack(_currentMenuStackId)
            };

            // 4. メニューViewに表示を指示 (ワールド座標を画面座標に変換する必要がある)
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(clickedView.transform.position);

            // ログを追加して実行を確認
            Debug.Log($"[DEBUG] Pop-up Menu Show() command sent at screen position: {screenPosition}");

            _popupMenu.Show(screenPosition, new MenuItem[] { shuffleItem });
        }

        /// <summary>
        /// ポップアップメニューがメニュー外クリックなどで閉じられたときの処理。
        /// </summary>
        private void HandleMenuCancel()
        {
            Debug.Log("Popup Menu Cancelled.");
        }

        /// <summary>
        /// Viewからのドラッグ開始イベントを処理します。
        /// ドラッグ開始カードが未選択の場合は選択状態にし、全選択カードのZ値を最前面に設定します。
        /// </summary>
        public void HandleDragStart(CardDisplay startingView, Vector3 initialWorldPosition)
        {
            // 1. ドラッグ開始カードが未選択なら、グループ移動の前にそのカードを選択する (旧OnPointerDownロジック)
            // ★修正ポイント★: トグルではなく、「未選択であれば選択状態にする」ロジックに限定
            //if (!startingView.CardData.isSelected)
            //{
            //    // HandleSingleClickを直接呼び出すのではなく、選択ロジックを適用
            //    // HandleSingleClickはトグル機能を持つため、直接呼び出すと状態が反転しすぎる問題がある。

            //    // 選択ロジックを直接実行 (HandleSingleClick内の選択部分のみを抽出)
            //    startingView.CardData.isSelected = true; // 確実に選択状態にする
            //    _selectedCardData.Add(startingView.CardData);
            //    startingView.UpdateVisuals();

            //    Debug.Log($"Logic: Auto-Selected Card {startingView.CardData.Id} for dragging.");
            //}

            // 2. 選択カード全てを最前面に設定する（旧OnPointerDownのロジックをPresenterで再現）
            // Presenterが管理する選択中CardDataに対応するViewを取得
            var selectedViews = _views.Where(v => _selectedCardData.Contains(v.CardData));

            foreach (var view in selectedViews)
            {
                // Z値を一時的に最前面Z値に設定するようViewに命令
                // Z値の変更はViewの責務（SetPosition）を通じて実行
                view.SetPosition(new Vector3(
                    view.CardData.Position.x, // 現在のX/Y位置を維持
                    view.CardData.Position.y,
                    -0.1f // 最前面Z値
                ));
            }

            Debug.Log($"Presenter: Drag Started. Selected count: {_selectedCardData.Count}");
        }

        /// <summary>
        /// Viewからのドラッグ中イベントを処理し、選択グループ全体を移動させます。
        /// </summary>
        /// <param name="draggedView">現在ドラッグされているCardDisplay (View)。</param>
        /// <param name="moveDelta">Viewが移動した量（X, Yの差分）。</param>
        public void HandleDragging(CardDisplay draggedView, Vector3 moveDelta)
        {
            // 1. Viewが自分自身を移動済みなので、Modelのデータも更新する
            // Viewは自分自身を移動済みのため、Presenterはデータ層を同期させる
            draggedView.CardData.Position = draggedView.transform.position;

            // 2. 選択カード全てを移動させる（自分自身を除く）
            var selectedViews = _views.Where(v => _selectedCardData.Contains(v.CardData));

            foreach (var view in selectedViews)
            {
                // ドラッグを開始したカードはすでに移動済みなのでスキップ
                if (view == draggedView) continue;

                // 他のカードは移動差分を加算し、Viewに位置更新を命令
                Vector3 newPosition = view.CardData.Position + moveDelta;

                // Modelの位置を更新 (データ永続化のため)
                view.CardData.Position = newPosition;

                // Viewの描画位置を更新
                view.SetPosition(newPosition);
            }
        }

        /// <summary>
        /// Viewからのドラッグ終了イベントを処理します。
        /// 選択状態を解除し、Viewに再描画を命令します。
        /// </summary>
        public void HandleEndDrag(CardDisplay droppedView)
        {
            // 1. ドラッグ終了後、全カードの選択状態を解除（旧OnEndDragのロジック）
            var cardsToDeselect = _selectedCardData.ToList(); // 処理中にリストが変更されるのを防ぐ

            foreach (var data in cardsToDeselect)
            {
                // Modelの状態を更新
                data.isSelected = false;

                // Presenterのリストから削除
                _selectedCardData.Remove(data);

                // 対応するViewを取得し、描画を更新（選択解除とZ-Index復元）
                var view = _views.FirstOrDefault(v => v.CardData == data);
                if (view != null) // 安全チェックを追加
                {
                    view.UpdateVisuals();
                }
                else
                {
                    // Viewが存在しない場合はログを出力（Viewが先に破棄された場合の対処）
                    Debug.LogWarning($"HandleEndDrag: View not found for CardData {data.Id}. Skipping visuals update.");
                }
            }

            Debug.Log($"Presenter: Drag Ended. All {_selectedCardData.Count} cards deselected.");
        }

        /// <summary>
        /// 指定されたスタックIDのカードをシャッフルします。
        /// この操作はPopupMenuからのみ呼び出されることを想定しています。
        /// </summary>
        /// <param name="stackId">シャッフル対象のCardStackのID。</param>
        public void HandleShuffleStack(string stackId)
        {
            CardStack stack = GetStack(stackId);

            // 【普遍的なロジック】: 存在しないスタックや、シャッフルの意味がない1枚以下のスタックは操作しない。
            // ※スタックは2枚以上で存在することになっているが、安全のためチェック
            if (stack == null || stack.Cards.Count <= 1)
            {
                Debug.LogWarning($"Shuffle failed: Stack '{stackId}' is null or has {stack?.Cards.Count ?? 0} cards.");
                return;
            }

            // ToDo: シャッフルロジックを実装
            Debug.Log($"Shuffling stack: {stack.Id} with {stack.Cards.Count} cards.");

            // ViewのZIndexやPositionの更新も必要（最上部のカードのみ見え方が変わるなど）
        }

        /// <summary>
        /// 指定されたスタックIDの最上部カードをPopし、場に出すドロー操作を実行します。
        /// </summary>
        /// <param name="sourceStackId">Popする元のCardStackのID。</param>
        public void HandleDrawFromStack(string sourceStackId)
        {
            CardStack sourceStack = GetStack(sourceStackId);

            if (sourceStack == null || sourceStack.Cards.Count == 0)
            {
                Debug.LogWarning($"Draw failed: Stack '{sourceStackId}' is empty or not found.");
                return;
            }

            // 1. Model更新：スタックから最上部カードを取り出す
            CardData drawnCard = sourceStack.Pop();

            // 2. ToDo: Viewの移動と状態更新（裏返し、新しいPosition/ZIndexの設定）

            // 3. ToDo: スタックの解体処理
            // Popの結果、カード枚数が1枚以下になった場合、PresenterはModelからこのスタックをDictionaryから削除する
            if (sourceStack.Cards.Count < 2)
            {
                Debug.Log($"CardStack '{sourceStackId}' dissolved because count dropped to {sourceStack.Cards.Count}.");
                // _cardStacks.Remove(sourceStackId); // 実際に削除

                // 残ったカード（もし1枚あれば）は、単なる「場の一枚のカード」として扱われる
            }
        }

        // ★★★ ユニットテスト用アクセスポイント ★★★
#if UNITY_EDITOR
        /// <summary>
        /// 【テスト専用】内部の選択カードリストに特定のCardDataが含まれているか確認します。
        /// </summary>
        /// <param name="data">確認したいCardData。</param>
        public bool IsCardSelected(CardData data) => _selectedCardData.Contains(data);

        /// <summary>
        /// 【テスト専用】テストのセットアップのため、内部リストにカードを強制的に追加します。
        /// </summary>
        /// <param name="data">強制的に選択状態にするCardData。</param>
        public void ForceAddCardToSelectionForTesting(CardData data)
        {
            if (!_selectedCardData.Contains(data))
            {
                _selectedCardData.Add(data);
            }
        }

        /// <summary>
        /// 【テスト専用】テストのクリーンアップのため、内部リストをクリアします。
        /// </summary>
        public void ClearSelectionForTesting()
        {
            _selectedCardData.Clear();
        }
#endif
    }
}