using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
namespace BlazorApp2.Data.Services.Mails
{
    public class MailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendVerificationCodeAsync(string email, string code)
        {
            var mailSettings = _configuration.GetSection("MailSettings");

            // 创建 MimeMessage 代替 MailMessage
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                mailSettings["DisplayName"],
                mailSettings["Mail"]
            ));
            message.To.Add(new MailboxAddress(email, email));
            message.Subject = "您的验证码";
            // 设置邮件正文
            message.Body = new TextPart("plain")
            {
                Text = $"您的验证码是: {code}，有效期为5分钟。"
            };

            using var smtpClient = new SmtpClient();

            try
            {
                // 连接到 SMTP 服务器
                await smtpClient.ConnectAsync(
                    mailSettings["Host"],
                    int.Parse(mailSettings["Port"]),
                    MailKit.Security.SecureSocketOptions.SslOnConnect
                );

                // 认证
                await smtpClient.AuthenticateAsync(
                    mailSettings["Mail"],
                    mailSettings["Password"]
                );

                // 发送邮件
                await smtpClient.SendAsync(message);
            }
            finally
            {
                // 断开连接
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
   
}
