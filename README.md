# MailSenderH

> DB 큐 기반의 메일 자동 발송 Windows 서비스

`MailSenderH` 는 DB 의 메일 큐 테이블(`email_queue`) 을 일정 주기로 폴링하여,
대기 중인 메일을 SMTP 를 통해 자동으로 발송하고 그 결과를 로그 테이블(`email_log`) 과
파일 로그에 기록하는 **.NET Framework 4.8 기반 Windows Service** 입니다.

---

## 📅 프로젝트 수행 기간

| 항목 | 일자 |
| --- | --- |
| 최초 커밋 (Initial commit) | **2025-03-16** |
| 최종 커밋 (Latest commit) | **2025-03-16** |
| **총 수행 기간** | **2025-03-16 ~ 2025-03-16 (1 일)** |

> 커밋 이력 기준으로 산정된 개발 기간입니다.

```
2025-03-16  e81a577  .gitattributes, .gitignore 및 README.md 추가
2025-03-16  f56e1fd  초기 커밋
2025-03-16  d440f4a  파일 추가
```

---

## 🛠️ 기술 스택

| 분류 | 사용 기술 |
| --- | --- |
| Language | C# |
| Framework | .NET Framework 4.8 |
| Application Type | Windows Service (`System.ServiceProcess`) |
| Database | MariaDB / MySQL (기본), MSSQL 전환 가능 |
| DB Access | ADO.NET `DbProviderFactory` (Provider 동적 로딩) |
| Mail | `System.Net.Mail.SmtpClient` |
| Logging | 자체 구현 `FileLogger` (일자별 텍스트 로그) |
| Installer | Visual Studio Installer Project (`.vdproj`) |
| IDE | Visual Studio 2022 (17.13) |

### 주요 NuGet 패키지

- `MySql.Data` 8.0.33
- `MySqlConnector` 2.4.0
- `System.Configuration.ConfigurationManager` 9.0.3
- `Microsoft.Extensions.Logging.Abstractions` 8.0.2

---

## 📂 프로젝트 구조

```
MailSenderH/
├── MailSenderH.sln                     # Visual Studio 솔루션
├── MailSenderH 프로젝트 개요.docx      # 프로젝트 개요 문서
├── table 생성정보.sql                  # DB 스키마 (email_queue / email_log / email_cc)
│
├── MailSenderH/                        # 메인 Windows Service 프로젝트
│   ├── Program.cs                      # 진입점 (서비스 모드 / 콘솔 테스트 모드 분기)
│   ├── Service.cs                      # ServiceBase 구현, Timer 기반 폴링
│   ├── Service.Designer.cs
│   ├── App.config                      # DB / SMTP / 로그 경로 설정
│   ├── packages.config
│   ├── install_service.bat             # 서비스 등록 스크립트 (sc create)
│   ├── uninstall_service.bat           # 서비스 제거 스크립트 (sc delete)
│   │
│   ├── DataAccess/
│   │   └── DatabaseRepository.cs       # 대기 메일 조회, CC 조회, 전송 결과 로깅 SP 호출
│   ├── Entities/
│   │   └── EmailData.cs                # 메일 큐 DTO
│   ├── Services/
│   │   └── EmailService.cs             # SMTP 전송 로직
│   └── Logging/
│       └── FileLogger.cs               # 일자별 파일 로깅
│
└── MailSenderHSetUp/                   # Visual Studio Installer 프로젝트
    └── MailSenderHSetUp.vdproj
```

---

## 🔄 동작 흐름

```
┌──────────────────────────────┐
│  Windows Service 시작        │
│  (MailSenderHSvc)            │
└──────────────┬───────────────┘
               │  3초 주기 Timer
               ▼
┌──────────────────────────────┐
│  DatabaseRepository          │
│  P_GETPENDINGEMAILS (SP)     │  ◀── email_queue
└──────────────┬───────────────┘
               │ List<EmailData>
               ▼
┌──────────────────────────────┐
│  EmailService.SendEmails()   │
│  - SmtpClient 설정           │
│  - CC 조회 (P_GETEMAILCC)    │
│  - smtp.Send(message)        │
└──────────────┬───────────────┘
               │ 성공/실패
               ▼
┌──────────────────────────────┐
│  P_LOG_EMAIL_STATUS (SP)     │
│  - email_log INSERT          │
│  - email_queue DELETE        │
│  + FileLogger 기록            │
└──────────────────────────────┘
```

1. `Program.Main` 이 `Environment.UserInteractive` 를 확인하여
   - **콘솔 실행 시**: `Service.RunTestMode()` 로 1회 테스트 발송
   - **서비스 실행 시**: `ServiceBase.Run(service)` 로 윈도우 서비스 등록 실행
2. `Service.OnStart` 에서 **3 초 주기** `System.Timers.Timer` 시작
3. 매 틱마다 `EmailService.SendEmails()` 호출 →
   - `P_GETPENDINGEMAILS` 로 대기 메일 조회
   - 각 메일별 `P_GETEMAILCC` 로 참조자 목록 조회
   - `SmtpClient.Send()` 로 발송
   - 결과를 `P_LOG_EMAIL_STATUS` 로 기록 (큐 삭제 + 로그 INSERT)
4. 모든 단계의 INFO / ERROR 가 `LogBasePath\YYYYMM\YYYYMMDD.txt` 로 기록

---

## 🗃️ 데이터베이스 스키마

