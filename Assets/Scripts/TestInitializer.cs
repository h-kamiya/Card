using UnityEngine;

public class TestInitializer : MonoBehaviour
{
    public CardDisplay cardPrefab; // Card Prefab��Inspector���犄�蓖�Ă�
    public Vector3 initialPosition = new Vector3(-3, 0, 0);

    void Start()
    {
        // 1. �V����CardData���쐬
        CardData newCardData = new CardData
        {
            Id = "TestCard",
            Text = "TSA",
            State = CardData.CardState.FACE_UP,
            ZIndex = 10,
            Position = initialPosition
        };

        // 2. Prefab���C���X�^���X��
        CardDisplay newCard = Instantiate(cardPrefab, initialPosition, Quaternion.identity);

        // 3. CardDisplay��������
        newCard.Initialize(newCardData);
    }
}