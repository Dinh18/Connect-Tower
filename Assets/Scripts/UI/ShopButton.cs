using UnityEngine;

public class ShopButton : MonoBehaviour
{
    [SerializeField] private GameObject homeBackground;
    [SerializeField] private GameObject shopBackground;
    [SerializeField] private GameObject shop;
    private Vector3 originTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originTransform = transform.position;
        shopBackground.SetActive(false);
        shop.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClick()
    {
        homeBackground.SetActive(false);

        shopBackground.SetActive(true);

        shop.SetActive(true);

    }
}
