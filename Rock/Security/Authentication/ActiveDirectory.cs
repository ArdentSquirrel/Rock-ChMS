﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.DirectoryServices.AccountManagement;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Security.Authentication
{
    /// <summary>
    /// Authenticates a username using Active Directory
    /// </summary>
    [Description( "Active Directory Authentication Provider" )]
    [Export(typeof(AuthenticationComponent))]
    [ExportMetadata("ComponentName", "Active Directory")]
    [TextField( "Server", "The Active Directory server name", true, "", "Server", 0 )]
    [TextField( "Domain", "The network domain that users belongs to", true, "", "Server", 1 )]
    public class ActiveDirectory : AuthenticationComponent
    {
        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public override bool RequiresRemoteAuthentication
        {
            get { return false; }
        }

        /// <summary>
        /// Authenticates the specified user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public override bool Authenticate( UserLogin user, string password )
        {
            string username = user.UserName;
            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "Domain" ) ) )
                username = string.Format( @"{0}\{1}", GetAttributeValue( "Domain" ), user.UserName );

            var context = new PrincipalContext( ContextType.Domain, GetAttributeValue( "Server" ) );
            using ( context )
            {
                return context.ValidateCredentials( user.UserName, password );
            }
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override string EncodePassword( UserLogin user, string password )
        {
            return null;
        }

        public override bool Authenticate( System.Web.HttpRequest request, out string userName, out string returnUrl )
        {
            throw new NotImplementedException();
        }

        public override Uri GenerateLoginUrl( System.Web.HttpRequest request )
        {
            throw new NotImplementedException();
        }

        public override bool IsReturningFromAuthentication( System.Web.HttpRequest request )
        {
            throw new NotImplementedException();
        }

        public override string ImageUrl()
        {
            throw new NotImplementedException();
        }
    }
}