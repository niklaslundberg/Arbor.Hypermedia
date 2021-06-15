namespace Arbor.Hypermedia
{
    public class SelectionOption
    {
        public SelectionOption(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }
    }
}