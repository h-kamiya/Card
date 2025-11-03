using System.Collections.Generic;
using Game.Models;
using Game.Views;
using Game.Presenters;
using UnityEngine;
using System.Linq;

public class GameInitializer : MonoBehaviour
{
    public CardDisplay cardPrefab;

    [Header("初期スタック設定")]
    public Vector3 initialStackPosition = new Vector3(0, 0, 0); // 最初のスタック（山札）を置く位置
    public int totalCards = 6; // 初期スタックに含めるカードの枚数 (最低2枚以上を想定)

    /// <summary>
    /// PopupMenu ViewのPrefab。Inspectorから割り当てられます。
    /// </summary>
    public PopupMenu popupMenuPrefab;

    private CardInteractionPresenter _presenter;

    void Start()
    {
        // 1. 全ての初期CardStackを生成
        List<CardStack> initialStacks = InitializeStacks();

        // 2. Viewリストの準備（全てのCardDataに対応するViewを作成）
        List<ICardDisplay> views = CreateViewsForStacks(initialStacks);

        // 3. PopupMenu Viewのインスタンス化 (UI Viewの準備)
        PopupMenu popupMenuInstance = Instantiate(popupMenuPrefab);
        popupMenuInstance.Hide();

        // ★★★ 修正: シーンのルートCanvasを取得し、親に設定する ★★★
        Canvas rootCanvas = FindAnyObjectByType<Canvas>();

        if (rootCanvas != null)
        {
            // Canvasの子に設定。falseでスケールや回転をリセット
            popupMenuInstance.transform.SetParent(rootCanvas.transform, false);
        }

        // 4. Presenterを初期化し、Viewリストと全てのStackを渡してMVP接続を完了
        if (views.Count > 0 && popupMenuInstance != null)
        {
            /// <summary>
            /// Presenterの新しいコンストラクタを使用し、IPopupMenuを依存性注入します。
            /// </summary>
            _presenter = new CardInteractionPresenter(views, initialStacks, popupMenuInstance);

            Debug.Log($"Game Initialized: Total {views.Count} cards created. Managing {initialStacks.Count} stacks. Presenter connected.");
        }
        else
        {
            Debug.LogWarning("Card generation failed. No Presenter initialized.");
        }
    }

    /// <summary>
    /// 初期に必要なCardStack（山札）を生成し、その中に全てのCardDataを格納します。
    /// 捨て札など0枚のスタックはここでは生成しません。
    /// </summary>
    private List<CardStack> InitializeStacks()
    {
        List<CardStack> allStacks = new List<CardStack>();

        if (totalCards >= 2)
        {
            // 1. カードデータの生成
            List<string> cardIds = Enumerable.Range(1, totalCards).Select(i => $"Card_{i}").ToList();
            List<CardData> initialCardData = new List<CardData>();

            for (int i = 0; i < totalCards; i++)
            {
                CardData newCardData = new CardData
                {
                    Id = cardIds[i],
                    State = CardData.CardState.FACE_DOWN_ALL,
                    ZIndex = (int)(i + 1),
                    Position = initialStackPosition // 全てのカードは最初のスタックの位置にある
                };
                initialCardData.Add(newCardData);
            }

            // 2. スタックの作成 (ID="Deck"は便宜上の初期値)
            CardStack initialStack = new CardStack("Deck", initialCardData);
            allStacks.Add(initialStack);
        }

        return allStacks;
    }

    /// <summary>
    /// 全てのCardStack内のCardDataに対応するViewを生成します。
    /// </summary>
    private List<ICardDisplay> CreateViewsForStacks(List<CardStack> stacks)
    {
        List<ICardDisplay> createdViews = new List<ICardDisplay>();

        foreach (var stack in stacks)
        {
            // 全スタックの全カードに対してViewを生成
            for (int i = 0; i < stack.Cards.Count; i++)
            {
                CardData data = stack.Cards[i];

                // Prefabをインスタンス化（スタックの基準位置に重ねて生成）
                CardDisplay newCard = Instantiate(cardPrefab, data.Position, Quaternion.identity);

                newCard.Initialize(data);

                createdViews.Add(newCard);
            }
        }
        return createdViews;
    }
}