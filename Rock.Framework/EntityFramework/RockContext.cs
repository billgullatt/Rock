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
using System.Data;
using System.Data.Objects;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Rock.EntityFramework
{
    public partial class RockContext : DbContext
    {
        public DbSet<Rock.Models.Cms.Auth> Auths { get; set; }
        public DbSet<Rock.Models.Cms.Block> Blocks { get; set; }
        public DbSet<Rock.Models.Cms.BlockInstance> BlockInstances { get; set; }
        public DbSet<Rock.Models.Cms.Blog> Blogs { get; set; }
        public DbSet<Rock.Models.Cms.BlogCategory> BlogCategories { get; set; }
        public DbSet<Rock.Models.Cms.BlogPost> BlogPosts { get; set; }
        public DbSet<Rock.Models.Cms.BlogPostComment> BlogPostComments { get; set; }
        public DbSet<Rock.Models.Cms.BlogTag> BlogTags { get; set; }
        public DbSet<Rock.Models.Cms.File> Files { get; set; }
        public DbSet<Rock.Models.Cms.HtmlContent> HtmlContents { get; set; }
        public DbSet<Rock.Models.Cms.Page> Pages { get; set; }
        public DbSet<Rock.Models.Cms.PageRoute> PageRoutes { get; set; }
        public DbSet<Rock.Models.Cms.Site> Sites { get; set; }
        public DbSet<Rock.Models.Cms.SiteDomain> SiteDomains { get; set; }
        public DbSet<Rock.Models.Cms.User> Users { get; set; }
        public DbSet<Rock.Models.Core.Attribute> Attributes { get; set; }
        public DbSet<Rock.Models.Core.AttributeQualifier> AttributeQualifiers { get; set; }
        public DbSet<Rock.Models.Core.AttributeValue> AttributeValues { get; set; }
        public DbSet<Rock.Models.Core.DefinedType> DefinedTypes { get; set; }
        public DbSet<Rock.Models.Core.DefinedValue> DefinedValues { get; set; }
        public DbSet<Rock.Models.Core.EntityChange> EntityChanges { get; set; }
        public DbSet<Rock.Models.Core.FieldType> FieldTypes { get; set; }
        public DbSet<Rock.Models.Core.ServiceLog> ServiceLogs { get; set; }
        public DbSet<Rock.Models.Crm.Address> Addresses { get; set; }
        public DbSet<Rock.Models.Crm.Person> People { get; set; }
        public DbSet<Rock.Models.Crm.PhoneNumber> PhoneNumbers { get; set; }
        public DbSet<Rock.Models.Groups.Group> Groups { get; set; }
        public DbSet<Rock.Models.Groups.GroupRole> GroupRoles { get; set; }
        public DbSet<Rock.Models.Groups.GroupType> GroupTypes { get; set; }
        public DbSet<Rock.Models.Groups.Member> Members { get; set; }
        public DbSet<Rock.Models.Util.Job> Jobs { get; set; }

        protected override void OnModelCreating( DbModelBuilder modelBuilder )
        {
            modelBuilder.Configurations.Add( new Rock.Models.Cms.AuthConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.BlockConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.BlockInstanceConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.BlogConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.BlogCategoryConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.BlogPostConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.BlogPostCommentConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.BlogTagConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.FileConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.HtmlContentConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.PageConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.PageRouteConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.SiteConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.SiteDomainConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Cms.UserConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Core.AttributeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Core.AttributeQualifierConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Core.AttributeValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Core.DefinedTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Core.DefinedValueConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Core.EntityChangeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Core.FieldTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Core.ServiceLogConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Crm.AddressConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Crm.PersonConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Crm.PhoneNumberConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Groups.GroupConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Groups.GroupRoleConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Groups.GroupTypeConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Groups.MemberConfiguration() );
            modelBuilder.Configurations.Add( new Rock.Models.Util.JobConfiguration() );
		}
    }
}

