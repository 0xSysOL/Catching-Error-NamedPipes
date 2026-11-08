using Microsoft.Data.SqlClient;
using Models;
using Setting___HR_Client___WS_Server;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace DataLayer
{
    public class clsLogErrorToDatabase
    {

        public static bool FillParamerters(ref SqlCommand command, MErrorInformation mError)
        {


            command.Parameters.Add("@Message", SqlDbType.NVarChar, 2000).Value = mError.Message;
            command.Parameters.Add("@StackTrace", SqlDbType.NVarChar, -1).Value = mError.StackTrace ?? (object)DBNull.Value;
            command.Parameters.Add("@MethodName", SqlDbType.NVarChar, 256).Value = mError.MethodName ?? (object)DBNull.Value;
            command.Parameters.Add("@ClassName", SqlDbType.NVarChar, 256).Value = mError.ClassName ?? (object)DBNull.Value;
            command.Parameters.Add("@Namespace", SqlDbType.NVarChar, 256).Value = mError.Namespace ?? (object)DBNull.Value;
            command.Parameters.Add("@OccuredAt", SqlDbType.DateTime).Value = mError.OccuredAt;
            command.Parameters.Add("@UserID", SqlDbType.Int, -1).Value = mError.UserID ?? (object)DBNull.Value;

            return true;
        }

        public static bool Log(MErrorInformation mError)
        {


            if (mError == null)
                throw new Exception("The MErrorInformation Class Is NULL");


            string? ConnectionString  = clsUtilities.GetConnectionString();
            if (ConnectionString == null)
                return false;



            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {


                string Query = "insert into ErrorLog(Message,StackTrace,MethodName,ClassName,Namespace,OccuredAt,UserID)" +
                    "values (@Message,@StackTrace,@MethodName,@ClassName,@Namespace,@OccuredAt,@UserID)";
                SqlCommand command = new SqlCommand(Query, connection);

                FillParamerters(ref command,mError);

                connection.Open();

                command.ExecuteNonQuery();




            }

            return true;

        }
        public static bool Log(MErrorInformation mError, SqlConnection connection)
        {


            if (mError == null)
                throw new Exception("The MErrorInformation Class Is NULL");


            string? ConnectionString = clsUtilities.GetConnectionString();
            if (ConnectionString == null)
                return false;



            using (connection = new SqlConnection(ConnectionString))
            {


                string Query = "insert into ErrorLog(Message,StackTrace,MethodName,ClassName,Namespace,OccuredAt,UserID)" +
                    "values (@Message,@StackTrace,@MethodName,@ClassName,@Namespace,@OccuredAt,@UserID)";
                SqlCommand command = new SqlCommand(Query, connection);

                FillParamerters(ref command, mError);

                connection.Open();

                command.ExecuteNonQuery();




            }

            return true;

        }

        public static bool LogGroup(Queue<MErrorInformation> QMErrors)
        {
            SqlConnection connection = new SqlConnection(clsUtilities.GetConnectionString());
            try
            {
                connection.Open();
                while (QMErrors.Count > 0)
                {

                    Log(QMErrors.Dequeue(), connection);
                }
            }
            finally
            {
                connection.Close();
            }


            return true;

        }

    }
}
