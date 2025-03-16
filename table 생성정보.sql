CREATE TABLE email_queue (
    EmailTranId       INT AUTO_INCREMENT PRIMARY KEY,
    UserId           VARCHAR(25) NOT NULL,  -- 사용자 ID 또는 매장 코드
    AuthKey          VARCHAR(100) NOT NULL, -- 인증 키
    CampaignId       VARCHAR(50) NULL,      -- 캠페인 ID (선택)
    MsgId            VARCHAR(50) NULL,      -- 메시지 ID (선택)
    SendOrder        INT NOT NULL DEFAULT 1, -- 보낸 순서
    Subject          NVARCHAR(200) NULL,    -- 이메일 제목
    FromName        NVARCHAR(50) NULL,     -- 발신자 이름
    FromEmail       NVARCHAR(50) NOT NULL, -- 발신자 이메일
    ToName          NVARCHAR(50) NULL,     -- 수신자 이름
    ToEmail         NVARCHAR(50) NOT NULL, -- 수신자 이메일
    Body            TEXT NOT NULL,         -- 이메일 본문
    IsHtml          BOOLEAN NOT NULL DEFAULT TRUE, -- HTML 여부
    Priority        VARCHAR(10) NULL,      -- 우선순위 (Low, Normal, High)
    SMTPHost        VARCHAR(50) NULL,      -- SMTP 서버
    SMTPPort        INT NULL,              -- SMTP 포트
    SMTPEnableSsl   BOOLEAN NOT NULL DEFAULT TRUE, -- SSL 사용 여부
    SMTPUserName    VARCHAR(50) NULL,      -- SMTP 계정명
    SMTPUserPassword VARCHAR(50) NULL,     -- SMTP 비밀번호
    Result          VARCHAR(10) NULL,      -- 수신 결과
    IsDelete        BOOLEAN NOT NULL DEFAULT FALSE, -- 삭제 여부
    RegDate         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 작성일
    ScheduleDate    DATETIME NULL,         -- 예약 전송일
    SendDate        DATETIME NULL,         -- 실제 전송일
    IsSend          BOOLEAN NOT NULL DEFAULT FALSE, -- 전송 여부
    FrmId           VARCHAR(50) NULL,      -- 폼 ID
    bs_cd           VARCHAR(10) NULL,      -- 사업장 코드
    fac_cd          VARCHAR(10) NULL       -- 공장 코드
);




CREATE TABLE email_log (
    LogId           INT AUTO_INCREMENT PRIMARY KEY,
    EmailTranId     INT NOT NULL,           -- 이메일 전송 키 참조
    Recipient       VARCHAR(50) NOT NULL,   -- 수신자 이메일
    Subject         NVARCHAR(200) NOT NULL, -- 이메일 제목
    Body            TEXT NOT NULL,          -- 이메일 본문
    Status         ENUM('SENT', 'FAILED') NOT NULL, -- 전송 상태
    ErrorMessage   TEXT NULL,               -- 오류 메시지 (실패 시 저장)
    SentAt         DATETIME DEFAULT CURRENT_TIMESTAMP, -- 전송된 시간
    FOREIGN KEY (EmailTranId) REFERENCES email_queue(EmailTranId) ON DELETE CASCADE
);



CREATE TABLE email_cc (
    Id             INT AUTO_INCREMENT PRIMARY KEY,
    EmailTranId    INT NOT NULL,  -- 참조하는 이메일 전송 ID
    CcEmail        VARCHAR(50) NOT NULL,  -- 참조자 이메일
    FOREIGN KEY (EmailTranId) REFERENCES email_queue(EmailTranId) ON DELETE CASCADE
);

