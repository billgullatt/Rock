//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the T4\Model.tt template.
//
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Rock.Models;

namespace Rock.Models.Core
{
    [Table( "coreServiceLog" )]
    public partial class ServiceLog : ModelWithAttributes<ServiceLog>
    {
		[DataMember]
		public DateTime? Time { get; set; }
		
		[DataMember]
		public string Input { get; set; }
		
		[MaxLength( 50 )]
		[DataMember]
		public string Type { get; set; }
		
		[MaxLength( 50 )]
		[DataMember]
		public string Name { get; set; }
		
		[MaxLength( 50 )]
		[DataMember]
		public string Result { get; set; }
		
		[DataMember]
		public bool Success { get; set; }
		
		[NotMapped]
		public override string AuthEntity { get { return "Core.ServiceLog"; } }

        public static ServiceLog Read(int id)
        {
            return new Rock.Services.Core.ServiceLogService().Get( id );
        }

    }

    public partial class ServiceLogConfiguration : EntityTypeConfiguration<ServiceLog>
    {
        public ServiceLogConfiguration()
        {
		}
    }
}
