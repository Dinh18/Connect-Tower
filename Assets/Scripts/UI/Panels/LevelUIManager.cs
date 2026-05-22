using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIManager : MonoBehaviour
{
    [SerializeField] List<LevelUIController> levelUIControllers;

    public IEnumerator Show()
    {
        yield return new WaitUntil( () =>CoreServices.Get<DataManager>().dataReady);
        for(int i = 0; i< levelUIControllers.Count; i++)
        {
            levelUIControllers[i].ShowLevel(i);
        }
    }
   
}