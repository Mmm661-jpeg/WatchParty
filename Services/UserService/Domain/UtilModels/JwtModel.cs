namespace UserService.Domain.UtilModels
{
    public class JwtModel
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Expiration { get; set; }

        public string AdminUsername { get; set; }
    }
}
