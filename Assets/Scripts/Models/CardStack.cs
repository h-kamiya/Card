// CardStack.cs (Modelsフォルダ)

using System.Collections.Generic;
using Game.Models;

namespace Game.Models
{
    /// <summary>
    /// プレイヤー操作によって場に一時的に生成される「2枚以上のカードの束」を表現するModel。
    /// 固定ゾーン管理や特殊なロジック管理機能は持たず、純粋にカードの集まりを表します。
    /// </summary>
    public class CardStack
    {
        public string Id { get; private set; } // スタックの一意な識別子 (例: "Deck", "Stack_1", "Discard")

        // 【スタックの実体】: カードのリスト。リストの末尾が「最上部のカード」となる。
        public List<CardData> Cards { get; private set; } = new List<CardData>();

        /// <summary>
        /// CardStackは初期状態で複数のカード（スタックの要件：2枚以上）を持つ必要があります。
        /// </summary>
        public CardStack(string id, List<CardData> initialCards)
        {
            Id = id;
            Cards.AddRange(initialCards);
        }

        /// <summary>
        /// スタックの最上部にカードを追加します。
        /// </summary>
        public void Push(CardData card)
        {
            Cards.Add(card);
        }

        /// <summary>
        /// スタックの最上部からカードを取り出し、スタックから削除します。（ドロー操作など）
        /// </summary>
        public CardData Pop()
        {
            if (Cards.Count == 0) return null;

            int topIndex = Cards.Count - 1;
            CardData topCard = Cards[topIndex];

            Cards.RemoveAt(topIndex);

            return topCard;
        }
    }
}