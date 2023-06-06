using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDR.Register.Domain.ValueObjects
{
    public class BusinessRuleError
    {
        public string Title { get; set; }
        public string Code { get; set; }

        public string Detail { get; set; }

        public BusinessRuleError(string errorTitle, string errorCode, string errorDetail)
        {
            Title = errorTitle;
            Code = errorCode;
            Detail=errorDetail;
        }
    }
}
