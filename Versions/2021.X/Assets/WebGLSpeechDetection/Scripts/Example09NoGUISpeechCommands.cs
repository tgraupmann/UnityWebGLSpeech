using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebGLSpeechDetection
{
    public class Example09NoGUISpeechCommands : MonoBehaviour
    {
        /// <summary>
        /// Reference to the plugin
        /// </summary>
        private ISpeechDetectionPlugin _mSpeechDetectionPlugin = null;

        /// <summary>
        /// List of words to detect
        /// </summary>
        private List<string> _mWords = new List<string>();

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

            // A list of words to detect
            string[] wordsToDetect =
            {
                "Climb",
                "Crouch",
                "Jump",
                "Left",
                "Right",
                "Stand",
                "Run",
                "10", // match 10 before 1
                "Ten",
                "1",
                "One",
                "2",
                "Two",
                "3",
                "Three",
                "4",
                "Four",
                "5",
                "Five",
                "6",
                "Six",
                "7",
                "Seven",
                "8",
                "Eight",
                "9",
                "Nine",
            };

            foreach (string word in wordsToDetect)
            {
                _mWords.Add(word.ToLower());
            }
        }

        /// <summary>
        /// Handler for speech detection events
        /// </summary>
        /// <param name="detectionResult"></param>
        /// <returns>Return true if the result was handled</returns>
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
            bool doAbort = false;
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
                    string lower = alternative.transcript.ToLower();
                    Debug.LogFormat("Detected: {0}", lower);
                    foreach (string word in _mWords)
                    {
                        if (lower.Contains(word))
                        {
                            Debug.Log(string.Format("**** {0} ****", word));
                            doAbort = true;
                            break;
                        }
                    }
                }
                if (doAbort)
                {
                    break;
                }
            }

            // abort detection on match for faster matching on words instead of complete sentences
            if (doAbort)
            {
                _mSpeechDetectionPlugin.Abort();
                return true;
            }

            return false;
        }
    }
}
