using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// MVPパターンにおけるPresenter層。
/// Viewからの入力イベントを購読し、Model（CardData）の状態を変更するゲームロジックの中核を担います。
/// カードの選択、裏返し、グループ移動などの相互作用ロジックを管理します。
/// </summary>
public class CardInteractionPresenter
{
    // ★重要フィールド★: Viewのリスト
    private readonly List<ICardDisplay> _views;

    // Presenterが管理する選択中カードのリスト (以前の静的リストに代わるもの)
    /// <summary>
    /// Presenterがゲームロジックに基づいて管理する、現在選択中のCardDataのリスト。
    /// このリストはViewの静的リストに代わるものであり、テスタブルなロジックを提供します。
    /// </summary>
    private readonly List<CardData> _selectedCardData = new List<CardData>();

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
            // ★実装: ViewからのクリックイベントをPresenterのハンドラに接続
            view.OnCardSingleClick += HandleSingleClick;
            view.OnCardDoubleClick += HandleDoubleClick;
            // view.OnCardDragStart など、他のイベントの購読もここで行う
        }
    }

    /// <summary>
    /// Viewからのシングルクリックイベントを処理し、選択状態をトグルするロジックを実行します。
    /// </summary>
    /// <param name="clickedView">クリックされたCardDisplay (View)。</param>
    private void HandleSingleClick(CardDisplay clickedView)
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
    private void HandleDoubleClick(CardDisplay clickedView)
    {
        // 1. 必ず先にHandleSingleClickが呼ばれ、意図した選択状態が反転しているため、再度反転させる
        bool isSelected = !clickedView.CardData.isSelected;
        clickedView.CardData.isSelected = isSelected;

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
}