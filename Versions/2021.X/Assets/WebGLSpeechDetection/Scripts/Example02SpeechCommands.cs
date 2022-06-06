using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechDetection
{
    public class Example02SpeechCommands : MonoBehaviour
    {
        /// <summary>
        /// Reference to the group of words
        /// </summary>
        public Image _mImageWords = null;

        /// <summary>
        /// Reference to the example text summary
        /// </summary>
        public Text _mTextSummary = null;

        /// <summary>
        /// Reference to a warning if plugin is not available
        /// </summary>
        public Text _mTextWarning = null;

        /// <summary>
        /// Dropdown selector for languages
        /// </summary>
        public Dropdown _mDropDownLanguages = null;

        /// <summary>
        /// Dropdown selector for dialects
        /// </summary>
        public Dropdown _mDropDownDialects = null;

        /// <summary>
        /// Reference to the plugin
        /// </summary>
        private ISpeechDetectionPlugin _mSpeechDetectionPlugin = null;

        /// <summary>
        /// Reference to the supported languages and dialects
        /// </summary>
        private LanguageResult _mLanguageResult = null;

        /// <summary>
        /// Dictionary of words to detect
        /// </summary>
        private Dictionary<string, Example02Word> _mWords = new Dictionary<string, Example02Word>();

        /// <summary>
        /// Set the starting UI layout
        /// </summary>
        private void Awake()
        {
            // no need to display the summary if the plugin is missing
            SpeechDetectionUtils.SetActive(false,
                _mDropDownDialects, _mDropDownLanguages,
                _mImageWords, _mTextSummary);
        }

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

            // no need to display a warning if the plugin is available
            SpeechDetectionUtils.SetActive(false, _mTextWarning);

            // make the dropdowns non-interactive
            SpeechDetectionUtils.SetInteractable(false,
                _mDropDownLanguages, _mDropDownDialects);

            // show UI controls
            SpeechDetectionUtils.SetActive(true,
                _mDropDownDialects, _mDropDownLanguages,
                _mImageWords, _mTextSummary);

            // subscribe to events
            _mSpeechDetectionPlugin.AddListenerOnDetectionResult(HandleDetectionResult);

            // Get languages from plugin,
            _mSpeechDetectionPlugin.GetLanguages((languageResult) =>
            {
                _mLanguageResult = languageResult;

                // prepare the language drop down items
                SpeechDetectionUtils.PopulateLanguagesDropdown(_mDropDownLanguages, _mLanguageResult);

                // make the dropdowns interactive
                SpeechDetectionUtils.SetInteractable(true,
                    _mDropDownLanguages, _mDropDownDialects);

                // subscribe to language change events
                if (_mDropDownLanguages)
                {
                    _mDropDownLanguages.onValueChanged.AddListener(delegate {
                        SpeechDetectionUtils.HandleLanguageChanged(_mDropDownLanguages,
                            _mDropDownDialects,
                            _mLanguageResult,
                            _mSpeechDetectionPlugin);
                    });
                }

                // subscribe to dialect change events
                if (_mDropDownDialects)
                {
                    _mDropDownDialects.onValueChanged.AddListener(delegate {
                        SpeechDetectionUtils.HandleDialectChanged(_mDropDownDialects,
                            _mLanguageResult,
                            _mSpeechDetectionPlugin);
                    });
                }

                // Disabled until a language is selected
                SpeechDetectionUtils.DisableDialects(_mDropDownDialects);

                // set the default language
                SpeechDetectionUtils.RestoreLanguage(_mDropDownLanguages);

                // set the default dialect
                SpeechDetectionUtils.RestoreDialect(_mDropDownDialects);
            });            

            // get all the words in the UI
            foreach (Example02Word word in GameObject.FindObjectsOfType<Example02Word>())
            {
                Text text = word.GetComponentInChildren<Text>();
                if (text)
                {
                    _mWords[text.text.ToLower()] = word;
                }
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
                    foreach (KeyValuePair<string, Example02Word> kvp in _mWords)
                    {
                        if (lower.Contains(kvp.Key))
                        {
                            kvp.Value.Highlight();
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
