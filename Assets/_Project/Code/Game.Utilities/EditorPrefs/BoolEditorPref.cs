namespace Code.Game.Utilities.EditorPrefs
{
    public class BoolEditorPref : BaseEditorPref<bool>
    {
        public BoolEditorPref(string key, bool defaultValue = false) : base(key, defaultValue)
        {
        }

        protected override void SetValue(bool value)
        {
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetBool(key, value);
#endif
        }

        protected override bool GetValue()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetBool(key, defaultValue);
#else
            return defaultValue;
#endif
        }
    }
}