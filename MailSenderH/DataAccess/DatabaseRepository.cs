using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MailSenderH.Entities;
using System.Data; // EmailData 엔티티 가져오기
using System.Data.Common;
using System.Collections;

namespace MailSenderH.DataAccess
{
    public class DatabaseRepository
    {




        private static readonly string DbType = ConfigurationManager.AppSettings["DatabaseType"];
        private static readonly string ConnectionString = ConfigurationManager.AppSettings[$"{DbType}_ConnectionString"];
        private static readonly string ProviderName = ConfigurationManager.AppSettings[$"{DbType}_Provider"];
        //private static readonly string ConnectionString = ConfigurationManager.AppSettings["MariaDB_ConnectionString"];
        //private static readonly string ProviderName = ConfigurationManager.AppSettings["MariaDB_Provider"];



        /// <summary>
        /// 전송 대기 중인 이메일 목록을 가져오는 메서드 (Stored Procedure 사용)
        /// </summary>
        /// <returns>전송 대기 중인 이메일 목록</returns>
        public static List<EmailData> GetPendingEmails()
        {
            Console.WriteLine($" DatabaseType: {DbType}");
            Console.WriteLine($" ProviderName: {ProviderName}");
            Console.WriteLine($" ConnectionString: {ConnectionString}");
            List<EmailData> emailList = new List<EmailData>();

            //db provider factory를 사용하여 동적으로 db 연결
            //try
            //{
            //    DataTable dt = DbProviderFactories.GetFactoryClasses();
            //    Console.WriteLine("==== 현재 등록된 DbProviderFactories 목록 ====");
            //    foreach (DataRow row in dt.Rows)
            //    {
            //        Console.WriteLine($"Provider: {row["InvariantName"]} - {row["Name"]}");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"DbProviderFactories 조회 중 오류 발생: {ex.Message}");
            //}

            DbProviderFactory factory = null;
            try
            {
                factory = DbProviderFactories.GetFactory(ProviderName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"예외 발생: {ex.Message}");
                Console.WriteLine($"예외 스택 트레이스: {ex.StackTrace}");
            }

            //DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);
            Console.WriteLine("123");

            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "P_GETPENDINGEMAILS";
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emailList.Add(new EmailData
                            {
                                EmailTranId = reader.GetInt32(0),
                                ToEmail = reader.GetString(1),
                                Subject = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Body = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                IsHtml = reader.GetBoolean(4),
                                SMTPHost = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                SMTPPort = reader.IsDBNull(6) ? 25 : reader.GetInt32(6),
                                SMTPEnableSsl = reader.GetBoolean(7),
                                SMTPUserName = reader.IsDBNull(8) ? "" : reader.GetString(8),
                                SMTPUserPassword = reader.IsDBNull(9) ? "" : reader.GetString(9)
                            });
                        }
                    }
                }
            }

            return emailList;
        }

        public static List<string> GetEmailCC(int emailTranId)
        {
            List<string> ccList = new List<string>();
            DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);

            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "P_GETEMAILCC";
                    cmd.CommandType = CommandType.StoredProcedure;

                    DbParameter param = cmd.CreateParameter();
                    param.ParameterName = "@p_EmailTranId";
                    param.Value = emailTranId;
                    cmd.Parameters.Add(param);

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ccList.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return ccList;
        }

        public static void LogEmailAndDeleteQueue(int emailTranId, string recipient, string subject, string body, string status, string errorMessage)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);

            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "P_LOG_EMAIL_STATUS";
                    cmd.CommandType = CommandType.StoredProcedure;

                    DbParameter param1 = cmd.CreateParameter();
                    param1.ParameterName = "@p_EmailTranId";
                    param1.Value = emailTranId;
                    cmd.Parameters.Add(param1);

                    DbParameter param2 = cmd.CreateParameter();
                    param2.ParameterName = "@p_Recipient";
                    param2.Value = recipient;
                    cmd.Parameters.Add(param2);

                    DbParameter param3 = cmd.CreateParameter();
                    param3.ParameterName = "@p_Subject";
                    param3.Value = subject;
                    cmd.Parameters.Add(param3);

                    DbParameter param4 = cmd.CreateParameter();
                    param4.ParameterName = "@p_Body";
                    param4.Value = body;
                    cmd.Parameters.Add(param4);

                    DbParameter param5 = cmd.CreateParameter();
                    param5.ParameterName = "@p_Status";
                    param5.Value = status;
                    cmd.Parameters.Add(param5);

                    DbParameter param6 = cmd.CreateParameter();
                    param6.ParameterName = "@p_ErrorMessage";
                    param6.Value = string.IsNullOrEmpty(errorMessage) ? DBNull.Value : (object)errorMessage;
                    cmd.Parameters.Add(param6);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
