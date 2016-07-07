﻿using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using Rock.Data;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Sql stores the result of provided SQL query into a variable.
    /// 
    /// {% sql results %}
    /// SELECT [FirstName], [LastName] FROM [Person]
    /// {% endsql %}
    /// </summary>
    public class Sql : DotLiquid.Block
    {
        private static readonly Regex Syntax = new Regex( @"(\w+)" );

        private string _to;

        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            Match syntaxMatch = Syntax.Match( markup );
            if ( syntaxMatch.Success )
                _to = syntaxMatch.Groups[1].Value;
            else
                throw new System.Exception( "Could not find the variable to place results in." );

            base.Initialize( tagName, markup, tokens );
        }

        public override void Render( Context context, TextWriter result )
        {
            using ( TextWriter sql = new StringWriter() )
            {
                base.Render( context, sql );

                var results = DbService.GetDataSet( sql.ToString(), CommandType.Text, null, null );

                var dropRows = new List<DataRowDrop>();
                if ( results.Tables.Count == 1 )
                {
                    foreach ( DataRow row in results.Tables[0].Rows )
                    {
                        dropRows.Add( new DataRowDrop( row ) );
                    }
                }

                context.Scopes.Last()[_to] = dropRows;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private class DataRowDrop : DotLiquid.Drop
        {
            private readonly DataRow _dataRow;

            public DataRowDrop( DataRow dataRow )
            {
                _dataRow = dataRow;
            }

            public override object BeforeMethod( string method )
            {
                if ( _dataRow.Table.Columns.Contains( method ) )
                {
                    return _dataRow[method];
                }

                return null;
            }
        }
    }
}