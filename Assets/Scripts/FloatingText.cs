using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float fadeTime = 1;

    public void Init(int value)
    {
        text.text = value.ToString();

        var sequence = DOTween.Sequence();

        sequence.Insert(0, text.DOFade(0, fadeTime));
        sequence.Insert(0, text.transform.DOMove(text.transform.position + Vector3.up, fadeTime));

        sequence.OnComplete(() => Destroy(gameObject));
    }
}