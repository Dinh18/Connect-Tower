using UnityEngine;
using UnityEngine.UI;

public class BoosterButton : MonoBehaviour
{
    private Button boosterButton;
    private IBooster booster;
    void OnEnable()
    {
        boosterButton.onClick.AddListener(OnButtonClicked);
    }
    void OnDisable()
    {
        boosterButton.onClick.RemoveListener(OnButtonClicked);
    }
    void Awake()
    {
        boosterButton = GetComponent<Button>();

        booster = GetComponentInChildren<IBooster>();

    }

    public void OnButtonClicked()
    {
        boosterButton.interactable = false;

        booster.Excute();

        boosterButton.interactable = true;
    }
}
