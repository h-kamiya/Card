using UnityEngine;
using System; // Serializable���g�p���邽�߂ɕK�v

// Unity�̃G�f�B�^�[�ŕ\���E�ҏW�\�ɂ���
[Serializable]
public class CardData
{
    // C#�̗񋓌^ (Enum) �ŃJ�[�h�̏�Ԃ��`
    public enum CardState { FACE_UP, FACE_DOWN_ALL } // �ȗ������ꂽ���

    // �f�[�^�i���f���j�̑���
    public string Id;                  // ��: "S-A" (�X�y�[�h�̃G�[�X)
    public string Text;                // ��: "SA"
    public string LocationId;          // ��: "DECK", "HAND_ZONE", "DISCARD_ZONE"
    public CardState State;           // �J�[�h�̗��\�̏��
    public int ZIndex;                 // �d�ˏ� (�h���b�O���ɍł������Ȃ�)
    // ���ǉ�: �I����Ԃ�ێ�����t���O
    public bool isSelected = false;
    // �Q�[���I�u�W�F�N�g�i�r���[�j���Q�Ƃ��邽�߂̏��
    public Vector3 Position;           // ��ʏ�̍ŏI���W
}