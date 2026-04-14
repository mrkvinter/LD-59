namespace Code.Game.Utilities.EditorPrefs
{
    public class StringEditorPref : BaseEditorPref<string>
    {
        public StringEditorPref(string key, string defaultValue = null) : base(key, defaultValue)
        {
        }

        protected override void SetValue(string value)
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetString(key, value);
#endif
        }

        protected override string GetValue()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetString(key, defaultValue);
#else
            return defaultValue;
#endif
        }
    }
}