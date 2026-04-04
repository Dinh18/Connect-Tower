using UnityEngine;

public class ShopUI : MonoBehaviour
{
    private MainMenuUIManager MainMenuPanel;
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
