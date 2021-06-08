namespace UnityEditor.GameFoundation.Components
{
    internal struct DropdownItem
    {
        public string displayName;
        public string key;
        public DropdownItem(string displayName, string key)
        {
            this.displayName = displayName;
            this.key = key;
        }
    }
}
