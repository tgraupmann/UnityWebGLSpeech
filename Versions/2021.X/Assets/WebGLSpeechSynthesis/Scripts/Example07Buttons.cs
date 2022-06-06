using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
    public class Example07Buttons : MonoBehaviour
    {
        private static readonly string[] _sButtons =
        {
            "Hello! Text to speech is great! Thumbs up!",
            "Say something else",
            "Run from the dinosaur!",
            "The bridge is about to collapse!",
        };

        private void OnGUI()
        {
            GUILayout.BeginVertical(GUILayout.Height(Screen.height));
            GUILayout.FlexibleSpace();

            foreach (string button in _sButtons)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(button, GUILayout.Height(40)))
                {
                    Example07Simple.GetInstance().Speak(button);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

    }
}