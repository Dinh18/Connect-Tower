using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AddMoveEffect : MonoBehaviour, IBoosterEffect
{
    [SerializeField] private RectTransform addMoveIcon;
    [SerializeField] private RectTransform moveCountText;
    [SerializeField] private RectTransform snowEffectPrefab;
    [SerializeField] private RectTransform snowExplosionPrefab;
    // [SerializeField] private GameObject countdownImage;
    // [SerializeField] private UIFrostEffect frostEffect;
    // [SerializeField] private AnimationCurve curveY;
    // [SerializeField] private AnimationCurve curveX;
    private Vector3 originPos;
    private Vector3 originScale;
    private Quaternion originRotation;
    void Awake()
    {
        originPos = addMoveIcon.position;
        originScale = addMoveIcon.localScale;
        originRotation = addMoveIcon.localRotation;

    }
    // private 
    public void PlayEffect(Action ExcuteBooster)
    {
        Sequence sequence = DOTween.Sequence();
        snowEffectPrefab.gameObject.SetActive(true);

        Rect safeArea = Screen.safeArea;

        Vector3 topCenterScreen = new Vector3(safeArea.width / 2, safeArea.height, 0);
        Vector3 bottomCenterScreen = new Vector3(safeArea.width / 2, originPos.y + 10, 0);

        snowEffectPrefab.position = bottomCenterScreen;
        sequence.Append(snowEffectPrefab.DOMove(topCenterScreen, 1f).SetEase(Ease.InOutQuad));
        sequence.Join(snowEffectPrefab.DOScale(10f, 1f).SetEase(Ease.InOutQuad));

        // float explosionTime = 0.2f;

        // // Phóng to đột ngột
        // sequence.Append(snowEffectPrefab.DOScale(6f, explosionTime).SetEase(Ease.OutExpo));

        sequence.AppendCallback(() => {
            snowExplosionPrefab.position = snowEffectPrefab.position;
            snowExplosionPrefab.gameObject.SetActive(true);
            // countdownImage.SetActive(true);
            // DOTween.To(() => frostEffect.FrostAmount, 
            //            x => frostEffect.FrostAmount = x, 
            //            0.5f, 
            //            0.5f)
            //       .SetEase(Ease.OutQuad);
            StartCoroutine(ResetEffect(ExcuteBooster, bottomCenterScreen));
        });
    }
    public IEnumerator ResetEffect(Action ExcuteBooster, Vector3 bottomCenterScreen)
    {
        ExcuteBooster?.Invoke();
        snowEffectPrefab.position = bottomCenterScreen;
        snowEffectPrefab.localScale = Vector3.one;
        snowEffectPrefab.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        snowExplosionPrefab.gameObject.SetActive(false);

    }
}
