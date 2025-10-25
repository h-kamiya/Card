// TestInitializer.cs (または GameInitializer.cs)

using System.Collections.Generic;
using Game.Models;
using Game.Views;
using Game.Presenters; // ★追加: Presenterの参照
using UnityEngine;

// クラス名を TestInitializer から GameInitializer に変更することを推奨します。
public class GameInitializer : MonoBehaviour
{
    public CardDisplay cardPrefab; // Card PrefabをInspectorから割り当てる
    [Header("カード配置設定")]
    public Vector3 startPosition = new Vector3(-2f, 0, 0); // 最初のカードの開始位置 (x=-2, y=0)
    public float cardSpacing = 1.2f; // カード間のX軸間隔
    public int numberOfCards = 5; // 生成するカードの枚数

    private CardInteractionPresenter _presenter;

    void Start()
    {
        // 1. カードデータとViewを生成・初期化
        List<ICardDisplay> views = InitializeCards();

        // 2. Presenterを初期化し、Viewリストを渡してMVP接続を完了
        // (PresenterはViewリストを購読し、イベント接続を確立します)
        if (views.Count > 0)
        {
            _presenter = new CardInteractionPresenter(views);
            Debug.Log($"Game Initialized: {views.Count} cards generated and Presenter connected.");
        }
        else
        {
            Debug.LogWarning("Card generation failed. No Presenter initialized.");
        }
    }

    /// <summary>
    /// 複数のCardDataとViewを生成し、リストを返します。
    /// </summary>
    private List<ICardDisplay> InitializeCards()
    {
        List<ICardDisplay> createdViews = new List<ICardDisplay>();

        // 仮のカードIDリスト (このIDに対応するSpriteをCardDisplayのInspectorに設定してください)
        List<string> cardIds = new List<string> { "S_A", "H_2", "D_3", "C_4", "S_5", "H_6" };

        // 実際に生成する枚数を調整
        int count = Mathf.Min(numberOfCards, cardIds.Count);

        for (int i = 0; i < count; i++)
        {
            // 1. 各カードの初期位置を計算 (X軸方向に間隔を空けて配置)
            Vector3 currentPosition = startPosition + new Vector3(i * cardSpacing, 0, 0);

            // 2. 新しいCardDataを作成 (Model)
            CardData newCardData = new CardData
            {
                Id = cardIds[i],
                // Textは今回は省略
                State = (i % 2 == 0) ? CardData.CardState.FACE_UP : CardData.CardState.FACE_DOWN_ALL, // 表面と裏面を交互に設定
                ZIndex = (10 - i), // 重ね順を設定 (右に行くほどZIndexが低く、奥になる)
                Position = currentPosition // 最初に計算した位置を設定
            };

            // 3. Prefabをインスタンス化 (View)
            CardDisplay newCard = Instantiate(cardPrefab, currentPosition, Quaternion.identity);

            // 4. CardDisplayを初期化
            newCard.Initialize(newCardData);

            // 5. Presenterに渡すViewリストに追加
            createdViews.Add(newCard);
        }

        return createdViews;
    }
}