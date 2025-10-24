using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // ���ǉ��F�C�x���g�V�X�e�����g�p

// IPointerDownHandler: �N���b�N/�^�b�v�J�n
// IDragHandler: �h���b�O��
// IPointerClickHandler: �N���b�N/�^�b�v����
// IEndDragHandler: �h���b�O�I��
public class CardDisplay : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerClickHandler,
    IEndDragHandler
{
    [Header("�A�Z�b�g�̎Q��")]
    // 2D�̏ꍇ�ASpriteRenderer�ŉ摜��\��
    public SpriteRenderer spriteRenderer;

    // �p�ӂ����e�N�X�`���A�Z�b�g��Unity�G�f�B�^����ݒ�
    public Sprite faceSprite; // �\�ʂ̉摜 (�X�y�[�h��A�Ȃ�)
    public Sprite backSprite; // ���ʂ̉摜 (���ʂ̗���)

    [HideInInspector]
    public CardData cardData; // ���̃Q�[���I�u�W�F�N�g�ɑΉ�����CardData�C���X�^���X

    // ��ʂɕ\������e�L�X�g�R���|�[�l���g (�I�v�V�����B����͏ȗ���)
    // public TextMeshPro textDisplay; 
    // ���ǉ�: �V�[���S�̂őI�����ꂽ�J�[�h�̃��X�g���Ǘ�
    public static List<CardDisplay> SelectedCards = new List<CardDisplay>();
    private Vector3 initialDragPosition; // �h���b�O�J�n���̈ʒu���L��

    // �h���b�O�����p�̕ϐ�
    private Vector3 dragOffset; // �h���b�O�J�n���̃}�E�X/�I�u�W�F�N�g�̑��Έʒu
    private bool isDragging = false; // �h���b�O�����ǂ����̃t���O

    // --- �������ƕ`�� ---

    public void Initialize(CardData data)
    {
        this.cardData = data;
        // �f�[�^ID�Ɋ�Â���Sprite��ݒ肷�郍�W�b�N (��: Resources.Load<Sprite>(data.Id))
        // ����͊ȗ����̂��߁AInitialize�O�Ɏ蓮�� 'faceSprite' ��ݒ肷����̂Ƃ��܂��B

        // ����`��
        UpdateVisuals();
    }

    // �I����Ԃ��g�O������֐�
    public void ToggleSelection()
    {
        cardData.isSelected = !cardData.isSelected;

        if (cardData.isSelected)
        {
            // �I�����X�g�ɒǉ����A���o�I�ȃt�B�[�h�o�b�N��L����
            if (!SelectedCards.Contains(this))
            {
                SelectedCards.Add(this);
            }
            // �����o�I�ȃt�B�[�h�o�b�N�i��: �A�E�g���C���A�F�ύX�j�������Ɏ���
            spriteRenderer.color = Color.yellow; // �ȈՓI�ȃn�C���C�g
        }
        else
        {
            // �I�����X�g����폜���A���o�I�ȃt�B�[�h�o�b�N�𖳌���
            SelectedCards.Remove(this);
            spriteRenderer.color = Color.white; // �ʏ�̐F�ɖ߂�
        }
        // TODO: ������GameManager����ĉi�����������Ăяo��
    }

    // ���f���̏�ԂɊ�Â��ĕ\�����X�V����֐�
    public void UpdateVisuals()
    {
        // 1. �摜 (���\) �̐؂�ւ�
        if (cardData.State == CardData.CardState.FACE_UP)
        {
            spriteRenderer.sprite = faceSprite;
            // textDisplay.text = cardData.Text; // �e�L�X�g���\��
        }
        else // FACE_DOWN_ALL �̏ꍇ
        {
            spriteRenderer.sprite = backSprite;
            // textDisplay.text = "???"; // ���ʕ\��
        }

        // ���ǉ�: �I����Ԃ̏����`��𔽉f
        spriteRenderer.color = cardData.isSelected ? Color.yellow : Color.white;

        // 2. �ʒu�Əd�ˏ��̍X�V
        // transform.position = cardData.Position; // �ړ��̓h���b�O�Œ��ڍs�����߁A�����ł�ZIndex�̂ݔ��f

        // ZIndex�Ɋ�Â���Z���W�̓K�p
        // ZIndex�͏d�ˏ������߂邽�߂̒l�BZ�l���傫���قǎ�O�ɕ\������܂��B
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            -cardData.ZIndex * 0.01f // ZIndex�������ق�Z�l�͎�O(�������l)�ɂȂ�悤����
        );

        // 3. �I����Ԃ̎��o�I�ȃt�B�[�h�o�b�N (��: �A�E�g���C���\���Ȃ�)
        // if (cardData.isSelected) { ... }
    }

    // CardDisplay.cs ���ɒǉ�
    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. Z-Index���őO�ʂɂ���i�C���ς݃��W�b�N�j
        // ... (������Z���ݒ胍�W�b�N�͈ێ�: transform.position.z = -0.1f;)
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            -0.1f
        );

        // 2. �h���b�O�I�t�Z�b�g���v�Z (�����͕ύX�Ȃ�)
        dragOffset = transform.position - Camera.main.ScreenToWorldPoint(eventData.position);
        isDragging = false;

        // ���ǉ�: �I����Ԃ̊m�F�ƃh���b�O����
        initialDragPosition = transform.position; // �h���b�O�J�n���̈ʒu���L�^

        // �I������Ă��Ȃ��J�[�h���h���b�O�J�n�����ꍇ�A�܂��I������
        if (!cardData.isSelected)
        {
            ToggleSelection();
        }

        // Z-Index���őO�ʂ� (�I���J�[�h�S�Ă��ꎞ�I�ɍőO�ʂ�)
        foreach (var card in SelectedCards)
        {
            card.transform.position = new Vector3(
                card.transform.position.x,
                card.transform.position.y,
                -0.1f // �S��-0.1f�ɂ��邱�ƂŁA�h���b�O�����d�ˏ��𓝈�
            );
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;

        // 1. ���݂̃J�[�h�̐V�����ʒu���v�Z
        Vector3 curScreenPoint = eventData.position;
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + dragOffset;

        // 2. �ړ��� (����) ���v�Z
        Vector3 moveDelta = newPosition - transform.position;

        // 3. �I������Ă���S�ẴJ�[�h���ړ�
        foreach (var card in SelectedCards)
        {
            // ���݃h���b�O���Ă���J�[�h�͌v�Z���ꂽnewPosition�ɁA���͈ړ����������Z
            if (card == this)
            {
                card.transform.position = newPosition;
            }
            else
            {
                // ���̃J�[�h�́A���̃J�[�h�Ɠ����ړ������ňړ�������
                card.transform.position += moveDelta;
            }
        }
        // Z���͕ύX���Ȃ��悤�ɏ㏑������K�v�́AOnPointerDown�œ��ꂵ�����ߕs�v�B
    }

    // CardDisplay.cs �� OnEndDrag ���C��

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // TODO: �����Ńh���b�v��̏ꏊ�iDeck Zone, Hand Zone�Ȃǁj�𔻒肵�A
        // GameManager�����SelectedCards�S�Ă�CardData��LocationId��Position���X�V���鏈�����K�v�B

        // ���ǉ�: �h���b�O�I����A�S�J�[�h�̑I����Ԃ����� (�K�{�ł͂���܂��񂪁A��ʓI��UX)
        // List���R�s�[���Ă���������Ȃ��ƁA���[�v���Ƀ��X�g���ύX����G���[�ɂȂ�
        var cardsToDeselect = new List<CardDisplay>(SelectedCards);
        foreach (var card in cardsToDeselect)
        {
            card.ToggleSelection(); // isSelected=false �ɂȂ�A���X�g������폜�����
        }
    }

    // CardDisplay.cs ���ɒǉ�
    public void OnPointerClick(PointerEventData eventData)
    {
        // �h���b�O���삾�����ꍇ�́A�N���b�N�C�x���g�𖳎�����
        if (isDragging)
        {
            return;
        }

        // ���N���b�N (Primary action) �̂ݏ���
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // �@ �_�u���N���b�N�ŗ��Ԃ����g�O��
            if (eventData.clickCount == 2)
            {
                // CardData�̏�Ԃ��g�O�����郍�W�b�N
                if (cardData.State == CardData.CardState.FACE_UP)
                {
                    cardData.State = CardData.CardState.FACE_DOWN_ALL;
                }
                else
                {
                    cardData.State = CardData.CardState.FACE_UP;
                }
                UpdateVisuals();

                // �������I�� (�I����Ԃ̕ύX�͍s��Ȃ�)
                return;
            }

            // �A �V���O���N���b�N�őI����Ԃ��g�O��
            if (eventData.clickCount == 1)
            {
                ToggleSelection();
            }
        }

        // �B �E�N���b�N�i�I�����W�b�N�̑�ցj: ����̓V���O���N���b�N�Ŏ��������̂ŕs�v�����A�c���ꍇ�͈ێ�
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"Card {cardData.Id} was right-clicked/long-pressed.");
        }
    }
}