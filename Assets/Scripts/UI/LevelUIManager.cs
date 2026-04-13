using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIManager : MonoBehaviour
{
    [SerializeField] List<LevelUIController> levelUIControllers;

    public void Show()
    {
        for(int i = 0; i< levelUIControllers.Count; i++)
        {
            levelUIControllers[i].ShowLevel(i);
        }
    }
   
}