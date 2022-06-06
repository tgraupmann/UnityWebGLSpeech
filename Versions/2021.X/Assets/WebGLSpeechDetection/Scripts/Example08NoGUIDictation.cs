using System.Collections;
using System.Text;
using UnityEngine;

namespace UnityWebGLSpeechDetection
{
    public class Example08NoGUIDictation : MonoBehaviour
    {
        /// <summary>
        /// Reference to the plugin
        /// </summary>
        private ISpeechDetectionPlugin _mSpeechDetectionPlugin = null;

        // Use this for initialization
        IEnumerator Start()
        {
            // get the singleton instance
            _mSpeechDetectionPlugin = SpeechDetectionUtils.GetInstance();

            // check the reference to the plugin
            if (null == _mSpeechDetectionPlugin)
            {
                Debug.LogError("WebGL Speech Detection Plugin is not set!");
                yield break;
            }

            // wait for plugin to become available
            while (!_mSpeechDetectionPlugin.IsAvailable())
            {
                yield return null;
            }

            // subscribe to events
            _mSpeechDetectionPlugin.AddListenerOnDetectionResult(HandleDetectionResult);
        }

        /// <summary>
        /// Handler for speech detection events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        bool HandleDetectionResult(DetectionResult detectionResult)
        {
            if (null == detectionResult)
            {
                return false;
            }
            SpeechRecognitionResult[] results = detectionResult.results;
            if (null == results)
            {
                return false;
            }
            foreach (SpeechRecognitionResult result in results)
            {
                SpeechRecognitionAlternative[] alternatives = result.alternatives;
                if (null == alternatives)
                {
                    continue;
                }
                foreach (SpeechRecognitionAlternative alternative in alternatives)
                {
                    if (string.IsNullOrEmpty(alternative.transcript))
                    {
                        continue;
                    }
                    if (result.isFinal)
                    {
                        Debug.Log(string.Format("[FINAL] \"{0}\" Confidence={1}", 
                            alternative.transcript,
                            alternative.confidence));
                    }
                    else
                    {
                        Debug.Log(string.Format("\"{0}\" Confidence={1}",
                            alternative.transcript,
                            alternative.confidence));
                    }
                }
            }

            // dictation doesn't need to handle the event
            return false;
        }
    }
}
