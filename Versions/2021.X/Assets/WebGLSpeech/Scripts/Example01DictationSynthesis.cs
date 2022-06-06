using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityWebGLSpeechDetection;
using UnityWebGLSpeechSynthesis;

namespace UnityWebGLSpeech
{
    public class Example01DictationSynthesis : MonoBehaviour
    {
        /// <summary>
        /// Reference to a warning if plugin is not available
        /// </summary>
        public Text _mTextWarning = null;

        /// <summary>
        /// Reference to the example text summary
        /// </summary>
        public Text _mTextSummary = null;

        /// <summary>
        /// Detection: Dropdown selector for languages
        /// </summary>
        public Dropdown _mDropDownLanguages = null;

        /// <summary>
        /// Detection: Dropdown selector for dialects
        /// </summary>
        public Dropdown _mDropDownDialects = null;

		///
		/// Continue button
		///
		public Button _mButtonContinue = null;

        /// <summary>
        /// Detection: Resume detection
        /// </summary>
        public Button _mButtonStartDetection = null;

        /// <summary>
        /// Detection: Pause detection
        /// </summary>
        public Button _mButtonStopDetection = null;

        /// <summary>
        /// Synthesis: Dropdown selector for voices
        /// </summary>
        public Dropdown _mDropdownVoices = null;

        /// <summary>
        /// Synthesis: Slider for rate
        /// </summary>
        public Slider _mSliderRate = null;

        /// <summary>
        /// Synthesis: Slider for pitch
        /// </summary>
        public Slider _mSliderPitch = null;

        /// <summary>
        /// Synthesis: Slider for volume
        /// </summary>
        public Slider _mSliderVolume = null;

        /// <summary>
        /// Volume of speaking
        /// </summary>
        private float _mVolume = 1f;

        /// <summary>
        /// Reference to the text that will be detected and then spoken
        /// </summary>
        public Text _mTextDictation = null;

        /// <summary>
        /// Launch proxy button
        /// </summary>
        public Button _mButtonLaunchProxy = null;

        /// <summary>
        /// Open browser tab button
        /// </summary>
        public Button _mButtonOpenBrowserTab = null;

        /// <summary>
        /// Close proxy button
        /// </summary>
        public Button _mButtonCloseProxy = null;

        /// <summary>
        /// Reference to the detection plugin
        /// </summary>
        private ISpeechDetectionPlugin _mSpeechDetectionPlugin = null;

        /// <summary>
        /// Reference to the synthesis plugin
        /// </summary>
        private ISpeechSynthesisPlugin _mSpeechSynthesisPlugin = null;

        /// <summary>
        /// Detection: Reference to the supported languages and dialects
        /// </summary>
        private LanguageResult _mLanguageResult = null;

        /// <summary>
        /// Detection: List of detected words
        /// </summary>
        private List<string> _mWords = new List<string>();

        /// <summary>
        /// Detection: String builder to format the dictation text
        /// </summary>
        private StringBuilder _mStringBuilder = new StringBuilder();

        #region Synthesis

        /// <summary>
        /// Reference to the supported voices
        /// </summary>
        private VoiceResult _mVoiceResult = null;

        /// <summary>
        /// Reference to the utterance which holds the voice and text to speak
        /// </summary>
        private SpeechSynthesisUtterance _mSpeechSynthesisUtterance = null;

        /// <summary>
        /// Only set the pitch on button up
        /// </summary>
        private IEnumerator _mSetPitch = null;

        /// <summary>
        /// Only set the rate on button up
        /// </summary>
        private IEnumerator _mSetRate = null;

        /// <summary>
        /// Track when the utterance is created
        /// </summary>
        private bool _mUtteranceSet = false;

        /// <summary>
        /// Track when the voices are created
        /// </summary>
        private bool _mVoicesSet = false;

        /// <summary>
        /// Call coroutine from update
        /// </summary>
        private bool _mGetVoices = false;

        /// <summary>
        /// The text to speak
        /// </summary>
        private string _mSpeakText = string.Empty;

