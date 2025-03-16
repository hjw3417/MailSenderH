using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailSenderH.Entities
{
    public class EmailData
    {
        /// <summary>
        /// 이메일 전송 ID (Primary Key, email_queue 테이블의 EmailTranId)
        /// </summary>
        public int EmailTranId { get; set; }

        /// <summary>
        /// 수신자 이메일 주소
        /// </summary>
        public string ToEmail { get; set; }

        /// <summary>
        /// 이메일 제목 (NULL 가능)
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 이메일 본문 (HTML 또는 일반 텍스트)
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 이메일 본문이 HTML인지 여부 (true: HTML, false: 일반 텍스트)
        /// </summary>
        public bool IsHtml { get; set; }

        /// <summary>
        /// 이메일을 전송할 SMTP 서버 주소 (예: smtp.gmail.com)
        /// </summary>
        public string SMTPHost { get; set; }

        /// <summary>
        /// SMTP 서버 포트 번호 (기본값: 25, SSL 사용 시 465 또는 587)
        /// </summary>
        public int SMTPPort { get; set; }

        /// <summary>
        /// SMTP 서버에서 SSL을 활성화할지 여부 (true: SSL 사용, false: 사용 안 함)
        /// </summary>
        public bool SMTPEnableSsl { get; set; }

        /// <summary>
        /// SMTP 서버 로그인 계정 (NULL 가능)
        /// </summary>
        public string SMTPUserName { get; set; }

        /// <summary>
        /// SMTP 서버 로그인 비밀번호 (NULL 가능)
        /// </summary>
        public string SMTPUserPassword { get; set; }
    }
}
