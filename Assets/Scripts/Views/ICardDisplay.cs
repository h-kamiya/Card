using System;
using UnityEngine;

using Game.Models; // CardDataを参照するため

namespace Game.Views
{
    /// <summary>
    /// CardDisplay (View) の公開インターフェース。Presenterはこれを通じてViewを操作する。
    /// </summary>
    public interface ICardDisplay
    {
        // ViewがPresenterに「入力」を報告するためのイベント
        // どのカードで何が起きたかを通知する（Presenterが購読する）
        /// <summary>
        /// カードがシングルクリックされた際に発火します。
        /// Presenterがこのイベントを購読し、選択ロジックを実行します。
        /// </summary>
        event Action<CardDisplay> OnCardSingleClick;

        /// <summary>
        /// カードがダブルクリックされた際に発火します。
        /// Presenterがこのイベントを購読し、裏返しロジックを実行します。
        /// </summary>
        event Action<CardDisplay> OnCardDoubleClick;

        /// <summary>
        /// カードが右クリックされた際に発火します。
        /// Presenterがこのイベントを購読し、ポップアップメニュー表示ロジックを実行します。
        /// </summary>
        event Action<CardDisplay> OnCardRightClick;

        /// <summary>
        /// カードのドラッグが開始された際に発火します。
        /// <param name="Vector3">ドラッグ開始時のワールド座標。</param>
        /// </summary>
        event Action<CardDisplay, Vector3> OnCardDragStart; // Vector3: ドラッグ開始時の位置など

        /// <summary>
        /// カードがドラッグ中に移動した際に発火します。
        /// <param name="Vector3">現在のワールド座標。</param>
        /// </summary>
        event Action<CardDisplay, Vector3> OnCardDragging; // Vector3: 移動差分など

        /// <summary>
        /// カードがドラッグ終了した際に発火します。
        /// </summary>
        event Action<CardDisplay> OnCardEndDrag;

        // PresenterからViewに「描画更新」を命令するためのメソッド
        /// <summary>
        /// Presenterからの指示に基づき、CardDataの状態を反映して視覚要素を更新します。
        /// </summary>
        void UpdateVisuals();

        /// <summary>
        /// Presenterからの指示に基づき、カードのワールド座標を設定します。
        /// </summary>
        /// <param name="position">設定するワールド座標。</param>
        void SetPosition(Vector3 position);

        // 現在はCardDisplayのプロパティとしてアクセスされるデータを公開
        /// <summary>
        /// このViewに対応するデータモデルの参照を取得します。
        /// PresenterがViewからデータを読み取るために使用します。
        /// </summary>
        CardData CardData { get; }
    }
}