        /// <summary>
        /// Ignore detection when speaking
        /// </summary>
        private bool _mWaitForOnEnd = false;

        #endregion

        // Use this for initialization
        IEnumerator Start()
        {
            // Hide fields for API that doesn't work yet
            SpeechDetectionUtils.SetActive(false, _mButtonStartDetection, _mButtonStopDetection);

            // Hide fields during setup
            SpeechDetectionUtils.SetActive(false, _mTextSummary, _mTextDictation);

            // deactivate fields during setup
            SpeechSynthesisUtils.SetInteractable(false,
                _mDropDownDialects,
                _mDropDownLanguages,
				_mButtonContinue,
                //_mButtonStartDetection,
                //_mButtonStopDetection,
                _mDropdownVoices,
                _mSliderPitch,
                _mSliderRate,
                _mSliderVolume);

            _mSpeechDetectionPlugin = SpeechDetectionUtils.GetInstance();
            if (_mSpeechDetectionPlugin != null)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
				/*
                if (_mButtonStartDetection)
                {
                    _mButtonStartDetection.onClick.AddListener(() =>
                    {
                        SpeechSynthesisUtils.SetInteractable(false, _mButtonStartDetection);
                        if (_mButtonStopDetection)
                        {
                            SpeechSynthesisUtils.SetInteractable(true, _mButtonStopDetection);
                        }
                        _mSpeechDetectionPlugin.StartRecognition();
                    });
                }
				*/

				/*
                if (_mButtonStopDetection)
                {
                    _mButtonStopDetection.onClick.AddListener(() =>
                    {
                        if (_mButtonStartDetection)
                        {
                            SpeechSynthesisUtils.SetInteractable(true, _mButtonStartDetection);
                        }
                        SpeechSynthesisUtils.SetInteractable(false, _mButtonStopDetection);
                        _mSpeechDetectionPlugin.StopRecognition();
                    });
                }
				*/

                if (_mButtonLaunchProxy)
                {
                    _mButtonLaunchProxy.gameObject.SetActive(true);
                    _mButtonLaunchProxy.onClick.AddListener(() =>
                    {
                        _mSpeechDetectionPlugin.ManagementLaunchProxy();
                    });
                }

                if (_mButtonOpenBrowserTab)
                {
                    _mButtonOpenBrowserTab.gameObject.SetActive(true);
                    _mButtonOpenBrowserTab.onClick.AddListener(() =>
                    {
                        _mSpeechDetectionPlugin.ManagementOpenBrowserTab();
                    });
                }

                if (_mButtonCloseProxy)
                {
                    _mButtonCloseProxy.gameObject.SetActive(true);
                    _mButtonCloseProxy.onClick.AddListener(() =>
                    {
                        _mSpeechDetectionPlugin.ManagementCloseProxy();
                    });
                }
#endif

				if (_mButtonContinue)
				{
					_mButtonContinue.gameObject.SetActive(true);
					_mButtonContinue.onClick.AddListener(() =>
					{
						// ready to speak again
						_mWaitForOnEnd = false;
					});
				}
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            if (_mTextWarning)
            {
                _mTextWarning.text = "This example requires the Chrome Speech Proxy to be listening on the specified port.";
            }
#endif
            if (null == _mSpeechDetectionPlugin)
            {
                Debug.LogError("Speech Detection Plugin is not set!");
                yield break;
            }

            // wait for plugin to become available
            while (!_mSpeechDetectionPlugin.IsAvailable())
            {
                yield return null;
            }

            // get singleton instance
            _mSpeechSynthesisPlugin = SpeechSynthesisUtils.GetInstance();
            if (null == _mSpeechSynthesisPlugin)
            {
                Debug.LogError("Speech Synthesis Plugin is not set!");
                yield break;
            }

            // wait for proxy to become available
            while (!_mSpeechSynthesisPlugin.IsAvailable())
            {
                yield return null;
            }

            // subscribe to events
            _mSpeechSynthesisPlugin.AddListenerSynthesisOnEnd(HandleSynthesisOnEnd);

            // hide warning now that plugins are detected
            SpeechDetectionUtils.SetActive(false, _mTextWarning);

            // show example summary
            SpeechDetectionUtils.SetActive(true, _mTextSummary, _mTextDictation);


            // subscribe to events
            _mSpeechDetectionPlugin.AddListenerOnDetectionResult(HandleDetectionResult);

            // Get languages from detection plugin
            _mSpeechDetectionPlugin.GetLanguages((languageResult) =>
            {
                _mLanguageResult = languageResult;

                // prepare the language drop down items
                SpeechDetectionUtils.PopulateLanguagesDropdown(_mDropDownLanguages, _mLanguageResult);

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

                // reenable events after setting the default
                SpeechDetectionUtils.RestoreDialect(_mDropDownDialects);

                // reactivate fields after setup
                SpeechSynthesisUtils.SetInteractable(true,
                    _mDropDownDialects,
                    _mDropDownLanguages);
            });

            // reactivate fields after setup
            SpeechSynthesisUtils.SetInteractable(true,
				_mButtonContinue /*,
                _mButtonStartDetection,
                _mButtonStopDetection */);


            // Get voices from synthesis plugin
            StartCoroutine(GetVoices());

            // Create an instance of SpeechSynthesisUtterance
            _mSpeechSynthesisPlugin.CreateSpeechSynthesisUtterance((utterance) =>
            {
                //Debug.LogFormat("Utterance created: {0}", utterance._mReference);
                _mSpeechSynthesisUtterance = utterance;

                // enable the rest of the UI
                SpeechSynthesisUtils.SetInteractable(true,
                    _mSliderPitch, _mSliderRate, _mSliderVolume);

                // The utterance is set
                _mUtteranceSet = true;

                // Set default voice if ready
                SetIfReadyForDefaultVoice();
            });

            if (_mSliderPitch)
            {
                _mSliderPitch.onValueChanged.AddListener((val) =>
                {
                    //Debug.Log("Pitch: " + val);
                    _mSetPitch = SetPitch(Mathf.Lerp(0.1f, 2f, val));
                });
            }

            if (_mSliderRate)
            {
                _mSliderRate.onValueChanged.AddListener((val) =>
                {
                    //Debug.Log("Rate: " + val);
                    _mSetRate = SetRate(Mathf.Lerp(0.1f, 2f, val));
                });
            }

            if (_mSliderVolume)
            {
                _mSliderVolume.onValueChanged.AddListener((val) =>
                {
                    //Debug.Log("Volume: " + val);
                    _mVolume = val;
                });
            }
        }

