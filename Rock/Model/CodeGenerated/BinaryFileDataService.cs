//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// BinaryFileData Service class
    /// </summary>
    public partial class BinaryFileDataService : Service<BinaryFileData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileDataService"/> class
        /// </summary>
        public BinaryFileDataService()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileDataService"/> class
        /// </summary>
        /// <param name="repository">The repository.</param>
        public BinaryFileDataService(IRepository<BinaryFileData> repository) : base(repository)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileDataService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public BinaryFileDataService(RockContext context) : base(context)
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
        public bool CanDelete( BinaryFileData item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class BinaryFileDataExtensionMethods
    {
        /// <summary>
        /// Clones this BinaryFileData object to a new BinaryFileData object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static BinaryFileData Clone( this BinaryFileData source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as BinaryFileData;
            }
            else
            {
                var target = new BinaryFileData();
                target.Content = source.Content;
                target.Id = source.Id;
                target.Guid = source.Guid;

            
                return target;
            }
        }
    }
}
