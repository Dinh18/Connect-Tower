using UnityEngine;

public class SlotVFX : MonoBehaviour
{
    private ParticleSystem mergeVFX;
    void OnEnable()
    {
        
    }
    
    public void Setup()
    {
        mergeVFX = GetComponent<ParticleSystem>();
        this.gameObject.SetActive(false);
    }
    public void PlayVFX()
    {
        this.gameObject.SetActive(true);
        mergeVFX.Play();
    }

}
