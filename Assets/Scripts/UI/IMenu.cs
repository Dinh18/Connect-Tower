using UnityEngine;

public interface IMenu
{
    public void Setup(UIManager uIManager);
    public void Hide();
    public void Show();
    public GameObject GetGameObject();
}
