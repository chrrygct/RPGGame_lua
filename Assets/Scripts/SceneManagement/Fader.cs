using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        CanvasGroup canvasGroup ;
        Coroutine currentActiveFade;
        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            //StartCoroutine(FadeOut(3f));
        }

        public IEnumerator FadeOut(float time)
        {
            return Fade(1,time);
        }
        public IEnumerator FadeIn(float time)
        {
            return Fade(0,time);
        }
        public IEnumerator Fade(float targetAlpha,float time)
        {
            if (currentActiveFade != null)
            {
                StopCoroutine(currentActiveFade);
            }
            currentActiveFade = StartCoroutine(FadeRoutine(targetAlpha, time));
            yield return currentActiveFade;
        }

        public IEnumerator FadeRoutine(float targetAlpha, float time)
        {
            while(!Mathf.Approximately(canvasGroup.alpha,targetAlpha))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime / time) ;
                yield return null;
            }
        }       


        public void FadeOutImmediate()
        {
            //print(canvasGroup);
            canvasGroup.alpha = 1;
        }
        public void FadeInImmediate()
        {
            canvasGroup.alpha = 0;
        }  
    }
}


