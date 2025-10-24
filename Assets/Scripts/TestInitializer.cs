using UnityEngine;

public class TestInitializer : MonoBehaviour
{
    public CardDisplay cardPrefab; // Card PrefabをInspectorから割り当てる
    public Vector3 initialPosition = new Vector3(-3, 0, 0);

    void Start()
    {
        // 1. 新しいCardDataを作成
        CardData newCardData = new CardData
        {
            Id = "TestCard",
            Text = "TSA",
            State = CardData.CardState.FACE_UP,
            ZIndex = 10,
            Position = initialPosition
        };

        // 2. Prefabをインスタンス化
        CardDisplay newCard = Instantiate(cardPrefab, initialPosition, Quaternion.identity);

        // 3. CardDisplayを初期化
        newCard.Initialize(newCardData);
    }
}