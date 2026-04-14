namespace Code.Prefs
{
    public class BoolPlayerPref : BasePlayerPref<bool>
    {
        public BoolPlayerPref(string key, bool defaultValue = false) : base(key, defaultValue)
        {
        }

        public override bool Value
        {
            get => UnityEngine.PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
            set => UnityEngine.PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }
}