using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechDetection
{
    /// <summary>
    /// Set the referenced Text UI text to the GameObject name
    /// </summary>
    public class Example02Word : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Image UI object
        /// </summary>
        public Image _mImage = null;

        /// <summary>
        /// Highlight the word, wait, and then set the default color
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoHighlight()
        {
            _mImage.color = new Color(1, 0.5f, 0);
            yield return new WaitForSeconds(1);
            _mImage.color = Color.white;
        }

        /// <summary>
        /// Public method invokes the coroutine
        /// </summary>
        public void Highlight()
        {
            StartCoroutine(DoHighlight());
        }
    }
}
