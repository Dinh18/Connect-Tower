using UnityEngine;
using UnityEngine.UI;

public class BoosterButton : MonoBehaviour
{
    private Button boosterButton;
    private IBooster booster;
    void Awake()
    {
        boosterButton = GetComponent<Button>();

        booster = GetComponentInChildren<IBooster>();

        boosterButton.onClick.AddListener(OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        boosterButton.interactable = false;

        booster.Excute();

        boosterButton.interactable = true;
    }
}
