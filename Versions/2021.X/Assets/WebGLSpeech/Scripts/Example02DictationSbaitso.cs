using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityWebGLSpeechDetection;
using UnityWebGLSpeechSynthesis;

namespace UnityWebGLSpeech
{
    public class Example02DictationSbaitso : MonoBehaviour
    {
        /// <summary>
        /// Launch proxy button
        /// </summary>
        public Button _mButtonLaunchProxy = null;

        /// <summary>
        /// Open browser tab button
        /// </summary>
        public Button _mButtonOpenBrowserTab = null;

		///
		/// Continue button
		///
		public Button _mButtonContinue = null;

        public Text _mTextWarning = null;
        public GameObject _mPanelForText = null;
        public InputField _mPrefabInputField = null;
        
        private bool _mDoGetVoices = false;
        private bool _mWaitForOnEnd = false;
        private List<GameObject> _mTextLines = new List<GameObject>();
        private string _mName = string.Empty;

        enum States
        {
            Intro,
            NamePrompt,
            WaitForName,
            Outro,
            Talking,
        }

        private States _mState = States.Intro;

        /// <summary>
        /// Reference to the detection plugin
        /// </summary>
        private ISpeechDetectionPlugin _mSpeechDetectionPlugin = null;

        /// <summary>
        /// Reference to the synthesis plugin
        /// </summary>
        private ISpeechSynthesisPlugin _mSpeechSynthesisPlugin = null;

        /// <summary>
        /// Reference to the supported voices
        /// </summary>
        private VoiceResult _mVoiceResult = null;

        /// <summary>
        /// Reference to the utterance which holds the voice and text to speak
        /// </summary>
        private SpeechSynthesisUtterance _mSpeechSynthesisUtterance = null;

        /// <summary>
        /// Show temporary detection results in the input field
        /// </summary>
        private string _mTempResult = string.Empty;

        /// <summary>
        /// Final detection event sets the result
        /// </summary>
        private string _mDetectionResult = string.Empty;

        void CreateText(string msg, Color color)
        {
            GameObject go = new GameObject("Text");
            go.transform.SetParent(_mPanelForText.transform);
            Text text = go.AddComponent<Text>();
            text.fontSize = 24;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = msg;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            go.AddComponent<ContentSizeFitter>();
            _mTextLines.Add(text.gameObject);

            if (_mTextLines.Count > 5)
            {
                Destroy(_mTextLines[0]);
                _mTextLines.RemoveAt(0);
            }
        }

        IEnumerator CreateNameInputField()
        {
            yield return new WaitForSeconds(1);
            GameObject go = Instantiate(_mPrefabInputField.gameObject);
            InputField inputField = go.GetComponent<InputField>();
            go.transform.SetParent(_mPanelForText.transform);

            EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
            
            while (true)
            {
                if (!string.IsNullOrEmpty(_mTempResult))
                {
                    inputField.text = _mTempResult;
                    _mTempResult = string.Empty;
                }
                if ((!string.IsNullOrEmpty(inputField.text) &&
                    Input.GetKeyUp(KeyCode.Return)) ||
                    !string.IsNullOrEmpty(_mDetectionResult))
                {
                    if (!string.IsNullOrEmpty(_mDetectionResult))
                    {
                        inputField.text = _mDetectionResult;
                        _mDetectionResult = string.Empty;
                    }
                    _mName = inputField.text;
                    Destroy(go);
                    _mState = States.Outro;
                    CreateText(_mName, Color.red);
                    yield break;
                }
                yield return null;
            }
        }

        IEnumerator CreateTalkInputField()
        {
            yield return new WaitForSeconds(0.1f);

            GameObject go = Instantiate(_mPrefabInputField.gameObject);
            InputField inputField = go.GetComponent<InputField>();
            go.transform.SetParent(_mPanelForText.transform);

            EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);

            while (true)
            {
                if (!string.IsNullOrEmpty(_mTempResult))
                {
                    inputField.text = _mTempResult;
                    _mTempResult = string.Empty;
                }
                if ((!string.IsNullOrEmpty(inputField.text) &&
                    Input.GetKeyUp(KeyCode.Return)) ||
                    !string.IsNullOrEmpty(_mDetectionResult))
                {
                    if (!string.IsNullOrEmpty(_mDetectionResult))
                    {
                        inputField.text = _mDetectionResult;
                        _mDetectionResult = string.Empty;
                    }
                    string question = inputField.text;
                    Destroy(go);
                    CreateText(question, Color.red);
                    string answer = AISbaitso.GetResponse(question);
                    CreateTextAndSpeak(answer);
                    StartCoroutine(CreateTalkInputField());
                    yield break;
                }
                yield return null;
            }
        }

