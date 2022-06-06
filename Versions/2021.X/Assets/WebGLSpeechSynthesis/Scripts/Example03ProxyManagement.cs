using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechSynthesis
{
    public class Example03ProxyManagement : MonoBehaviour
    {
        /// <summary>
        /// Reference to the proxy
        /// </summary>
        private ISpeechSynthesisPlugin _mSpeechSynthesisPlugin = null;

        public Button _mButtonCloseBrowserTab = null;

        public Button _mButtonCloseProxy = null;

        public Button _mButtonLaunchProxy = null;

        public Button _mButtonOpenBrowserTab = null;

        public Button _mButtonSetProxyPort = null;

        public InputField _mInputPort = null;

        /// <summary>
        /// Save a reference to the script to be accessed outside the class
        /// </summary>
        private static Example03ProxyManagement _sInstance = null;

        /// <summary>
        /// Get the example instance
        /// </summary>
        /// <returns></returns>
        public static Example03ProxyManagement GetInstance()
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

            // proxy has to launch before being available
            if (_mButtonLaunchProxy)
            {
                _mButtonLaunchProxy.onClick.AddListener(() =>
                {
                    _mSpeechSynthesisPlugin.ManagementLaunchProxy();
                });
            }

            // wait for plugin to become available
            while (!_mSpeechSynthesisPlugin.IsAvailable())
            {
                yield return null;
            }

            if (_mButtonCloseBrowserTab)
            {
                _mButtonCloseBrowserTab.onClick.AddListener(() =>
                {
                    _mSpeechSynthesisPlugin.ManagementCloseBrowserTab();
                });
            }

            if (_mButtonCloseProxy)
            {
                _mButtonCloseProxy.onClick.AddListener(() =>
                {
                    _mSpeechSynthesisPlugin.ManagementCloseProxy();
                });
            }

            if (_mButtonOpenBrowserTab)
            {
                _mButtonOpenBrowserTab.onClick.AddListener(() =>
                {
                    _mSpeechSynthesisPlugin.ManagementOpenBrowserTab();
                });
            }

            if (_mButtonSetProxyPort &&
                _mInputPort)
            {
                _mButtonSetProxyPort.onClick.AddListener(() =>
                {
                    int port;
                    if (int.TryParse(_mInputPort.text, out port))
                    {
                        _mSpeechSynthesisPlugin.ManagementSetProxyPort(port);
                    }
                    else
                    {
                        _mInputPort.text = "5000";
                    }
                });
            }
        }
    }
}
