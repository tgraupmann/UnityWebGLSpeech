using System.Collections;
using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
    public class Example06NoGUI : MonoBehaviour
    {
        /// <summary>
        /// Reference to the plugin
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
        /// Text of string to speak
        /// </summary>
        private string _mTextToSpeak = string.Empty;

        /// <summary>
        /// Save a reference to the script to be accessed outside the class
        /// </summary>
        private static Example06NoGUI _sInstance = null;

        /// <summary>
        /// Get the example instance
        /// </summary>
        /// <returns></returns>
        public static Example06NoGUI GetInstance()
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

            // wait for proxy to become available
            while (!_mSpeechSynthesisPlugin.IsAvailable())
            {
                yield return null;
            }

            // set default speech text
            _mTextToSpeak = "Hello! Text to speech is great! Thumbs up!";

            // subscribe to events
            _mSpeechSynthesisPlugin.AddListenerSynthesisOnEnd(HandleSynthesisOnEnd);

            // Get voices from plugin
            StartCoroutine(GetVoices());

            // Create an instance of SpeechSynthesisUtterance
            _mSpeechSynthesisPlugin.CreateSpeechSynthesisUtterance((utterance) =>
            {
                //Debug.LogFormat("Utterance created: {0}", utterance._mReference);
                _mSpeechSynthesisUtterance = utterance;

                // The utterance is set
                _mUtteranceSet = true;

                // Set default voice if ready
                OnSpeechAPILoaded();
            });
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

                // The voices are set
                _mVoicesSet = true;

                // Speech API is ready to use
                OnSpeechAPILoaded();
            });
        }

        /// <summary>
        /// Speak the utterance
        /// </summary>
        private void Speak()
        {
            if (null == _mSpeechSynthesisUtterance)
            {
                Debug.LogError("Utterance is not set!");
                return;
            }

            // Cancel if already speaking
            _mSpeechSynthesisPlugin.Cancel();

            // Set the text that will be spoken
            _mSpeechSynthesisPlugin.SetText(_mSpeechSynthesisUtterance, _mTextToSpeak);

            // Use the plugin to speak the utterance
            _mSpeechSynthesisPlugin.Speak(_mSpeechSynthesisUtterance);
        }

        /// <summary>
        /// Only set pitch or set rate on mouse up
        /// </summary>
        private void FixedUpdate()
        {
            if (_mGetVoices)
            {
                _mGetVoices = false;
                StartCoroutine(GetVoices());
            }
        }

        /// <summary>
        /// Speech API is ready to use
        /// </summary>
        private void OnSpeechAPILoaded()
        {
            if (_mVoicesSet &&
                _mUtteranceSet &&
                null != _mSpeechSynthesisUtterance)
            {
                // set a random voice
                if (null != _mVoiceResult &&
                    null != _mVoiceResult.voices &&
                    _mVoiceResult.voices.Length > 0)
                {
                    int index = Random.Range(0, _mVoiceResult.voices.Length);
                    Voice voice = _mVoiceResult.voices[index];
                    if (null != voice)
                    {
                        _mSpeechSynthesisPlugin.SetVoice(_mSpeechSynthesisUtterance, voice);
                    }
                }
                
                // set a random rate to speak
                float rate = Random.Range(0.1f, 2f);
                _mSpeechSynthesisPlugin.SetRate(_mSpeechSynthesisUtterance, rate);

                // set a random pitch to speak
                float pitch = Random.Range(0.1f, 2f);
                _mSpeechSynthesisPlugin.SetPitch(_mSpeechSynthesisUtterance, pitch);

                // Speak the utterance
                Speak();
            }
        }

        void HandleSynthesisOnEnd(SpeechSynthesisEvent speechSynthesisEvent)
        {
            // Speak a random voice again
            OnSpeechAPILoaded();
        }
    }
}
