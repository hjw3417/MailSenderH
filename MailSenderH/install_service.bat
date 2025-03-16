@echo off
:: 관리자 권한 확인
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [INFO] 관리자 권한이 필요합니다. 다시 실행합니다...
    powershell Start-Process '%0' -Verb RunAs
    exit /b
)

echo [INFO] MailSenderH 서비스 등록 중...

:: LocalSystem 권한으로 서비스 등록
sc create MailSenderHSvc binPath= "C:\Program Files (x86)\MailSenderH\MailSenderHSetUp\MailSenderH.exe" DisplayName= "MailSenderH Service" start= auto obj= LocalSystem
if %errorlevel% neq 0 (
    echo [ERROR] 서비스 등록 실패!
    exit /b 1
)

:: 서비스 시작 전 3초 대기 (파일 복사 완료 대기)
timeout /t 3 /nobreak >nul

echo [INFO] 서비스 시작 중...
sc start MailSenderHSvc
if %errorlevel% neq 0 (
    echo [ERROR] 서비스 시작 실패! 로그를 확인하세요.
    exit /b 1
)

echo [SUCCESS] MailSenderH 서비스가 성공적으로 등록 및 시작되었습니다.
exit /b 0
