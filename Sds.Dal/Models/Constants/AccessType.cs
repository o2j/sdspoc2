using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sds.Dal.Models.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccessType
    {
        [EnumMember(Value = "view")]
        view,

        [EnumMember(Value = "print")]
        print
    }
}
