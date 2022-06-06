#if UNITY_2018_1_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine;
#endif

namespace UnityWebGLSpeechDetection
{
    public class WWWPlayMode : IWWW
    {
#if UNITY_2018_1_OR_NEWER
        private UnityWebRequest _mWWW = null;
#else
        private WWW _mWWW = null;
#endif

        public WWWPlayMode(string url)
        {
            //Debug.LogFormat("WWWPlayMode: url={0}", url);
#if UNITY_2018_1_OR_NEWER
            _mWWW = new UnityWebRequest(url);
            _mWWW.downloadHandler = new DownloadHandlerBuffer();
#else
            _mWWW = new WWW(url);
#endif
        }

        public bool IsDone()
        {
            return _mWWW.isDone;
        }

        public string GetError()
        {
#if UNITY_2021 || UNITY_2020
            if (_mWWW.result == UnityWebRequest.Result.ConnectionError)
            {
                return _mWWW.error;
            }
            else
            {
                return null;
            }
#elif UNITY_2018_1_OR_NEWER
            if (_mWWW.isNetworkError || _mWWW.isHttpError)
            {
                return _mWWW.error;
            }
            else
            {
                return null;
            }
#else
            return _mWWW.error;
#endif
        }

        public string GetText()
        {
#if UNITY_2018_1_OR_NEWER
            return _mWWW.downloadHandler.text;
#else
            return _mWWW.text;
#endif
        }

        public void Dispose()
        {
            _mWWW.Dispose();
        }

#if UNITY_2018_1_OR_NEWER
        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            return _mWWW.SendWebRequest();
        }
#endif
    }
}
