using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;
using Game.Presenters;
using Game.Views;
using Game.Models; // FindObjectsOfTypeAllを使用するため

// CardInteractionPresenterが存在するアセンブリをusing
// using Game.Presenters; // アセンブリ名に応じて変更
// using Game.Models; // CardDataが存在するアセンブリ名に応じて変更

// テストクラス自体も名前空間に含めることが推奨されます
namespace Game.Tests
{
    public class CardInteractionPresenterTests
    {
        private CardInteractionPresenter _presenter;
        private List<ICardDisplay> _mockViews;

        // テスト中に作成したGameObjectを格納するリスト
        private List<GameObject> _createdGameObjects = new List<GameObject>();

        /// <summary>
        /// 各テストの実行前に呼ばれ、テスト環境（ロジックインスタンス）を準備します。
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // 1. モック/ダミーのViewリストを準備
            _mockViews = new List<ICardDisplay>();

            // 2. CardInteractionPresenterのインスタンス化 (テスト対象の準備)
            _presenter = new CardInteractionPresenter(_mockViews);
        }

        /// <summary>
        /// 各テストの実行後に呼ばれ、作成したGameObjectとロジックをクリーンアップします。
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            // 1. テスト中に作成したGameObjectを破棄
            foreach (var go in _createdGameObjects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            _createdGameObjects.Clear();

            // 2. ロジックの内部状態をリセット
            _presenter.ClearSelectionForTesting();
            _presenter = null;
        }

        // --- ユニットテストの例 ---

        /// <summary>
        /// 未選択のカードをシングルクリックした場合、選択状態になり、内部リストに追加されることを検証します。
        /// </summary>
        [Test]
        public void HandleSingleClick_NotSelected_ShouldSelectAndAddToList()
        {
            // Arrange
            var cardData = new CardData { Id = "1", isSelected = false };
            var mockView = CreateMockCardDisplay(cardData);

            // Act
            _presenter.HandleSingleClick(mockView);

            // Assert
            // 1. モデルの状態が更新されたか
            Assert.IsTrue(cardData.isSelected, "カードの選択状態がTrueになっていません。");

            // 2. Presenterが管理する内部リストに追加されたか
            Assert.IsTrue(_presenter.IsCardSelected(cardData), "内部選択リストにカードが追加されていません。");
        }

        /// <summary>
        /// 選択済みのカードをシングルクリックした場合、非選択状態になり、内部リストから削除されることを検証します。
        /// </summary>
        [Test]
        public void HandleSingleClick_IsSelected_ShouldDeselectAndRemoveFromList()
        {
            // Arrange
            var cardData = new CardData { Id = "2", isSelected = true };
            var mockView = CreateMockCardDisplay(cardData);

            // テストの隔離のため、初期状態で内部リストにカードがある状態を再現
            _presenter.ForceAddCardToSelectionForTesting(cardData);

            // Act
            _presenter.HandleSingleClick(mockView);

            // Assert
            // 1. モデルの状態が更新されたか
            Assert.IsFalse(cardData.isSelected, "カードの選択状態がFalseになっていません。");

            // 2. Presenterが管理する内部リストから削除されたか
            Assert.IsFalse(_presenter.IsCardSelected(cardData), "内部選択リストからカードが削除されていません。");
        }

        /// <summary>
        /// 裏面（FACE_DOWN_ALL）のカードをダブルクリックした場合、表面（FACE_UP）になることを検証します。
        /// </summary>
        [Test]
        public void HandleDoubleClick_FaceDown_ShouldFlipUp()
        {
            // Arrange
            var cardData = new CardData { Id = "3", State = CardData.CardState.FACE_DOWN_ALL };
            var mockView = CreateMockCardDisplay(cardData);

            // Act
            _presenter.HandleDoubleClick(mockView);

            // Assert
            Assert.AreEqual(CardData.CardState.FACE_UP, cardData.State, "カードの状態がFACE_UPになっていません。");
        }

        // --- ヘルパーメソッド ---

        /// <summary>
        /// CardDisplay (MonoBehaviour)をテストするために、GameObjectとコンポーネントを一時的に生成します。
        /// </summary>
        private CardDisplay CreateMockCardDisplay(CardData data)
        {
            // 1. GameObjectの作成とリストへの追加
            var go = new GameObject($"TestCard_{data.Id}");
            _createdGameObjects.Add(go);

            // 2. CardDisplayコンポーネントをアタッチ
            var display = go.AddComponent<CardDisplay>();

            // 3. SpriteRendererコンポーネントをアタッチ
            var renderer = go.AddComponent<SpriteRenderer>(); // ★追加: SpriteRendererを取得★

            // 4. CardDisplayのフィールドにSpriteRendererを割り当て (NullReference対策)
            display.spriteRenderer = renderer; // ★修正: ここでフィールドに割り当てる★
            // UpdateVisualsに必要なSpriteRendererも追加 (UpdateVisualsが呼ばれるため) // ←このコメントはもう不要

            // 5. ICardDisplayで公開されているプロパティを初期化
            display.cardData = data;

            // 6. Viewリストにも追加
            _mockViews.Add(display);

            return display;
        }
    }
}