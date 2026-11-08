using Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{

    public class MErrorInformation
    {

        [DbColumnAttribute("ErrorID")]
        public int? ErrorID { get; set; }


        [DbColumnAttribute("Message")]
        public string? Message { get; }


        [DbColumnAttribute("StackTrace")]
        public string? StackTrace { get; }

        [DbColumnAttribute("MethodName")]
        public string? MethodName { get; }

        [DbColumnAttribute("ClassName")]
        public string? ClassName { get; }

        [DbColumnAttribute("NamespaceName")]
        public string? NamespaceName { get; }

        [DbColumnAttribute("UserID")]
        public int? UserID { get; set; }




        public MErrorInformation(Exception ex)
        {
            Message = ex.Message;
            StackTrace = ex.StackTrace;
            MethodName = ex.TargetSite?.Name;
            ClassName = ex.TargetSite?.DeclaringType?.Name;
            NamespaceName = ex.TargetSite?.DeclaringType?.Namespace;

        }


    }



}