        /// <summary>
        /// Fire the on end event when speaking is finished
        /// </summary>
        /// <param name="speechSynthesisEvent"></param>
        void HandleSynthesisOnEnd(SpeechSynthesisEvent speechSynthesisEvent)
        {
            //Debug.Log("HandleSynthesisOnEnd:");
            if (null == speechSynthesisEvent)
            {
                return;
            }

            // ready to speak again
            _mWaitForOnEnd = false;
        }

        /// <summary>
        /// Handler for speech detection events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        bool HandleDetectionResult(DetectionResult detectionResult)
        {
            //ignore detection results when speaking
            if (_mWaitForOnEnd)
            {
                _mSpeechDetectionPlugin.Abort();
                return true;
            }
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
                        _mWords.Add(string.Format("[FINAL] \"{0}\" Confidence={1}",
                            alternative.transcript,
                            alternative.confidence));

                        _mSpeakText = alternative.transcript;
                        Speak();
                    }
                    else
                    {
                        _mWords.Add(string.Format("\"{0}\" Confidence={1}",
                            alternative.transcript,
                            alternative.confidence));
                    }
                }
            }
            while (_mWords.Count > 15)
            {
                _mWords.RemoveAt(0);
            }

            if (_mTextDictation)
            {
                if (_mStringBuilder.Length > 0)
                {
                    _mStringBuilder.Remove(0, _mStringBuilder.Length);
                }
                foreach (string text in _mWords)
                {
                    _mStringBuilder.AppendLine(text);
                }
                _mTextDictation.text = _mStringBuilder.ToString();
            }

            // dictation doesn't need to handle the event
            return false;
        }

        /// <summary>
        /// Get available voices for synthesis
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetVoices()
        {
            // wait for results
            yield return new WaitForSeconds(0.25f);

            _mSpeechSynthesisPlugin.GetVoices((voiceResult) =>
            {
                if (null == voiceResult)
                {
                    //retry
                    _mGetVoices = true;
                    return;
                }
                _mVoiceResult = voiceResult;

                // prepare the voices drop down items
                SpeechSynthesisUtils.PopulateVoicesDropdown(_mDropdownVoices, _mVoiceResult);

                // The voices are set
                _mVoicesSet = true;

                // Set the default voice if ready
                SetIfReadyForDefaultVoice();
            });
        }

        /// <summary>
        /// Set the default voice if voices and utterance are ready
        /// </summary>
        private void SetIfReadyForDefaultVoice()
        {
            if (_mVoicesSet &&
                _mUtteranceSet)
            {
                // ready to handle voice changes
                if (_mDropdownVoices)
                {
                    // set up the drop down change listener
                    _mDropdownVoices.onValueChanged.AddListener(delegate {
                        // handle the voice change event, and set the voice on the utterance
                        SpeechSynthesisUtils.HandleVoiceChangedDropdown(_mDropdownVoices,
                            _mVoiceResult,
                            _mSpeechSynthesisUtterance,
                            _mSpeechSynthesisPlugin);
                        // Speak in the new voice
                        Speak();
                    });
                }

                // set the default voice
                SpeechSynthesisUtils.RestoreVoice(_mDropdownVoices);

                // enable voices dropdown
                SpeechSynthesisUtils.SetInteractable(true, _mDropdownVoices);
            }
        }


        // <summary>
        /// Speak the utterance
        /// </summary>
        private void Speak()
        {
            if (null == _mSpeechSynthesisUtterance)
            {
                Debug.LogError("Utterance is not set!");
                return;
            }

            if (string.IsNullOrEmpty(_mSpeakText))
            {
                return;
            }

            // Cancel if already speaking
            _mSpeechSynthesisPlugin.Cancel();

            // Set the text that will be spoken
            _mSpeechSynthesisPlugin.SetText(_mSpeechSynthesisUtterance, _mSpeakText);

            // Wait for speaking to end before doing detection
            _mWaitForOnEnd = true;

            _mSpeechSynthesisPlugin.SetVolume(_mSpeechSynthesisUtterance, _mVolume);

            // Use the plugin to speak the utterance
            _mSpeechSynthesisPlugin.Speak(_mSpeechSynthesisUtterance);
        }

        /// <summary>
        /// Set the pitch on mouse up when using the slider
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private IEnumerator SetPitch(float val)
        {
            if (null != _mSpeechSynthesisUtterance)
            {
                _mSpeechSynthesisPlugin.SetPitch(_mSpeechSynthesisUtterance, val);
                Speak();
            }
            yield break;
        }

        /// <summary>
        /// Set the rate on mouse up when using the slider
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private IEnumerator SetRate(float val)
        {
            if (null != _mSpeechSynthesisUtterance)
            {
                _mSpeechSynthesisPlugin.SetRate(_mSpeechSynthesisUtterance, val);
                Speak();
            }
            yield break;
        }

        /// <summary>
        /// Only set pitch or set rate on mouse up
        /// </summary>
        private void FixedUpdate()
        {
            if (null != _mSetPitch)
            {
                if (!Input.GetMouseButton(0))
                {
                    StartCoroutine(_mSetPitch);
                    _mSetPitch = null;
                }
            }
            if (null != _mSetRate)
            {
                if (!Input.GetMouseButton(0))
                {
                    StartCoroutine(_mSetRate);
                    _mSetRate = null;
                }
            }
            if (_mGetVoices)
            {
                _mGetVoices = false;
                StartCoroutine(GetVoices());
            }
        }
    }
}
