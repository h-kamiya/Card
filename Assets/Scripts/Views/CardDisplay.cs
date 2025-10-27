using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // ★追加：イベントシステムを使用
using Game.Models; // CardDataを参照するため

namespace Game.Views
{
    /// <summary>
    /// カードの表示（ビュー）とユーザーインタラクションを管理するコンポーネントです。
    /// IPointer*Handlerインターフェースを実装し、クリックやドラッグイベントを処理します。
    /// </summary>
    // IPointerDownHandler: クリック/タップ開始
    // IDragHandler: ドラッグ中
    // IPointerClickHandler: クリック/タップ完了
    // IEndDragHandler: ドラッグ終了
    public class CardDisplay : MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerClickHandler,
        IEndDragHandler,
        ICardDisplay
    {
        [Header("アセットの参照")]
        // 2Dの場合、SpriteRendererで画像を表示
        public SpriteRenderer spriteRenderer;
    
        // 用意したテクスチャアセットをUnityエディタから設定
        public Sprite faceSprite; // 表面の画像 (スペードのAなど)
        public Sprite backSprite; // 裏面の画像 (共通の裏面)
    
        [HideInInspector]
        public CardData cardData; // このゲームオブジェクトに対応するCardDataインスタンス
    
        // 画面に表示するテキストコンポーネント (オプション。今回は省略可)
        // public TextMeshPro textDisplay; 
        // ★追加: シーン全体で選択されたカードのリストを管理
        // ★追加: シーン全体で選択されたカードのリストを管理
        /// <summary>
        /// 現在シーン内で選択されている全てのCardDisplayインスタンスを格納する静的リストです。
        /// まとめてドラッグする際に利用されます。
        /// </summary>
        public static List<CardDisplay> SelectedCards = new List<CardDisplay>();
        private Vector3 initialDragPosition; // ドラッグ開始時の位置を記憶
    
        // ドラッグ処理用の変数
        private Vector3 dragOffset; // ドラッグ開始時のマウス/オブジェクトの相対位置
        private bool isDragging = false; // ドラッグ中かどうかのフラグ
    
    
        // CardDisplay.cs 内に追加
    
        // ICardDisplayインターフェースで要求されるイベントの定義
        // Presenterはこのイベントを購読する
        public event Action<CardDisplay> OnCardSingleClick;
        public event Action<CardDisplay> OnCardDoubleClick;
        public event Action<CardDisplay> OnCardRightClick;
        public event Action<CardDisplay, Vector3> OnCardDragStart;
        public event Action<CardDisplay, Vector3> OnCardDragging;
        public event Action<CardDisplay> OnCardEndDrag; // ★追加: ドラッグ終了イベント

        // ICardDisplayインターフェースで要求されるプロパティの定義
        // Presenterがカードデータを参照するために使用
        public CardData CardData => cardData; // 既存の cardData フィールドを参照
    
        // ICardDisplayインターフェースで要求されるメソッドの実装
        // Presenterから位置を設定されるためのメソッド
        public void SetPosition(Vector3 position)
        {
            // Viewの責務である位置の設定
            transform.position = position;
        }
    
        // NOTE: UpdateVisuals()は既に存在するため、ここでは再実装不要。
        // --- 初期化と描画 ---
    
        /// <summary>
        /// カードゲームオブジェクトを初期化し、対応するデータ（モデル）を関連付けます。
        /// </summary>
        /// <param name="data">このカードに関連付けるCardDataインスタンス。</param>
        public void Initialize(CardData data)
        {
            this.cardData = data;
            // データIDに基づいてSpriteを設定するロジック (例: Resources.Load<Sprite>(data.Id))
            // 今回は簡略化のため、Initialize前に手動で 'faceSprite' を設定するものとします。
    
            // 初回描画
            UpdateVisuals();
        }
    
        // モデルの状態に基づいて表示を更新する関数
        /// <summary>
        /// CardDataの状態に基づき、カードの視覚的な表示を更新します。
        /// スプライト（裏表）の切り替え、選択状態の色反映、Z座標（重ね順）の設定を行います。
        /// </summary>
        public void UpdateVisuals()
        {
            // 1. 画像 (裏表) の切り替え
            if (cardData.State == CardData.CardState.FACE_UP)
            {
                spriteRenderer.sprite = faceSprite;
                // textDisplay.text = cardData.Text; // テキストも表示
            }
            else // FACE_DOWN_ALL の場合
            {
                spriteRenderer.sprite = backSprite;
                // textDisplay.text = "???"; // 裏面表示
            }
    
            // ★追加: 選択状態の初期描画を反映
            spriteRenderer.color = cardData.isSelected ? Color.yellow : Color.white;
    
            // 2. 位置と重ね順の更新
            // transform.position = cardData.Position; // 移動はドラッグで直接行うため、ここではZIndexのみ反映
    
            // ZIndexに基づいたZ座標の適用
            // ZIndexは重ね順を決めるための値。Z値が大きいほど手前に表示されます。
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                -cardData.ZIndex * 0.01f // ZIndexが高いほどZ値は手前(小さい値)になるよう調整
            );
    
            // 3. 選択状態の視覚的なフィードバック (例: アウトライン表示など)
            // if (cardData.isSelected) { ... }
        }
    
