using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // ★追加：イベントシステムを使用

// IPointerDownHandler: クリック/タップ開始
// IDragHandler: ドラッグ中
// IPointerClickHandler: クリック/タップ完了
// IEndDragHandler: ドラッグ終了
public class CardDisplay : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerClickHandler,
    IEndDragHandler
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
    public static List<CardDisplay> SelectedCards = new List<CardDisplay>();
    private Vector3 initialDragPosition; // ドラッグ開始時の位置を記憶

    // ドラッグ処理用の変数
    private Vector3 dragOffset; // ドラッグ開始時のマウス/オブジェクトの相対位置
    private bool isDragging = false; // ドラッグ中かどうかのフラグ

    // --- 初期化と描画 ---

    public void Initialize(CardData data)
    {
        this.cardData = data;
        // データIDに基づいてSpriteを設定するロジック (例: Resources.Load<Sprite>(data.Id))
        // 今回は簡略化のため、Initialize前に手動で 'faceSprite' を設定するものとします。

        // 初回描画
        UpdateVisuals();
    }

    // 選択状態をトグルする関数
    public void ToggleSelection()
    {
        cardData.isSelected = !cardData.isSelected;

        if (cardData.isSelected)
        {
            // 選択リストに追加し、視覚的なフィードバックを有効化
            if (!SelectedCards.Contains(this))
            {
                SelectedCards.Add(this);
            }
            // ★視覚的なフィードバック（例: アウトライン、色変更）をここに実装
            spriteRenderer.color = Color.yellow; // 簡易的なハイライト
        }
        else
        {
            // 選択リストから削除し、視覚的なフィードバックを無効化
            SelectedCards.Remove(this);
            spriteRenderer.color = Color.white; // 通常の色に戻す
        }
        // TODO: ここでGameManagerを介して永続化処理を呼び出す
    }

    // モデルの状態に基づいて表示を更新する関数
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

        // ★追加: 選択状態の確認とドラッグ準備
        initialDragPosition = transform.position; // ドラッグ開始時の位置を記録

        // 選択されていないカードをドラッグ開始した場合、まず選択する
        if (!cardData.isSelected)
        {
            ToggleSelection();
        }

        // Z-Indexを最前面に (選択カード全てを一時的に最前面に)
        foreach (var card in SelectedCards)
        {
            card.transform.position = new Vector3(
                card.transform.position.x,
                card.transform.position.y,
                -0.1f // 全て-0.1fにすることで、ドラッグ中も重ね順を統一
            );
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;

        // 1. 現在のカードの新しい位置を計算
        Vector3 curScreenPoint = eventData.position;
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + dragOffset;

        // 2. 移動量 (差分) を計算
        Vector3 moveDelta = newPosition - transform.position;

        // 3. 選択されている全てのカードを移動
        foreach (var card in SelectedCards)
        {
            // 現在ドラッグしているカードは計算されたnewPositionに、他は移動差分を加算
            if (card == this)
            {
                card.transform.position = newPosition;
            }
            else
            {
                // 他のカードは、このカードと同じ移動差分で移動させる
                card.transform.position += moveDelta;
            }
        }
        // Z軸は変更しないように上書きする必要は、OnPointerDownで統一したため不要。
    }

    // CardDisplay.cs の OnEndDrag を修正

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // TODO: ここでドロップ先の場所（Deck Zone, Hand Zoneなど）を判定し、
        // GameManagerを介してSelectedCards全てのCardDataのLocationIdとPositionを更新する処理が必要。

        // ★追加: ドラッグ終了後、全カードの選択状態を解除 (必須ではありませんが、一般的なUX)
        // Listをコピーしてから解除しないと、ループ中にリストが変更されエラーになる
        var cardsToDeselect = new List<CardDisplay>(SelectedCards);
        foreach (var card in cardsToDeselect)
        {
            card.ToggleSelection(); // isSelected=false になり、リストからも削除される
        }
    }

    // CardDisplay.cs 内に追加
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
                // CardDataの状態をトグルするロジック
                if (cardData.State == CardData.CardState.FACE_UP)
                {
                    cardData.State = CardData.CardState.FACE_DOWN_ALL;
                }
                else
                {
                    cardData.State = CardData.CardState.FACE_UP;
                }
                UpdateVisuals();

                // 処理を終了 (選択状態の変更は行わない)
                return;
            }

            // ② シングルクリックで選択状態をトグル
            if (eventData.clickCount == 1)
            {
                ToggleSelection();
            }
        }

        // ③ 右クリック（選択ロジックの代替）: 今回はシングルクリックで実装したので不要だが、残す場合は維持
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"Card {cardData.Id} was right-clicked/long-pressed.");
        }
    }
}