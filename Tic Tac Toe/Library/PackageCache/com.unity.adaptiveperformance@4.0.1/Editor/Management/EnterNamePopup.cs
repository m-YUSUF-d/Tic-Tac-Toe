using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.AdaptivePerformance.Editor
{
    class EnterNamePopup : PopupWindowContent
    {
        public delegate void EnterDelegate(string str);
        readonly EnterDelegate EnterCB;
        private string m_NewProfileName = "New Scaler Profile";
        private bool m_NeedsFocus = true;

        public EnterNamePopup(SerializedProperty profiles, EnterDelegate cb)
        {
            EnterCB = cb;
            List<string> existingProfileNames = new List<string>();
            for (int i = 0; i < profiles.arraySize; i++)
            {
                string profileName = profiles.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue;
                if (!string.IsNullOrEmpty(profileName))
                    existingProfileNames.Add(profileName);
            }
            m_NewProfileName = ObjectNames.GetUniqueName(existingProfileNames.ToArray(), m_NewProfileName);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(400, EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + 14);
        }

        public override void OnGUI(Rect windowRect)
        {
            GUILayout.Space(5);
            Event evt = Event.current;
            bool hitEnter = evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter);
            GUI.SetNextControlName("ProfileName");
            m_NewProfileName = EditorGUILayout.TextField("New Profile Name", m_NewProfileName);

            if (m_NeedsFocus)
            {
                m_NeedsFocus = false;
                EditorGUI.FocusTextInControl("ProfileName");
            }

            GUI.enabled = m_NewProfileName.Length != 0;
            if (GUILayout.Button("Save") || hitEnter)
            {
                EnterCB(m_NewProfileName);
                editorWindow.Close();
            }
        }
    }
}
