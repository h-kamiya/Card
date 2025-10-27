using System;
using UnityEngine;

namespace Game.Views
{
    /// <summary>
    /// ポップアップメニューの単一の項目を表す構造体です。
    /// </summary>
    public struct MenuItem
    {
        /// <summary>
        /// メニューに表示されるテキストラベル。
        /// </summary>
        public string label;

        /// <summary>
        /// 項目がクリックされたときに実行される処理。Presenterのロジックがバインドされます。
        /// </summary>
        public Action onClick;
    }

    /// <summary>
    /// PresenterがUIのポップアップメニューを表示・非表示させるために使用するインターフェースです。
    /// </summary>
    public interface IPopupMenu
    {
        /// <summary>
        /// メニューを指定された画面座標に表示し、メニュー項目をバインドします。
        /// </summary>
        /// <param name="screenPosition">メニューを表示する画面上の座標。</param>
        /// <param name="items">メニューに表示する項目の配列。</param>
        void Show(Vector3 screenPosition, MenuItem[] items);

        /// <summary>
        /// メニューを非表示にします。
        /// </summary>
        void Hide();

        /// <summary>
        /// メニューが表示中に、メニュー項目以外の場所がクリックされたときに発生します。
        /// Presenterにキャンセル処理を通知するために使用されます。
        /// </summary>
        event Action OnCancel;
    }
}