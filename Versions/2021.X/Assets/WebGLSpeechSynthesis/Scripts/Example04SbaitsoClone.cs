using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityWebGLSpeechSynthesis
{
    public class Example04SbaitsoClone : MonoBehaviour
    {
        public Text _mTextWaiting = null;
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
        /// Reference to the proxy
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

        void CreateText(string msg)
        {
            GameObject go = new GameObject("Text");
            go.transform.SetParent(_mPanelForText.transform);
            Text text = go.AddComponent<Text>();
            text.fontSize = 24;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = msg;
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
                if (!string.IsNullOrEmpty(inputField.text) &&
                    Input.GetKeyUp(KeyCode.Return))
                {
                    _mName = inputField.text;
                    Destroy(go);
                    _mState = States.Outro;
                    CreateText(_mName);
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
                if (!string.IsNullOrEmpty(inputField.text) &&
                    Input.GetKeyUp(KeyCode.Return))
                {
                    string question = inputField.text;
                    Destroy(go);
                    CreateText(question);
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
            CreateText(msg);
            Speak(msg);
        }

        /// <summary>
        /// Save a reference to the script to be accessed outside the class
        /// </summary>
        private static Example04SbaitsoClone _sInstance = null;

        /// <summary>
        /// Get the example instance
        /// </summary>
        /// <returns></returns>
        public static Example04SbaitsoClone GetInstance()
        {
            return _sInstance;
        }

        private void Awake()
        {
            // set instance
            _sInstance = this;
        }

        // Use this for initialization
        IEnumerator Start()
        {
            // get the singleton instance
            _mSpeechSynthesisPlugin = SpeechSynthesisUtils.GetInstance();

            // check the reference to the plugin
            if (null == _mSpeechSynthesisPlugin)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.LogError("WebGL Speech Synthesis Plugin is not set!");
#else
                Debug.LogError("Proxy Speech Synthesis Plugin is not set!");
#endif
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

            // wait for proxy to become available
            while (!_mSpeechSynthesisPlugin.IsAvailable())
            {
                yield return null;
            }

            // subscribe to events
            _mSpeechSynthesisPlugin.AddListenerSynthesisOnEnd(HandleSynthesisOnEnd);

            // hide the waiting text
            SpeechSynthesisUtils.SetActive(false, _mTextWaiting);

            // Get voices from proxy
            StartCoroutine(GetVoices());

            // Create an instance of SpeechSynthesisUtterance
            _mSpeechSynthesisPlugin.CreateSpeechSynthesisUtterance((utterance) =>
            {
                //Debug.LogFormat("Utterance created: {0}", utterance._mReference);
                _mSpeechSynthesisUtterance = utterance;
            });

            while (null == _mSpeechSynthesisUtterance ||
                null == _mVoiceResult)
            {
                //wait
                yield return null;
            }

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
