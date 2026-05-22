using UnityEngine;

public class Menu : MonoBehaviour
{
    public virtual void Show()
    {
        this.gameObject.SetActive(true);
    }
    public virtual void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
