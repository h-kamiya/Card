using System;
using UnityEngine;

/// <summary>
/// CardDisplay (View) の公開インターフェース。Presenterはこれを通じてViewを操作する。
/// </summary>
public interface ICardDisplay
{
    // ViewがPresenterに「入力」を報告するためのイベント
    // どのカードで何が起きたかを通知する（Presenterが購読する）
    event Action<CardDisplay> OnCardSingleClick;
    event Action<CardDisplay> OnCardDoubleClick;
    event Action<CardDisplay, Vector3> OnCardDragStart; // Vector3: ドラッグ開始時の位置など
    event Action<CardDisplay, Vector3> OnCardDragging; // Vector3: 移動差分など

    // PresenterからViewに「描画更新」を命令するためのメソッド
    void UpdateVisuals();
    void SetPosition(Vector3 position);

    // 現在はCardDisplayのプロパティとしてアクセスされるデータを公開
    CardData CardData { get; }
}