        // CardDisplay.cs 内に追加
        /// <summary>
        /// **ユーザー操作: マウスの左ボタンが押された瞬間 / タッチが開始された瞬間**
        /// <para>ドラッグ操作の準備を行います。</para>
        /// <list type="bullet">
        /// <item>一時的にZ軸を最前面（-0.1f）に移動させます。</item>
        /// <item>ドラッグオフセットを計算します。</item>
        /// <item>カードが未選択の場合、この時点で選択状態（黄色）にします。</item>
        /// <item>全ての選択カードをZ軸の最前面に移動させます。</item>
        /// </list>
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            // 1. Z-Indexを最前面にする（修正済みロジック）
            // ... (既存のZ軸設定ロジックは維持: transform.position.z = -0.1f;)
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                -0.1f
            );
    
            // 2. ドラッグオフセットを計算 (ここは変更なし)
            dragOffset = transform.position - Camera.main.ScreenToWorldPoint(eventData.position);
            isDragging = false;
    
            // ★削除★: 選択されていないカードをドラッグ開始した場合、まず選択するロジック
            // ★削除★: 選択カード全てを一時的に最前面にするロジック
    
            // ★追加★: Presenterにドラッグ開始を通知
            OnCardDragStart?.Invoke(this, transform.position); // このカードでドラッグが開始されたことを通知
    
            // NOTE: initialDragPositionのフィールドは不要になったため削除して構いません。
        }
    
        /// <summary>
        /// **ユーザー操作: マウスの左ボタンを押したままカーソルが移動している間**
        /// <para>カードをカーソルに追従させます。カードが選択状態にある場合、SelectedCardsリスト内の全てのカードを同時に移動させます（まとめてドラッグ）。</para>
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            { 
                return;
            }
    
            isDragging = true;
    
            // 1. 現在のカードの新しい位置を計算 (Viewの責務)
            Vector3 curScreenPoint = eventData.position;
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + dragOffset;
            
            // 2. 移動量 (差分) を計算
            Vector3 moveDelta = newPosition - transform.position;
    
            // 3. 自分の位置のみを更新 (暫定。Presenterが全体を制御するまでは自身で移動)
            transform.position = newPosition; 
    
            // ★削除★: 選択されている全てのカードを移動するロジック
            
            // ★追加★: Presenterに移動情報を通知
            // Presenterはこれをトリガーに、他の選択カードを移動させる
            OnCardDragging?.Invoke(this, moveDelta);
        }
    
        // CardDisplay.cs の OnEndDrag を修正
    
        /// <summary>
        /// **ユーザー操作: マウスの左ボタンが離された瞬間（ドラッグ操作後）**
        /// <para>ドラッグ操作の終了を処理します。</para>
        /// <list type="bullet">
        /// <item>isDraggingフラグをリセットします。</item>
        /// <item>※このメソッドでは、選択状態の解除は行いません（シングルクリックで解除するため）。</item>
        /// </list>
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
    
            // ★修正★: Presenterにドロップ終了を通知
            OnCardEndDrag?.Invoke(this);
        }
    
        // CardDisplay.cs 内に追加
        /// <summary>
        /// **ユーザー操作: マウスの左ボタンが押されてすぐに離された瞬間（ドラッグと判定されない場合）**
        /// <para>クリック回数に基づき、選択または裏返し操作を実行します。</para>
        /// <list type="bullet">
        /// <item>ドラッグ操作であった場合は、このイベントを無視します。</item>
        /// <item>クリック回数 == 2 (ダブルクリック): 選択状態を変更せず、カードの裏表を反転します。（要件④）</item>
        /// <item>クリック回数 == 1 (シングルクリック): カードの選択状態をトグル（選択/非選択）します。（要件①②）</item>
        /// </list>
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // ドラッグ操作だった場合は、クリックイベントを無視する
            if (isDragging)
            {
                return;
            }
    
            // 左クリック (Primary action) のみ処理
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // ① ダブルクリックで裏返しをトグル
                if (eventData.clickCount == 2)
                {
                    Debug.Log($"eventData.clickCount == 2");
                    // ★修正★: ロジックを削除し、Presenterにイベントを通知
                    OnCardDoubleClick?.Invoke(this); // イベント発火
    
                    // 処理を終了 (選択状態の変更は行わない)
                    return;
                }
    
                // ② シングルクリックで選択状態をトグル
                if (eventData.clickCount == 1)
                {
                    Debug.Log($"eventData.clickCount == 1");
                    // ★修正★: ロジックを削除し、Presenterにイベントを通知
                    OnCardSingleClick?.Invoke(this); // イベント発火
                }
            }
            // ③ 右クリック（選択ロジックの代替）: 今回はシングルクリックで実装したので不要だが、残す場合は維持
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log($"Card {cardData.Id} was right-clicked/long-pressed.");
                OnCardRightClick?.Invoke(this);
                // 右クリックはダブルクリックではないため、eventData.clickCountの確認は不要
                // 処理を終了
                return;
            }
        }
    }
}