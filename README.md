Converter
=========

Convert one object to another with conversion rules in file (using System.Linq.Expressions)


﻿/*
* Create a program to convert SrcUser object to DestUser object.
* The source and destination objects have very different structure, for example SrcUser uses a collection
* to store emails and phones, but DestUser uses properties to store these data.
* Also both objecs have AdditionalProperties collection to store "dinamic" properties. But name of these properties
* can be different.
* Requirements:
* - The data conversion rules should be specified in a configuration file, for example XML, JSON or YAML.
* You can use any of these format.
* - It should be possible to use the same data conversion rules to convert data in reverce way. In this example
* from DestUser object to SrcUser object.
* Example of data conversion rules:
* SrcUser.First Name <-> DestUser.First Name
* SrcUser.Last Name <-> DestUser.Last Name
* SrcUser.Primary Email <-> DestUser.Email1
* SrcUser.First Not Primary Email <-> DestUser.Email2
* SrcUser.Second Not Primary Email <-> DestUser.Email2
* SrcUser.Home Phone <-> DestUser.Home Phone
* SrcUser.Business Phone <-> DestUser.Business Phone
* SrcUser.Mobile Phone <-> DestUser.AdditionalProperties.Mobile Phone
* SrcUser.AdditionalProperties.Company Name <-> DestUser.AdditionalProperties.Account Name
*/
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//namespace ConsoleApplication1
//{
// class Program
// {
// static void Main(string[] args)
// {
// }
// }
// class SrcUser
// {
// /// <summary>
// /// The first name of the user
// /// </summary>
// public string FirstName { get; set; }
// /// <summary>
// /// The last name of the user
// /// </summary>
// public string LastName { get; set; }
// /// <summary>
// /// The list of user's email addresses
// /// </summary>
// public List<SrcUserEmail> Emails { get; set; }
// /// <summary>
// /// The list of user's phone numbers
// /// </summary>
// public List<SrcUserPhone> Phones { get; set; }
// /// <summary>
// /// The list of additional properties.
// /// Each user can have own set of additional properties. For examle some user can have "CompanyName" property,
// /// but another user can work himself and as result he has no such property.
// /// </summary>
// public Dictionary<string, object> AdditionalProperties { get; set; }
// }
// class SrcUserEmail
// {
// /// <summary>
// /// The email address
// /// </summary>
// public string EmailAddress { get; set; }
// /// <summary>
// /// Indicates whether this email address is primary one or not.
// /// Each user can have only one email address
// /// </summary>
// public bool IsPrimary { get; set; }
// }
// class SrcUserPhone
// {
// /// <summary>
// /// The phone number
// /// </summary>
// public string Number { get; set; }
// /// <summary>
// /// The phone type
// /// </summary>
// public SrcUserPhoneType Type { get; set; }
// }
// enum SrcUserPhoneType
// {
// Home,
// Business,
// Mobile
// }
// class DestUser
// {
// /// <summary>
// /// The first name of the user
// /// </summary>
// public string FirstName { get; set; }
// /// <summary>
// /// The last name of the user
// /// </summary>
// public string LastName { get; set; }
// /// <summary>
// /// The first email address of the user
// /// </summary>
// public string Email1 { get; set; }
// /// <summary>
// /// The second email address of the user
// /// </summary>
// public string Email2 { get; set; }
// /// <summary>
// /// The third email address of the user
// /// </summary>
// public string Email3 { get; set; }
// /// <summary>
// /// The home phone number of the user
// /// </summary>
// public string HomePhone { get; set; }
// /// <summary>
// /// The business phone number of the user
// /// </summary>
// public string BusinessPhone { get; set; }
// /// <summary>
// /// The list of additional properties.
// /// Each user can have own set of additional properties. For examle some user can have "AccountName" property,
// /// but another user can work himself and as result he has no such property.
// /// </summary>
// public Dictionary<string, object> AdditionalProperties { get; set; }
// }
//}
