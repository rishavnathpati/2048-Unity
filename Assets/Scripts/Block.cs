using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int value;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private TextMeshPro text;
    
    public void Init(GameManager.BlockType type)
    {
        value = type.value;
        renderer.color = type.color;
        text.text = type.value.ToString();
    }
}
