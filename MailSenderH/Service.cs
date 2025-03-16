using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MailSenderH.Logging;
using MailSenderH.Services;

namespace MailSenderH
{
    public partial class Service : ServiceBase
    {
        private Timer timer;
        private EmailService emailService;

        public Service()
        {
            try
            {
                FileLogger.LogInfo("Service 생성자 호출됨.");
                ServiceName = "MailSenderHSvc";
                CanHandlePowerEvent = true;
                CanHandleSessionChangeEvent = true;
                CanPauseAndContinue = true;
                CanShutdown = true;
                emailService = new EmailService();
                FileLogger.LogInfo("EmailService 생성 완료.");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"Service 생성 중 오류 발생: {ex.Message}");
            }
        }


        protected override void OnStart(string[] args)
        {
            try
            {
                FileLogger.LogInfo("Onstart 진입");
                Console.WriteLine("Onstart 진입");

                // 일정한 주기로 이메일 전송 실행
                timer = new Timer(3000);
                timer.Elapsed += OnTimerElapsed;
                timer.Start();
                //OnTimerElapsed(); // 직접 호출된 메소드 실행

                // 로깅 추가 (타이머 시작 확인)
                FileLogger.LogInfo("타이머가 시작되었습니다.");
                Console.WriteLine("타이머가 시작되었습니다.");

            }
            catch (Exception ex)
            {
                FileLogger.LogError($"서비스 시작 중 오류 발생: {ex.Message}");
                Console.WriteLine($"서비스 시작 중 오류 발생: {ex.Message}");

            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                FileLogger.LogInfo("타이머 이벤트 발생.");
                Console.WriteLine("타이머 이벤트 발생.");

                emailService.SendEmails();
                FileLogger.LogInfo("이메일 전송 성공.");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"이메일 전송 중 오류 발생: {ex.Message}");
                Console.WriteLine($"이메일 전송 중 오류 발생: {ex.Message}");

            }
        }

        // 매개변수 없이 실행할 메소드
        private void OnTimerElapsed()
        {
            try
            {
                // 로깅 추가 (직접 호출 시 실행 확인)
                FileLogger.LogInfo("직접 호출된 타이머 이벤트가 실행되었습니다.");

                emailService.SendEmails();
            }
            catch (Exception ex)
            {
                // 오류 발생 시 로그 추가
                FileLogger.LogError($"이메일 전송 중 오류 발생: {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            try
            {
                timer?.Stop();
                // 로깅 추가 (서비스 중지 확인)
                FileLogger.LogInfo("서비스가 중지되었습니다.");
                Console.WriteLine("서비스가 중지되었습니다.");

            }
            catch (Exception ex)
            {
                // 오류 발생 시 로그 추가
                FileLogger.LogError($"서비스 중지 중 오류 발생: {ex.Message}");
                Console.WriteLine($"서비스 중지 중 오류 발생: {ex.Message}");

            }
        }

        public void RunTestMode()
        {
            try
            {
                // 테스트 모드 실행 시 로깅
                Console.WriteLine("서비스 테스트 모드 실행...");
                FileLogger.LogInfo("서비스 테스트 모드 실행...");

                OnTimerElapsed(); // 직접 호출된 메소드 실행

                Console.WriteLine("테스트 모드: 서비스가 실행 중입니다. 종료하려면 Enter 키를 누르세요...");
                Console.ReadLine();
                Console.WriteLine("테스트 모드: 서비스가 중지되었습니다.");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"서비스 테스트 중 오류 발생: {ex.Message}");
            }
        }
    }
}
