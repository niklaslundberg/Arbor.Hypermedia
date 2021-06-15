namespace Arbor.Hypermedia
{
    public class CustomHttpMethod
    {
        public static readonly CustomHttpMethod Get = new("GET");
        public static readonly CustomHttpMethod Post = new("POST");
        public static readonly CustomHttpMethod Delete = new("DELETE");
        public static readonly CustomHttpMethod Put = new("PUT");
        public static readonly CustomHttpMethod Head = new("HEAD");
        public static readonly CustomHttpMethod Options = new("OPTIONS");

        private CustomHttpMethod(string method) => Method = method;

        public string Method { get; }

        public override string ToString() => Method;
    }
}