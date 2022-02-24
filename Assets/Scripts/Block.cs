using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int value;
    public Node node;
    public Block mergingBlock;
    public bool merging;

    [SerializeField] private GameObject visual;
    [SerializeField] private TextMeshPro valueText;
    private SpriteRenderer _spriteRenderer;
    public Vector2 Pos => transform.position;


    public void Init(GameManager.BlockType type)
    {
        _spriteRenderer = visual.GetComponent<SpriteRenderer>();

        value = type.value;
        _spriteRenderer.color = type.color;
        valueText.text = type.value.ToString();
    }

    public void SetBlock(Node varNode)
    {
        if (node != null) node.occupiedBlock = null;
        node = varNode;
        node.occupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith)
    {
        mergingBlock = blockToMergeWith;

        // Set current node as unoccupied to allow it to use it
        node.occupiedBlock = null;

        blockToMergeWith.merging = true;
    }

    public bool CanMerge(int varValue)
    {
        return varValue == value && !merging && mergingBlock == null;
    }
}