namespace PhonePeIntegrationApp.Settings
{
    public class PhonePeSettings
    {
        public const string SettingSection = "PhonePeSettings";
        public string MerchentId { get; set; }
        public string MerchentSecretKey { get; set; }
        public string PaymentApiUrl { get; set; }
        public string ApiEndpoint { get; set; }
        public int SaltIndex { get; set; }
    }
}
