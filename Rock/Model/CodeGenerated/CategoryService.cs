//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Category Service class
    /// </summary>
    public partial class CategoryService : Service<Category>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public CategoryService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Category item, out string errorMessage )
        {
            errorMessage = string.Empty;
 
            if ( new Service<Category>( Context ).Queryable().Any( a => a.ParentCategoryId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Category.FriendlyTypeName, Category.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<DataView>( Context ).Queryable().Any( a => a.CategoryId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Category.FriendlyTypeName, DataView.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<History>( Context ).Queryable().Any( a => a.CategoryId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Category.FriendlyTypeName, History.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<PrayerRequest>( Context ).Queryable().Any( a => a.CategoryId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Category.FriendlyTypeName, PrayerRequest.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<Report>( Context ).Queryable().Any( a => a.CategoryId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Category.FriendlyTypeName, Report.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<Schedule>( Context ).Queryable().Any( a => a.CategoryId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Category.FriendlyTypeName, Schedule.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<WorkflowType>( Context ).Queryable().Any( a => a.CategoryId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Category.FriendlyTypeName, WorkflowType.FriendlyTypeName );
                return false;
            }  
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class CategoryExtensionMethods
    {
        /// <summary>
        /// Clones this Category object to a new Category object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static Category Clone( this Category source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as Category;
            }
            else
            {
                var target = new Category();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another Category object to this Category object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this Category target, Category source )
        {
            target.IsSystem = source.IsSystem;
            target.ParentCategoryId = source.ParentCategoryId;
            target.EntityTypeId = source.EntityTypeId;
            target.EntityTypeQualifierColumn = source.EntityTypeQualifierColumn;
            target.EntityTypeQualifierValue = source.EntityTypeQualifierValue;
            target.Order = source.Order;
            target.Name = source.Name;
            target.Description = source.Description;
            target.IconCssClass = source.IconCssClass;
            target.HighlightColor = source.HighlightColor;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Id = source.Id;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
