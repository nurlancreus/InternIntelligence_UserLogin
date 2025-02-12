namespace InternIntelligence_UserLogin.Core.Options.Token
{
    public class TokenSettings
    {
        public AccessSettings Access { get; set; } = null!;
        public RefreshSettings Refresh { get; set; } = null!;
    }
}
