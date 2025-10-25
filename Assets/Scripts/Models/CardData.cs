using System; // Serializableを使用するために必要
using UnityEngine;

namespace Game.Models
{ 
    // Unityのエディターで表示・編集可能にする
    [Serializable]
    public class CardData
    {
        // C#の列挙型 (Enum) でカードの状態を定義
        public enum CardState { FACE_UP, FACE_DOWN_ALL } // 簡略化された状態

        // データ（モデル）の属性
        public string Id;                  // 例: "S-A" (スペードのエース)
        public string Text;                // 例: "SA"
        public string LocationId;          // 例: "DECK", "HAND_ZONE", "DISCARD_ZONE"
        public CardState State;           // カードの裏表の状態
        public int ZIndex;                 // 重ね順 (ドラッグ時に最も高くなる)
        // ★追加: 選択状態を保持するフラグ
        public bool isSelected = false;
        // ゲームオブジェクト（ビュー）が参照するための情報
        public Vector3 Position;           // 画面上の最終座標
    }
}