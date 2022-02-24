using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int value;
    public Node node;
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

    public void SetBlock(Node node)
    {
        if (this.node != null) this.node.occupiedBlock = null;
        this.node = node;
        this.node.occupiedBlock = this;

    }
}