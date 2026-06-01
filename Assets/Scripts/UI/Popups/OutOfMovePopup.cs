using System;
using UnityEngine;
using UnityEngine.UI;

public class OutOfMovePopup : Popup
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button addMoveButton;
    void OnEnable()
    {
        closeButton.onClick.AddListener(OnClickClose);
        addMoveButton.onClick.AddListener(OnClickAddMove);
    }

    void OnDisable()
    {
        closeButton.onClick.RemoveListener(OnClickClose);
        addMoveButton.onClick.RemoveListener(OnClickAddMove);
    }

    private void OnClickClose()
    {
        CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.Lose);
    }

    private void OnClickAddMove()
    {
        if(CoreServices.Get<GameManager>().AddMoveToContinue(5))
        {
            CoreServices.Get<UIManager>().PopUI();
        }
        else
        {
            CoreServices.Get<UIManager>().OpenShop();
        }
    }

    public override void Show()
    {
        base.Show();

    }
    public override void Hide()
    {
        base.Hide();
    }
}
