using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// カードのクリック、選択、ドラッグなどの相互作用に関するゲームロジックを担当するPresenter。
/// </summary>
public class CardInteractionPresenter
{
    private readonly List<ICardDisplay> _views;

    // Presenterが管理する選択中カードのリスト (以前の静的リストに代わるもの)
    private readonly List<CardData> _selectedCardData = new List<CardData>();

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

    private void HandleSingleClick(CardDisplay clickedView)
    {
        // Viewから入力があったことを受け取り、ロジックを実行する（トグル処理など）
        // clickedView.CardData.isSelected = !clickedView.CardData.isSelected;
        // clickedView.UpdateVisuals();

        Debug.Log($"Presenter: Card {clickedView.CardData.Id} Single Clicked.");
    }

    // 他のイベントハンドラ（ダブルクリック、ドラッグなど）もここに追加する
}