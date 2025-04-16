namespace BlazorApp2.Data.Services.Mails
{
    public class VerificationService
    {
        private readonly Dictionary<string, (string Code, DateTime Expiry)> _verificationCodes = new();
        private readonly Random _random = new();

        public string GenerateVerificationCode(string email)
        {
            // 生成6位随机数字
            var code = _random.Next(100000, 999999).ToString();
            var expiry = DateTime.Now.AddMinutes(5); // 5分钟后过期

            _verificationCodes[email] = (code, expiry);

            return code;
        }

        public bool VerifyCode(string email, string code)
        {
            if (!_verificationCodes.TryGetValue(email, out var stored))
                return false;

            // 移除已使用的验证码
            _verificationCodes.Remove(email);

            return stored.Code == code && DateTime.Now <= stored.Expiry;
        }
    }
}
