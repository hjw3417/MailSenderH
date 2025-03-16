@echo off
:: 관리자 권한 확인
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [INFO] 관리자 권한이 필요합니다. 다시 실행합니다...
    powershell Start-Process '%0' -Verb RunAs
    exit /b
)

echo [INFO] MailSenderH 서비스 중지 중...

sc stop MailSenderHSvc
if %errorlevel% neq 0 (
    echo [WARNING] 서비스가 실행 중이지 않거나 중지 실패!
)

echo [INFO] 서비스 삭제 중...
sc delete MailSenderHSvc
if %errorlevel% neq 0 (
    echo [ERROR] 서비스 삭제 실패!
    exit /b 1
)

echo [SUCCESS] MailSenderH 서비스가 성공적으로 삭제되었습니다.
exit /b 0
