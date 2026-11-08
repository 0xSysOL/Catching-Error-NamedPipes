using Data_Layer.Configuration;
using Microsoft.Data.SqlClient;
using Models.Entities;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
namespace Data_Layer
{

    internal class Utilities_DB
    {

        #region Employee
        public static void AddValueToManagerID(SqlCommand command, int? ID)
        {


            command.Parameters.AddWithValue("@ManagerID", ID == null ? DBNull.Value : ID);

        }
        #endregion


        #region Pagination
        public static void AddPaginationToSqlCommand(SqlCommand command, int PageNumber, int RowPerPage)
        {
            command.Parameters.AddWithValue("@PageNumber", PageNumber);
            command.Parameters.AddWithValue("@RowPerPage", RowPerPage);

        }
        #endregion

        #region initialize Sql Command
        /// <summary>
        /// initialize SqlCommand
        /// </summary>
        /// <param name="TextQ"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static SqlCommand GetSQL_Command(string TextQ, SqlConnection connection, CommandType commandType)
        {
            SqlCommand command = new SqlCommand(TextQ, connection);
            command.CommandType = commandType;
            return command;
        }

        /// <summary>
        /// we Should Use This Method For Inserting That We Add 3-4 or more 
        /// parameters Ather wise this Method Nussery to use and
        /// will take resources for nothing
        /// </summary>
        /// <param name="command">SqlCommand</param>
        /// <param name="TableOb" >
        /// Table Object If You Want Know What Type Will send From this Method
        /// See Class namespace Models.Entities</param>
        private static void _AddParametersWithValue(SqlCommand command, object TableOb, PropertyInfo[] propertyInfo)
        {

            foreach (var prop in propertyInfo)
            {
                var MyAttr = prop.GetCustomAttribute<DbColumnAttribute>();
                if (MyAttr != null && prop.GetValue(TableOb) != null)
                    command.Parameters.AddWithValue(MyAttr.ColumnName, prop.GetValue(TableOb));


            }
        }
        public static void AddParametersWithValue<Class>(SqlCommand command, object TableOb)
        {

            if (TableOb == null || command == null)
                return;

            if (clsSetting.cache.TryGetValue(typeof(Class).Name, out var value))
                _AddParametersWithValue(command, TableOb, value);

            else
            {
                Type Reflection = TableOb.GetType();
                PropertyInfo[] propertyInfo = Reflection.GetProperties();
                clsSetting.cache.TryAdd(typeof(Class).Name, propertyInfo);
                _AddParametersWithValue(command, TableOb, propertyInfo);
            }




        }



        #endregion

        #region OLD 
        /// <summary>
        /// you can use This Method For
        /// Types Queries Like (SELECT * TableName).
        /// </summary>
        /// <param name="command"></param>
        /// <returns>DataTable</returns>
        //public static DataTable SelectRecords_Execute(SqlCommand command)
        //{


        //    DataTable data = new DataTable();
        //    try
        //    {

        //        using (SqlDataReader reader = command.ExecuteReader())
        //        {
        //            data.Load(reader);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);

        //    }

        //    return data;

        //}






        #endregion


        #region Execute Sql Command

        /// <summary>
        /// New Select Record We Will Use This Bec the past its use  List<Dictionary<string, object>> and we replace it to this method 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>

        public static List<Dictionary<string, object>> SelectRecords_Execute(SqlCommand command)
        {
            List<Dictionary<string, object>> ListDic = new List<Dictionary<string, object>>();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                try
                {

                    int FieldCount = reader.FieldCount;
                    while (reader.Read())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        ListDic.Add(row);
                    }




                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }

            return ListDic;

        }
        public static Dictionary<string, object> _SelectRecordsWithWhere_Execute<ClassType>(SqlCommand command)
        {
            Dictionary<string, object> Dic = new Dictionary<string, object>();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                try
                {

                    int FieldCount = reader.FieldCount;
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            Dic.Add(reader.GetName(i), reader.GetValue(i));

                    }




                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }

