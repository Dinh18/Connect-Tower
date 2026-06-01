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

        Vector3 startPoint = this.transform.position; // Điểm xuất phát là vị trí của AddMoveEffect trên Canvas
        Vector3 endPoint = moveCountText.position;

        // Điểm uốn cong cố định ở giữa màn hình để đảm bảo không bay ra ngoài
        Vector3 curvePoint = new Vector3(safeArea.width / 2, safeArea.height / 2, 0);
        
        Vector3[] pathArr = new Vector3[] { startPoint, curvePoint, endPoint };

        snowEffectPrefab.position = startPoint;
        snowEffectPrefab.localScale = Vector3.one;

        float flyDuration = 1.0f;

        // Bay theo đường cong
        sequence.Append(snowEffectPrefab.DOPath(pathArr, flyDuration, PathType.CatmullRom).SetEase(Ease.OutSine));
        
        // Hiệu ứng scale tạo độ sâu 3D (phóng to khi ra giữa, thu nhỏ khi đến đích)
        sequence.Join(snowEffectPrefab.DOScale(8f, flyDuration / 2f).SetEase(Ease.OutQuad));
        sequence.Insert(flyDuration / 2f, snowEffectPrefab.DOScale(1f, flyDuration / 2f).SetEase(Ease.InQuad));

        // Xoay tròn tạo cảm giác động năng
        sequence.Join(snowEffectPrefab.DORotate(new Vector3(0, 0, -720f), flyDuration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.InOutSine));

        sequence.AppendCallback(() => {
            snowExplosionPrefab.position = moveCountText.position;
            snowExplosionPrefab.gameObject.SetActive(true);
            
            // Rung text moveCount để tăng lực va chạm
            moveCountText.DOKill();
            moveCountText.DOShakeScale(0.4f, 0.5f, 10, 90f, true);
            
            StartCoroutine(ResetEffect(ExcuteBooster, startPoint));
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