        void CreateTextAndSpeak(string msg)
        {
            _mWaitForOnEnd = true;
            CreateText(msg, Color.white);
            Speak(msg);
        }

        // Use this for initialization
        IEnumerator Start()
        {
            _mSpeechDetectionPlugin = SpeechDetectionUtils.GetInstance();
            if (_mSpeechDetectionPlugin != null)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
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

            _mSpeechSynthesisPlugin = SpeechSynthesisUtils.GetInstance();
            if (null == _mSpeechSynthesisPlugin)
            {
                Debug.LogError("Speech Synthesis Plugin is not set!");
                yield break;
            }

            if (null == _mPanelForText)
            {
                Debug.LogError("Panel for text not set!");
                yield break;
            }

            if (null == _mPrefabInputField)
            {
                Debug.LogError("Prefab Input Field not set!");
                yield break;
            }

            // wait for detection plugin to become available
            while (!_mSpeechDetectionPlugin.IsAvailable())
            {
                yield return null;
            }

            // wait for synthesis plugin to become available
            while (!_mSpeechSynthesisPlugin.IsAvailable())
            {
                yield return null;
            }

            // subscribe to events
            _mSpeechSynthesisPlugin.AddListenerSynthesisOnEnd(HandleSynthesisOnEnd);

            // hide the waiting text
            SpeechSynthesisUtils.SetActive(false, _mTextWarning);

            // Get voices from proxy
            StartCoroutine(GetVoices());

            // Create an instance of SpeechSynthesisUtterance
            _mSpeechSynthesisPlugin.CreateSpeechSynthesisUtterance((utterance) =>
            {
                //Debug.LogFormat("Utterance created: {0}", utterance._mReference);
                _mSpeechSynthesisUtterance = utterance;
            });

            // wait for utterance to be setup
            while (null == _mSpeechSynthesisUtterance ||
                null == _mVoiceResult)
            {
                //wait
                yield return null;
            }

            // subscribe to detection events
            _mSpeechDetectionPlugin.AddListenerOnDetectionResult(HandleDetectionResult);

            while (true)
            {
                if (_mWaitForOnEnd)
                {
                    yield return null;
                }
                else
                {
                    switch (_mState)
                    {
                        case States.Intro:
                            CreateTextAndSpeak("Dr. Sbaitso, by Creative Labs.");
                            _mState = States.NamePrompt;
                            break;
                        case States.NamePrompt:
                            CreateTextAndSpeak("Please enter your name...");
                            _mState = States.WaitForName;
                            StartCoroutine(CreateNameInputField());
                            break;
                        case States.Outro:
                            CreateTextAndSpeak(string.Format("Hello {0}, my name is Dr. Sbaitso.", _mName));
                            while (_mWaitForOnEnd)
                                yield return null;
                            CreateTextAndSpeak("I am here to help you.");
                            while (_mWaitForOnEnd)
                                yield return null;
                            CreateTextAndSpeak("Say whatever is in your mind freely.");
                            while (_mWaitForOnEnd)
                                yield return null;
                            CreateTextAndSpeak("Our conversation will be kept in strict confidence.");
                            while (_mWaitForOnEnd)
                                yield return null;
                            CreateTextAndSpeak("Memory contents will be wiped off after you leave.");
                            while (_mWaitForOnEnd)
                                yield return null;
                            CreateTextAndSpeak("So, tell me about your problems.");
                            while (_mWaitForOnEnd)
                                yield return null;
                            _mState = States.Talking;
                            StartCoroutine(CreateTalkInputField());
                            break;
                    }
                    yield return new WaitForFixedUpdate();
                }
            }
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
                        _mDetectionResult = alternative.transcript;
                        return true; //handled
                    }
                    else
                    {
                        _mTempResult = alternative.transcript;
                    }
                }
            }
            return false;
        }

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
        /// Get available voices
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
                    _mDoGetVoices = true;
                    return;
                }
                _mVoiceResult = voiceResult;
            });
        }

        private void FixedUpdate()
        {
            // retry mechanism to get voices
            if (_mDoGetVoices)
            {
                _mDoGetVoices = false;
                StartCoroutine(GetVoices());
            }
        }

        /// <summary>
        /// Speak the text
        /// </summary>
        private void Speak(string text)
        {
            if (null == text)
            {
                Debug.LogError("Text is not set!");
                return;
            }
            if (null == _mSpeechSynthesisUtterance)
            {
                Debug.LogError("Utterance is not set!");
                return;
            }

            // Cancel if already speaking
            _mSpeechSynthesisPlugin.Cancel();

            // Set the text that will be spoken
            _mSpeechSynthesisPlugin.SetText(_mSpeechSynthesisUtterance, text);

            // Use the plugin to speak the utterance
            _mSpeechSynthesisPlugin.Speak(_mSpeechSynthesisUtterance);
        }
    }
}
