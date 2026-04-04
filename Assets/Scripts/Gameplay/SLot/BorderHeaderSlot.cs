
using UnityEngine;
using UnityEngine.UI;

public class BorderHeaderSlot : MonoBehaviour
{
    private Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(this.gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetUp()
    {
        image = GetComponent<Image>();
    }
    public void SetColor(Color color)
    {
        image.color = color;
    }
}
