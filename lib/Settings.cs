using System;

namespace lib
{
    // Create Settings.Production.cs file with concrete values set in default constructor.
    public partial class Settings
    {
        public string GoogleAuthJson;
        public string YandexCloudStaticKeyId;
        public string YandexCloudStaticKey;
        public string YandexCloudKeyFile;
        public string YdbEndpoint;
        public string YdbDatabase;
        public string ApiToken;
    }
}
