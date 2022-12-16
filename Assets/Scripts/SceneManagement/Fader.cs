using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        CanvasGroup canvasGroup;
        Coroutine currentActiveFade; // The fade coroutine currently running
       
        private void Awake() 
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void FadeOutImmediate()
        {
            canvasGroup.alpha = 1;
        }

        /*Fade in and fade out changed to Coroutine return types rather than IEnumerator. This allows the function
        to be called without a yield return so the next line of code can be run without waiting for the
        fucntion to finish. In Portal, this means the PlayerController can be reenabled before FadeIn is complete*/
        Coroutine Fade(float target, float time)
        {
            if (currentActiveFade != null)
            {
                // If a fade in or fade out coroutine currently, stop the coroutine before starting a new fade coroutine
                StopCoroutine(currentActiveFade);
            }

            currentActiveFade = StartCoroutine(FadeRoutine(target, time));
            return currentActiveFade;
        }
        
        public Coroutine FadeOut(float time)
        {
            return Fade(1, time);
        }

        public Coroutine FadeIn(float time)
        {
            return Fade(0, time);
        }

          private IEnumerator FadeRoutine(float target, float time)
        {   
            /* While alpha value of canvas group has not reached the set target value, 
            continue to move alpha by 1 * (Time.deltaTime / time) each frame until it reaches target.
            The amount alpha needs to move to get from 0 to 1 is 1 * (Time.deltaTime / time)*/
            while (!Mathf.Approximately(canvasGroup.alpha, target))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.deltaTime / time);
                yield return null;
            }
        }
    }
}
