using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechDetection
{
    public class Example05ProxyManagement : MonoBehaviour
    {
        /// <summary>
        /// Reference to the proxy
        /// </summary>
        private ISpeechDetectionPlugin _mSpeechDetectionPlugin = null;

        public Button _mButtonCloseBrowserTab = null;

        public Button _mButtonCloseProxy = null;

        public Button _mButtonLaunchProxy = null;

        public Button _mButtonOpenBrowserTab = null;

        public Button _mButtonSetProxyPort = null;

        public InputField _mInputPort = null;

        // Use this for initialization
        IEnumerator Start()
        {
            // get the singleton instance
            _mSpeechDetectionPlugin = SpeechDetectionUtils.GetInstance();

            // check the reference to the plugin
            if (null == _mSpeechDetectionPlugin)
            {
                Debug.LogError("Proxy Speech Detection Plugin is not set!");
                yield break;
            }

            // proxy needs to be launched before it's available
            if (_mButtonLaunchProxy)
            {
                _mButtonLaunchProxy.onClick.AddListener(() =>
                {
                    _mSpeechDetectionPlugin.ManagementLaunchProxy();
                });
            }

            // wait for plugin to become available
            while (!_mSpeechDetectionPlugin.IsAvailable())
            {
                yield return null;
            }

            if (_mButtonCloseBrowserTab)
            {
                _mButtonCloseBrowserTab.onClick.AddListener(() =>
                {
                    _mSpeechDetectionPlugin.ManagementCloseBrowserTab();
                });
            }

            if (_mButtonCloseProxy)
            {
                _mButtonCloseProxy.onClick.AddListener(() =>
                {
                    _mSpeechDetectionPlugin.ManagementCloseProxy();
                });
            }

            if (_mButtonOpenBrowserTab)
            {
                _mButtonOpenBrowserTab.onClick.AddListener(() =>
                {
                    _mSpeechDetectionPlugin.ManagementOpenBrowserTab();
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
                        _mSpeechDetectionPlugin.ManagementSetProxyPort(port);
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
