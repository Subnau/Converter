using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConverterLib
{
    public class SrcUser
    {
        /// <summary>
        /// The first name of the user
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user
        /// </summary>
        public string LastName { get; set; }


        /// <summary>
        /// The list of user's email addresses
        /// </summary>
        public List<SrcUserEmail> Emails { get; set; }

        /// <summary>
        /// The list of user's phone numbers
        /// </summary>

        public List<SrcUserPhone> Phones { get; set; }

        /// <summary>
        /// The list of additional properties.
        /// Each user can have own set of additional properties. For examle some user can have "CompanyName" property,
        /// but another user can work himself and as result he has no such property.

        /// </summary>
        public Dictionary<string, object> AdditionalProperties { get; set; }
    }



    public class SrcUserEmail
    {

        /// <summary>
        /// The email address
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Indicates whether this email address is primary one or not.
        /// Each user can have only one email address
        /// </summary>
        public bool IsPrimary { get; set; }
    }

    public class SrcUserPhone
    {
        /// <summary>
        /// The phone number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The phone type
        /// </summary>
        public SrcUserPhoneType Type { get; set; }
    }

    public enum SrcUserPhoneType
    {
        Home,
        Business,
        Mobile
    }

    public class DestUser
    {

        /// <summary>
        /// The first name of the user
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The first email address of the user
        /// </summary>
        public string Email1 { get; set; }

        /// <summary>
        /// The second email address of the user
        /// </summary>
        public string Email2 { get; set; }

        /// <summary>
        /// The third email address of the user
        /// </summary>
        public string Email3 { get; set; }

        /// <summary>
        /// The home phone number of the user
        /// </summary>
        public string HomePhone { get; set; }

        /// <summary>
        /// The business phone number of the user
        /// </summary>
        public string BusinessPhone { get; set; }

        /// <summary>
        /// The list of additional properties.
        /// Each user can have own set of additional properties. For examle some user can have "AccountName" property,
        /// but another user can work himself and as result he has no such property.
        /// </summary>
        public Dictionary<string, object> AdditionalProperties { get; set; }
    }
}
