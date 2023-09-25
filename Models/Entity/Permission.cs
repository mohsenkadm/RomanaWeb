using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanaWeb.Models.Entity
{
    public class Permission
    {
        /// <summary>
        /// permission id
        /// </summary>
        public int PermissionId { get; set; }
        /// <summary>
        /// permission name id
        /// </summary>
        public int PermissionNameId { get; set; }
        /// <summary>
        /// user id
        /// </summary>
        public int AdminId { get; set; }
        /// <summary>
        /// state
        /// </summary>
        public bool State { get; set; }
        /// <summary>
        /// permission name for join table permission name
        /// </summary>
        public string PermissionName { get; set; }
        /// <summary>
        /// Control UserName 
        /// </summary>
        public string ControlName { get; set; }
        /// <summary>
        /// User name fro join table user
        /// </summary>
        public string UserName
        {
            get; set;
        }
    }
}
