using System;

namespace lib
{
    // Create Settings.Production.cs file with concrete values set in default constructor.
    public partial class Settings
    {
        public string GoogleAuthJson = string.Empty;
        public string YandexCloudStaticKeyId = string.Empty;
        public string YandexCloudStaticKey = string.Empty;
        public string YandexCloudKeyFile = string.Empty;
        public string YdbEndpoint = string.Empty;
        public string YdbDatabase = string.Empty;
        public string ApiToken = string.Empty;
    }
}
