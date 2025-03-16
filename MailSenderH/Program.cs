using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using MailSenderH.DataAccess;
using MailSenderH.Entities;
using MailSenderH.Logging;
using MailSenderH.Services;

namespace MailSenderH
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        static void Main(string[] args)
        {
            FileLogger.LogInfo("MailSenderH 프로그램 실행 시작");

            if (Environment.UserInteractive)
            {
                Console.WriteLine("서비스 실행 테스트 시작...");
                FileLogger.LogInfo("서비스 실행 테스트 시작");

                Service service = new Service();
                service.RunTestMode(); //수정: 테스트 실행 모드 호출

                //Console.WriteLine("서비스 실행 테스트 시작...");
                //FileLogger.LogInfo("서비스 실행 테스트 시작");

                //// 서비스 객체 생성
                //Service service = new Service();

                //// 강제로 OnStart 호출하여 서비스 시작 이벤트를 처리
                //service.OnStart(args);  // OnStart 호출

                //FileLogger.LogInfo("Windows 서비스 실행됨");

                //Console.WriteLine("서비스가 실행 중입니다. 종료하려면 Enter 키를 누르세요...");
                //Console.ReadLine();

                //// 강제로 OnStop 호출하여 서비스 중지 이벤트를 처리
                //service.OnStop();  // OnStop 호출

                //Console.WriteLine("서비스가 중지되었습니다.");
                //FileLogger.LogInfo("Windows 서비스 종료됨");
            }
            else
            {
                try
                {
                    FileLogger.LogInfo("Windows 서비스 모드로 실행됨");

                    Service service = new Service();
                    ServiceBase.Run(service);

                    FileLogger.LogInfo("ServiceBase.Run() 실행 완료");
                }
                catch (Exception ex)
                {
                    FileLogger.LogError($"Service 실행오류: {ex.Message}");
                }


                //Service service = new Service();
                //service.OnStart(args);  // 직접 호출하여 실행
                //Console.WriteLine("서비스가 시작되었습니다. 종료하려면 Enter 키를 누르세요...");
                //Console.ReadLine();
                //service.OnStop();
                //Console.WriteLine("서비스가 중지되었습니다.");

            }

            FileLogger.LogInfo("MailSenderH 프로그램 종료");
        }
    }
}
