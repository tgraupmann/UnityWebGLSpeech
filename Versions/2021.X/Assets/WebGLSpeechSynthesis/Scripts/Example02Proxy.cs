using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechSynthesis
{
    public class Example02Proxy : MonoBehaviour
    {
        public Text _mTextWaiting = null;
        public Text _mTextSummary = null;
        public Text _mTextPitch = null;
        public Text _mTextRate = null;
        public Dropdown _mDropdownVoices = null;
        public InputField _mInputField = null;
        public Slider _mSliderPitch = null;
        public Slider _mSliderRate = null;
        public Button _mButtonSpeak = null;
        public Button _mButtonStop = null;

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
        /// Save a reference to the script to be accessed outside the class
        /// </summary>
        private static Example02Proxy _sInstance = null;

        /// <summary>
        /// Get the example instance
        /// </summary>
        /// <returns></returns>
        public static Example02Proxy GetInstance()
        {
            return _sInstance;
        }

        /// <summary>
        /// Hide inactive parts at the beginning
        /// </summary>
        private void Awake()
        {
            // set instance
            _sInstance = this;

            // hide summary until ready
            SpeechSynthesisUtils.SetActive(false, _mTextSummary);

            // Disable until utterance is set
            SpeechSynthesisUtils.SetInteractable(false, 
                _mButtonSpeak, _mButtonStop, 
                _mDropdownVoices, _mSliderPitch, 
                _mSliderRate, _mInputField);
        }

        // Use this for initialization
        IEnumerator Start()
        {
            _mSpeechSynthesisPlugin = SpeechSynthesisUtils.GetInstance();
            if (null == _mSpeechSynthesisPlugin)
            {
                Debug.LogError("Proxy Speech Synthesis Plugin is not set!");
                yield break;
            }

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

            // hide the waiting text
            SpeechSynthesisUtils.SetActive(false, _mTextWaiting);

            // show the summary
            SpeechSynthesisUtils.SetActive(true, _mTextSummary);

            // set default speech text
            if (_mInputField)
            {
                _mInputField.text = "Hello! Text to speech is great! Thumbs up!";
            }

            // Get voices from proxy
            StartCoroutine(GetVoices());

            // Create an instance of SpeechSynthesisUtterance
            _mSpeechSynthesisPlugin.CreateSpeechSynthesisUtterance((utterance) =>
            {
                //Debug.LogFormat("Utterance created: {0}", utterance._mReference);
                _mSpeechSynthesisUtterance = utterance;

                // enable the rest of the UI
                SpeechSynthesisUtils.SetInteractable(true,
                    _mButtonSpeak, _mButtonStop,
                    _mSliderPitch, _mSliderRate,
                    _mInputField);

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

            if (_mButtonSpeak)
            {
                _mButtonSpeak.onClick.AddListener(() =>
                {
                    Speak();
                });
            }

            if (_mButtonStop)
            {
                _mButtonStop.onClick.AddListener(() =>
                {
                    _mSpeechSynthesisPlugin.Cancel();
                });
            }
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

                    // set the default voice
                    SpeechSynthesisUtils.RestoreVoice(_mDropdownVoices);

                    // enable voices dropdown
                    SpeechSynthesisUtils.SetInteractable(true, _mDropdownVoices);
                }
            }
        }

        /// <summary>
        /// Speak the utterance
        /// </summary>
        private void Speak()
        {
            if (null == _mInputField)
            {
                Debug.LogError("InputField is not set!");
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
            _mSpeechSynthesisPlugin.SetText(_mSpeechSynthesisUtterance, _mInputField.text);

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
