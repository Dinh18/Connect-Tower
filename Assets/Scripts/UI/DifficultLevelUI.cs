using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DifficultLevel : MonoBehaviour
{
    [SerializeField] private Image[] skullGameObject;
    [SerializeField] private GameObject dimImage;

    public void ShowDifficultLevel()
    {
        dimImage.SetActive(true);
        for(int i = 0; i < skullGameObject.Length; i++)
        {
            skullGameObject[i].DOKill();
            Color color = skullGameObject[i].color;
            color.a = 0f;
            skullGameObject[i].color = color;
            skullGameObject[i].gameObject.SetActive(true);
            Sequence sequence = DOTween.Sequence();
            // sequence.Append(skullGameObject[i].DOFade(0, 0.5f));
            sequence.Append(skullGameObject[i].DOFade(1, 3f));
            sequence.Join(skullGameObject[i].GetComponent<RectTransform>().DOAnchorPosY( 100, 3f).SetEase(Ease.OutQuad));
        }
        StartCoroutine(HideDifficultLevel());
    }

    public IEnumerator HideDifficultLevel()
    {
        yield return new WaitForSeconds(3);
        dimImage.SetActive(false);
        for(int i = 0; i < skullGameObject.Length; i++)
        {
            skullGameObject[i].DOKill();
            skullGameObject[i].gameObject.SetActive(false);
        }
    }

    private void SkullAnimation(Image skull, float duration)
    {
        
    }
}
