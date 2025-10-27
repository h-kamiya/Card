// PopupMenu.cs (Assets/Scripts/Views)

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Views
{
    /// <summary>
    /// 右クリックなどで出現するコンテキストメニューのUI View実装です。
    /// IPopupMenuインターフェースを実装し、項目の動的な生成とクリックイベントの処理を行います。
    /// </summary>
    public class PopupMenu : MonoBehaviour, IPopupMenu
    {
        [Header("UI設定")]
        /// <summary>
        /// メニュー項目（Button）のPrefab。
        /// </summary>
        public GameObject buttonPrefab;

        /// <summary>
        /// メニュー全体のコンテナとなるRectTransform。
        /// </summary>
        public RectTransform menuPanel;

        /// <summary>
        /// メニューが表示中に、メニュー項目以外の場所がクリックされたときに発生します。
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// フレームごとにメニュー外クリックを監視します。
        /// </summary>
        void Update()
        {
            // ... (Updateロジックは省略)
            if (Input.GetMouseButtonDown(0))
            {
                if (gameObject.activeSelf)
                {
                    Hide();
                    OnCancel?.Invoke();
                }
            }
        }

        /// <summary>
        /// メニューを指定された画面座標に表示し、メニュー項目を動的に生成してバインドします。
        /// </summary>
        /// <param name="screenPosition">メニューを表示する画面上の座標。</param>
        /// <param name="items">メニューに表示する項目の配列。</param>
        public void Show(Vector3 screenPosition, MenuItem[] items)
        {
            // 既存のボタンをクリア
            foreach (Transform child in menuPanel)
            {
                Destroy(child.gameObject);
            }

            // 画面座標 (ワールド座標ではない) でメニューを表示
            transform.position = screenPosition;

            // メニュー項目を動的に生成
            foreach (var item in items)
            {
                // ボタンをインスタンス化
                GameObject buttonObject = Instantiate(buttonPrefab, menuPanel);
                Button button = buttonObject.GetComponent<Button>();

                // テキストを設定
                Text buttonText = buttonObject.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = item.label;
                }

                // ボタンのOnClickイベントに関数を登録
                button.onClick.AddListener(() =>
                {
                    item.onClick?.Invoke(); // 登録された処理（シャッフルロジック）を実行
                    Hide(); // 実行後、メニューを非表示にする
                });
            }

            gameObject.SetActive(true); // メニューコンテナを表示
        }

        /// <summary>
        /// メニューを非表示にします。
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}