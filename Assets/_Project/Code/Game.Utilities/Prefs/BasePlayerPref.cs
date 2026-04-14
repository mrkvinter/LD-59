namespace Code.Prefs
{
    public abstract class BasePlayerPref<T>
    {
        protected readonly string key;
        protected readonly T defaultValue;
        
        public abstract T Value { get; set; }

        protected BasePlayerPref(string key, T defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }
    }
}