`table 생성정보.sql` 참조. 3 개의 테이블로 구성됩니다.

### `email_queue` — 메일 발송 대기 큐
발송 대상 메일의 본문, 수신자, SMTP 정보, 예약 일시 등을 보관합니다.
주요 컬럼: `EmailTranId(PK)`, `ToEmail`, `Subject`, `Body`, `IsHtml`,
`SMTPHost/Port/EnableSsl/UserName/UserPassword`, `Priority`,
`ScheduleDate`, `SendDate`, `IsSend`, `IsDelete`, `bs_cd`, `fac_cd` 등.

### `email_log` — 발송 결과 로그
`Status ENUM('SENT', 'FAILED')` 와 `ErrorMessage` 를 통해 결과를 추적합니다.
`email_queue.EmailTranId` 를 FK 로 참조 (`ON DELETE CASCADE`).

### `email_cc` — 참조 수신자(CC) 목록
한 건의 메일에 대한 N 개의 CC 주소를 보관합니다.

### 사용하는 Stored Procedure

| SP 명 | 용도 |
| --- | --- |
| `P_GETPENDINGEMAILS` | 발송 대기 중인 메일 조회 |
| `P_GETEMAILCC` | 특정 메일의 CC 목록 조회 |
| `P_LOG_EMAIL_STATUS` | 발송 결과 INSERT 및 큐 정리 |

> ⚠️ 위 SP 본문은 본 저장소에 포함되어 있지 않으며, DB 측에서 별도로
> 작성해 두어야 합니다.

---

## ⚙️ 환경 설정 (`App.config`)

```xml
<appSettings>
  <!-- 사용할 DB 타입: MariaDB | MSSQL -->
  <add key="DatabaseType"            value="MariaDB" />

  <!-- 연결 문자열 (DB 타입별) -->
  <add key="MariaDB_ConnectionString" value="Server=localhost;Database=mailsenderh_test;User Id=root;Password=...;SslMode=None;" />
  <add key="MSSQL_ConnectionString"   value="Server=...;Database=...;User Id=...;Password=...;" />

  <!-- ADO.NET Provider -->
  <add key="MariaDB_Provider" value="MySql.Data.MySqlClient" />
  <add key="MSSQL_Provider"   value="System.Data.SqlClient" />

  <!-- 파일 로그 기본 경로 -->
  <add key="LogBasePath" value="C:\MailSenderH\logs" />
</appSettings>
```

- `DatabaseType` 값을 바꾸기만 하면 MariaDB ↔ MSSQL 전환이 가능합니다.
- 로그는 `C:\MailSenderH\logs\YYYYMM\YYYYMMDD.txt` 형식으로 일자별 분리됩니다.

---

## 🚀 빌드 & 실행

### 1) 빌드
- Visual Studio 2022 에서 `MailSenderH.sln` 열기
- 구성: `Release | Any CPU` → 빌드

### 2) 콘솔(테스트) 실행
```cmd
MailSenderH.exe
```
- `Environment.UserInteractive == true` 로 인식되어
  **테스트 모드** 로 동작하며, 1회 메일 발송 후 Enter 대기 상태로 진입합니다.

### 3) Windows 서비스 등록
관리자 권한으로 다음을 실행합니다.

```cmd
install_service.bat
```
내부적으로 다음과 같이 등록됩니다.
```
sc create MailSenderHSvc
   binPath= "C:\Program Files (x86)\MailSenderH\MailSenderHSetUp\MailSenderH.exe"
   DisplayName= "MailSenderH Service"
   start= auto
   obj= LocalSystem
sc start MailSenderHSvc
```

### 4) 서비스 제거
```cmd
uninstall_service.bat
```
```
sc stop  MailSenderHSvc
sc delete MailSenderHSvc
```

### 5) 설치 패키지(MSI)
`MailSenderHSetUp` (Visual Studio Installer Project) 를 빌드하면 MSI 가
생성되며, 위 경로(`C:\Program Files (x86)\MailSenderH\...`) 에 배포됩니다.

---

## 📝 로깅

- 경로: `App.config` 의 `LogBasePath`
- 디렉터리 구조: `{LogBasePath}\{yyyyMM}\{yyyyMMdd}.txt`
- 포맷: `[yyyy-MM-dd HH:mm:ss] [INFO|ERROR] message`
- 서비스 생성/시작/중지, 타이머 이벤트, SMTP 전송 성공/실패, 예외 메시지가
  모두 기록됩니다.

---

## ✅ 주요 특징 요약

- **DB 큐 폴링 방식** 의 단순하고 직관적인 메일 발송 파이프라인
- **Stored Procedure** 사용으로 DB 측 정책(필터링/우선순위/배치) 변경이 용이
- `DbProviderFactory` 기반으로 **MariaDB / MSSQL 동시 지원**
- **CC 다중 수신자**, **HTML / Plain Text 본문**, **SSL SMTP** 지원
- **콘솔 테스트 모드** 와 **Windows Service 모드** 를 한 실행 파일에서 분기
- **일자별 파일 로깅** + DB 로그(`email_log`) 의 이중 추적

---

## 📌 참고

- 솔루션에 포함된 `MailSenderH 프로젝트 개요.docx` 와
  `table 생성정보.sql` 에 추가 설계 정보가 포함되어 있습니다.
- Service 이름: **`MailSenderHSvc`** / DisplayName: **`MailSenderH Service`**