            return Dic;

        }

        public static int? Inserting_Execute(SqlCommand command)
        {
            int? ID = null;
            try
            {
                ID = Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ID;
        }


        public static object Scalar_Execute(SqlCommand command)
        {

            try
            {
                return command.ExecuteScalar();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

            return null;



        }
        public static object ExecuteNoneQu(SqlCommand command)
        {

            try
            {

                return command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;



        }

        #endregion




        private static string _AddParametersToString<Class>(object TableOb, PropertyInfo[] propInfo)
        {

            string ColumnsName = "";

            foreach (var Property in propInfo)
            {

                if (Property.GetValue(TableOb) != null)
                {
                    var MyAttr = Property.GetCustomAttribute<DbColumnAttribute>();
                    if (MyAttr != null)
                        ColumnsName += MyAttr.ColumnName + ",";
                }
            }

            if (ColumnsName.Length > 0)
                return ColumnsName.Remove(ColumnsName.Length - 1);
            else
                return "";


        }

        /// <summary>
        /// 
        /// this Function Take one Parameter,
        /// Structure.
        /// <Structure> See In Variable Came from outside,
        /// if Is Null He Will Not Add the Column to string Variable
        /// If Not He Will Add Like that 
        /// Example: "PersonID,FirstName", And Method Will Added to string
        /// why this Important Because To Send As String Text To Database as parameters
        /// And There will be Process in DB , and selected The Columns That Send it
        /// Example: when you Send Just "PersonID" the Server
        /// Just Return one Column > PersonID, not else.
        /// 
        /// </summary>
        /// <param name="Structure"></param>
        /// <returns></returns>
        public static string AddParametersToString<Class>(object Structure)
        {
            if (Structure == null)
                return null;


            if (clsSetting.cache.TryGetValue(typeof(Class).Name, out var value))
            {
                return _AddParametersToString<Class>(Structure, value);

            }
            else
            {
                Type Reflection = Structure.GetType();
                PropertyInfo[] propInfo = Reflection.GetProperties();
                return _AddParametersToString<Class>(Structure, propInfo);
            }




        }


        #region Use Reflection And Converting Object To Class From well-known Class

        /// <summary>
        /// this method behoifer work that work with static cache it is store properties
        /// we stored after first reflection it good to perfromnce,
        /// and we do not repeat to each call reflect the class everey time
        /// </summary>
        /// <typeparam name="T"> Type of class</typeparam>
        /// <param name="Rows">Data</param>
        /// <returns></returns>
        static List<T> _ConvertRowsToClassesOB<T>(List<Dictionary<string, object>> Rows)
        {

            List<T> Data = new List<T>();
            PropertyInfo[] properties = clsSetting.cache.TryGetValue(typeof(T).Name, out PropertyInfo[] cachedProps)
                ? cachedProps : Array.Empty<PropertyInfo>();

            try

            {
                for (int i = 0; i < Rows.Count; i++)
                {
                    T Class = Activator.CreateInstance<T>();

                    foreach (var item in properties)
                    {
                        Dictionary<string, object> pairs = Rows[i];
                        var ColumnAttribute = item.GetCustomAttribute<DbColumnAttribute>();

                        if (ColumnAttribute != null && pairs.TryGetValue(ColumnAttribute.ColumnName, out var value))
                            item.SetValue(Class, value != DBNull.Value ? value : null);
                        else
                            item.SetValue(Class, null);


                    }

                    Data.Add(Class);

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }


            return Data;
        }


        /// <summary>
        /// This Method work when you have rows you get it from database and you want to convert it to
        /// structure you create it like (MPeople or MEmployee).
        /// and this method take two parameters (Data > Rows,properties)
        /// the properties came from base method (_ConvertRowsToClassesOB)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Rows">Data That Came From Database</param>
        /// <param name="properties">Reflection for The Structure You Send it from <T> </param>
        /// <returns>Return All Data As List</returns>
        static List<T> _ConvertRowsToClassesOB<T>(List<Dictionary<string, object>> Rows, PropertyInfo[] properties)
        {

            List<T> Data = new List<T>();
            try
            {

                for (int i = 0; i < Rows.Count; i++)
                {
                    T Class = Activator.CreateInstance<T>();

                    foreach (var item in properties)
                    {
                        var ColumnAttribute = item.GetCustomAttribute<DbColumnAttribute>();

                        Dictionary<string, object> pairs = Rows[i];
                        if (ColumnAttribute != null && pairs.TryGetValue(ColumnAttribute.ColumnName, out var value))
                            item.SetValue(Class, value != DBNull.Value ? value : null);
                        else
                            item.SetValue(Class, null);

                    }
                    Data.Add(Class);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }


            return Data;
        }
        public static List<T> ConvertRowsToClassesOB<T>(List<Dictionary<string, object>> Rows)
        {
            if (Rows == null || Rows.Count == 0)
                return null;


            if (clsSetting.cache.TryGetValue(typeof(T).Name, out var values))
            {
                return _ConvertRowsToClassesOB<T>(Rows, values);

            }
            else
            {


                Type Reflection = typeof(T);
                PropertyInfo[] properties = Reflection.GetProperties();
                AddToCacheDictionary(typeof(T).Name, properties);

                return _ConvertRowsToClassesOB<T>(Rows, properties);

            }

        }

        public static Class? ConvertRowToClassesOB<Class>(List<Dictionary<string, object>> OB)
        {
            return GetFirstOBFromList(ConvertRowsToClassesOB<Class>(OB));
        }

        public static Class? GetFirstOBFromList<Class>(List<Class> OB) => OB.FirstOrDefault();

        #endregion End

        static void AddToCacheDictionary(string ClassName, PropertyInfo[] properties) => clsSetting.cache.GetOrAdd(ClassName, properties);



        #region My Helper Classes
        public class TaskProcessor<T>
        {
            private ConcurrentQueue<T> C_Queue;

            public int Count
            {
                get
                {
                    return C_Queue.Count;
                }
            }



            public TaskProcessor()
            {
                C_Queue = new ConcurrentQueue<T>();
            }

            public T? Peek()
            {
                if (C_Queue == null) return default;


                if (C_Queue.TryPeek(out T? result))
                    return result;


                return default;

            }
            public T? Dequeue()
            {
                if (C_Queue == null) return default;


                if (C_Queue.TryDequeue(out T? result))
                    return result;


                return default;

            }


            public async Task Enqueue(T Ob) => C_Queue.Enqueue(Ob);




        }


        #endregion END
    }
}
