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
            view.OnCardSingleClick += HandleSingleClick;
            // 他のイベントの購読もここで行う
        }
    }

    /// <summary>
    /// Viewからのシングルクリックイベントを処理し、選択状態をトグルするロジックを実行します。
    /// </summary>
    /// <param name="clickedView">クリックされたCardDisplay (View)。</param>
    private void HandleSingleClick(CardDisplay clickedView)
    {
        // Viewから入力があったことを受け取り、ロジックを実行する（トグル処理など）
        // clickedView.CardData.isSelected = !clickedView.CardData.isSelected;
        // clickedView.UpdateVisuals();

        Debug.Log($"Presenter: Card {clickedView.CardData.Id} Single Clicked.");
    }

    // 他のイベントハンドラ（ダブルクリック、ドラッグなど）もここに追加する
}