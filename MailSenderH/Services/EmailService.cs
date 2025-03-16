using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using MailSenderH.Entities;
using System.Linq.Expressions;
using MailSenderH.DataAccess;
using MailSenderH.Logging;

namespace MailSenderH.Services
{
    class EmailService
    {
        /// <summary>
        /// 이메일을 전송하는 메서드
        /// </summary>
        /// <param name="emails">이메일 목록 (전송 대기 목록에서 가져옴)</param>
        public void SendEmails()
        {
            Console.WriteLine("email 전송 시작");

            List<EmailData> emails = DatabaseRepository.GetPendingEmails();

            if (emails.Count == 0)
            {
                Console.WriteLine("전송할 이메일이 없습니다.");
                return;
            }

            Console.WriteLine($" {emails.Count}개의 이메일을 전송합니다...");

            foreach (var email in emails)
            {
                try
                {
                    // smtp 클라이언트 설정
                    using (SmtpClient smtp = new SmtpClient(email.SMTPHost, email.SMTPPort))
                    {
                        smtp.EnableSsl = email.SMTPEnableSsl;
                        if (!string.IsNullOrEmpty(email.SMTPUserName) && !string.IsNullOrEmpty(email.SMTPUserPassword))
                        {
                            smtp.Credentials = new NetworkCredential(email.SMTPUserName, email.SMTPUserPassword);
                        }

                        // 메일 메시지 설정    

                        MailMessage message = new MailMessage
                        {
                            From = new MailAddress(email.SMTPUserName), //발신자
                            Subject = email.Subject,
                            Body = email.Body,
                            IsBodyHtml = email.IsHtml
                        };

                        message.To.Add(email.ToEmail);  //수신자 추가

                        List<string> ccEmails = DatabaseRepository.GetEmailCC(email.EmailTranId);
                        foreach (string cc in ccEmails)
                        {
                            message.CC.Add(cc);
                        }
                        Console.WriteLine($"SMTP 서버: {email.SMTPHost}");
                        Console.WriteLine($"SMTP 포트: {email.SMTPPort}");
                        Console.WriteLine($"SSL 사용 여부: {email.SMTPEnableSsl}");
                        Console.WriteLine($"SMTP 계정: {email.SMTPUserName}");
                        //이메일 전송
                        Console.WriteLine($"이메일 전송 중: {email.ToEmail}");

                        smtp.Send(message);

                        Console.WriteLine($"이메일 전송 완료: {email.ToEmail}");
                        // 성공 시 로그 기록
                        FileLogger.LogInfo($"이메일 전송 성공: [From: {email.SMTPUserName}] [To: {email.ToEmail}] [CC: {string.Join(", ", ccEmails)}] [Subject: {email.Subject}]");
                        // 전송 성공 시 DB 로그 기록 후 큐 삭제
                        DatabaseRepository.LogEmailAndDeleteQueue(email.EmailTranId, email.ToEmail, email.Subject, email.Body, "SENT", null);
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"이메일 전송 중 오류 발생: {ex.Message}");

                    // 실패 시 로그 기록
                    FileLogger.LogError($"이메일 전송 실패: [From: {email.SMTPUserName}] [To: {email.ToEmail}] [CC: {string.Join(", ", DatabaseRepository.GetEmailCC(email.EmailTranId))}] [Subject: {email.Subject}] [Error: {ex.Message}]");


                    // 전송 실패 시 DB 로그 기록 후 큐 삭제
                    DatabaseRepository.LogEmailAndDeleteQueue(email.EmailTranId, email.ToEmail, email.Subject, email.Body, "FAILED", ex.Message);
                }
            }
        }
    }
}
