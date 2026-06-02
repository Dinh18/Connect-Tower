using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintEffectController : MonoBehaviour
{
    [Header("Hint References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject dimImage;
    [SerializeField] private MagnifyingGlassEffect magnifyingGlass;

    private void OnEnable()
    {
        GameEventBus.Subscribe<RequestExecuteBoosterEvent>(OnRequestBooster);
    }

    private void OnDisable()
    {
        GameEventBus.UnSubscribe<RequestExecuteBoosterEvent>(OnRequestBooster);
    }

    private void OnRequestBooster(RequestExecuteBoosterEvent evt)
    {
        if (evt.boosterType == BoosterType.Hint)
        {
            SearchedBlocks(evt.onComplete);
        }
    }

    private void SearchedBlocks(System.Action<bool> onComplete)
    {
        bool hasHiddenBlocks = false;
        foreach(SlotController slot in CoreServices.Get<SlotsManager>().GetAllSlots())
        {
            if(!slot.isRevealed) hasHiddenBlocks = true;
            foreach(BlockController block in slot.blocks)
            {
                if(!block.isRevealed) hasHiddenBlocks = true;
            }
        }

        if (!hasHiddenBlocks) 
        {
            onComplete?.Invoke(false);
            return;
        }

        dimImage.SetActive(true);
        CoreServices.Get<InputManager>().SetInputBlocked(true);

        float duration = 7f; 
        magnifyingGlass.Activate(mainCamera, duration, () => {
            dimImage.SetActive(false);
            CoreServices.Get<InputManager>().SetInputBlocked(false);
            onComplete?.Invoke(true);
        });
    }

    public IEnumerator HintCoroutine(float time, GameObject hintImage1, GameObject hintImage2, BlockController block1, BlockController block2 = null)
    {
        hintImage1.transform.position = mainCamera.WorldToScreenPoint(block1.transform.position);
        hintImage1.SetActive(true);
        if(block2 != null)
        {
            hintImage2.transform.position = mainCamera.WorldToScreenPoint(block2.transform.position);
            hintImage2.SetActive(true);
        } 

        yield return new WaitForSeconds(time);

        block1.SetColorOutLine();
        if(block2 != null) block2.SetColorOutLine();

        hintImage1.SetActive(false);
        hintImage2.SetActive(false);
    }
}
