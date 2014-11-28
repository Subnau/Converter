using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ConverterLib;
using SharedUserClasses;

namespace ConverterConsole
{
    class Program
    {
        /*

                   * Create a program to convert SrcUser object to DestUser object.

                   * The source and destination objects have very different structure, for example SrcUser uses a collection

                   * to store emails and phones, but DestUser uses properties to store these data.

                   * Also both objecs have AdditionalProperties collection to store "dinamic" properties. But name of these properties

                   * can be different.

                   * Requirements:

                   *  - The data conversion rules should be specified in a configuration file, for example XML, JSON or YAML.

                   *    You can use any of these format.

                   *  - It should be possible to use the same data conversion rules to convert data in reverce way. In this example

                   *    from DestUser object to SrcUser object.

                   * Example of data conversion rules:

                   *  SrcUser.First Name <-> DestUser.First Name

                   *  SrcUser.Last Name <-> DestUser.Last Name

                   *  SrcUser.Primary Email <-> DestUser.Email1

                   *  SrcUser.First Not Primary Email <-> DestUser.Email2

                   *  SrcUser.Second Not Primary Email <-> DestUser.Email2

                   *  SrcUser.Home Phone <-> DestUser.Home Phone

                   *  SrcUser.Business Phone <-> DestUser.Business Phone

                   *  SrcUser.Mobile Phone <-> DestUser.AdditionalProperties.Mobile Phone

                   *  SrcUser.AdditionalProperties.Company Name <-> DestUser.AdditionalProperties.Account Name

                   */


        //Create rules for testing
        // ReSharper disable once UnusedMember.Local
        static List<Rule> createRules()
        {
            var rules = new List<Rule>
            {
                new Rule
                {
                    Description = "FirstName to FirstName",
                    Source = new SimpleBinding {PropName = "FirstName"},
                    Dest = new SimpleBinding {PropName = "FirstName"}
                },
                new Rule
                {
                    Description = "LastName to LastName",
                    Source = new SimpleBinding {PropName = "LastName"},
                    Dest = new SimpleBinding {PropName = "LastName"}
                },
                new Rule
                {
                    Description = "Company name to account name",
                    Source = new DicBinding
                    {
                        PropName = "AdditionalProperties",
                        Key = "Company Name"
                    },
                    Dest = new DicBinding
                    {
                        PropName = "AdditionalProperties",
                        Key = "Account Name"
                    },
                },
                new Rule
                {
                    Description = "Home phone in list to HomePhone",
                    Source = new ListBinding
                    {
                        ListName = "Phones",
                        PropName = "Number",
                        ItemIndex = 0,
                        ListFilterOrInits = new List<ListFilterOrInit>
                        {
                            new ListFilterOrInit {PropName = "Type", Condition = "equal", PropValue = "Home"}
                        }
                    },
                    Dest = new SimpleBinding {PropName = "HomePhone"}
                },
                new Rule
                {
                    Description = "Business phone in list to BusinessPhone",
                    Source = new ListBinding
                    {
                        ListName = "Phones",
                        PropName = "Number",
                        ItemIndex = 0,
                        ListFilterOrInits = new List<ListFilterOrInit>
                        {
                            new ListFilterOrInit {PropName = "Type", Condition = "equal", PropValue = "Business"}
                        }
                    },
                    Dest = new SimpleBinding {PropName = "BusinessPhone"}
                },
                new Rule
                {
                    Description = "Mobile phone in list to AddtionalPropertiest Mobile phone",
                    Source = new ListBinding
                    {
                        ListName = "Phones",
                        PropName = "Number",
                        ItemIndex = 0,
                        ListFilterOrInits = new List<ListFilterOrInit>
                        {
                            new ListFilterOrInit {PropName = "Type", Condition = "equal", PropValue = "Mobile"}
                        }
                    },
                    Dest = new DicBinding
                    {
                        PropName = "AdditionalProperties",
                        Key = "Mobile phone"
                    }
                },
                new Rule
                {
                    Description = "Primary email in list to Email1",
                    Source = new ListBinding
                    {
                        ListName = "Emails",
                        PropName = "EmailAddress",
                        ItemIndex = 0,
                        ListFilterOrInits = new List<ListFilterOrInit>
                        {
                            new ListFilterOrInit {PropName = "IsPrimary", Condition = "equal", PropValue = "True"}
                        }
                    },
                    Dest = new SimpleBinding {PropName = "Email1"}
                },
                new Rule
                {
                    Description = "First not primary email in list to Email2",
                    Source = new ListBinding
                    {
                        ListName = "Emails",
                        PropName = "EmailAddress",
                        ItemIndex = 0,
                        ListFilterOrInits = new List<ListFilterOrInit>
                        {
                            new ListFilterOrInit {PropName = "IsPrimary", Condition = "equal", PropValue = "False"}
                        }
                    },
                    Dest = new SimpleBinding {PropName = "Email2"}
                },
                new Rule
                {
                    Description = "Second not primary email in list to Email3",
                    Source = new ListBinding
                    {
                        ListName = "Emails",
                        PropName = "EmailAddress",
                        ItemIndex = 1,
                        ListFilterOrInits = new List<ListFilterOrInit>
                        {
                            new ListFilterOrInit {PropName = "IsPrimary", Condition = "equal", PropValue = "False"}
                        }
                    },
                    Dest = new SimpleBinding {PropName = "Email3"}
                }
            };

            return rules;
        }

        // ReSharper disable once InconsistentNaming
        static void Main()
        {

            var s = File.ReadAllText("ConverterRules.xml");

            var srcUser = new SrcUser();
            srcUser.FirstName = "FirstName";
            srcUser.LastName = "LastName";
            srcUser.AdditionalProperties = new Dictionary<string, object>();
            srcUser.AdditionalProperties["Company Name"] = "Company1";
            srcUser.Phones = new List<SrcUserPhone>
            {
                new SrcUserPhone { Number = "Business-123", Type = SrcUserPhoneType.Business },
                new SrcUserPhone { Number = "Mobile-123", Type = SrcUserPhoneType.Mobile },
                new SrcUserPhone { Number = "Home-123", Type = SrcUserPhoneType.Home }
            };

            srcUser.Emails = new List<SrcUserEmail>
            {
                new SrcUserEmail {EmailAddress = "mail_primary",IsPrimary = true},
                new SrcUserEmail {EmailAddress = "mail_first_not_primary",IsPrimary = false},
                new SrcUserEmail {EmailAddress = "mail_second_not_primary",IsPrimary = false}
            };

            var c = new Converter(s);
            DestUser destUser = c.Convert(srcUser);
            SrcUser srcUserAfterBackConvert = c.Convert(destUser);

            Debug.Assert(srcUser.FirstName == srcUserAfterBackConvert.FirstName);
            Debug.Assert(srcUser.LastName == srcUserAfterBackConvert.LastName);
            Debug.Assert(srcUser.AdditionalProperties["Company Name"] == srcUserAfterBackConvert.AdditionalProperties["Company Name"]);
            Debug.Assert(srcUser.Phones.First(p => p.Type == SrcUserPhoneType.Business).Number == srcUserAfterBackConvert.Phones.First(p => p.Type == SrcUserPhoneType.Business).Number);
            Debug.Assert(srcUser.Phones.First(p => p.Type == SrcUserPhoneType.Mobile).Number == srcUserAfterBackConvert.Phones.First(p => p.Type == SrcUserPhoneType.Mobile).Number);
            Debug.Assert(srcUser.Phones.First(p => p.Type == SrcUserPhoneType.Home).Number == srcUserAfterBackConvert.Phones.First(p => p.Type == SrcUserPhoneType.Home).Number);
            Debug.Assert(srcUser.Emails.First(p => p.IsPrimary).EmailAddress == srcUserAfterBackConvert.Emails.First(p => p.IsPrimary).EmailAddress);
            Debug.Assert(srcUser.Emails.Where(p => !p.IsPrimary).ElementAt(0).EmailAddress == srcUserAfterBackConvert.Emails.Where(p => !p.IsPrimary).ElementAt(0).EmailAddress);
            Debug.Assert(srcUser.Emails.Where(p => !p.IsPrimary).ElementAt(1).EmailAddress == srcUserAfterBackConvert.Emails.Where(p => !p.IsPrimary).ElementAt(1).EmailAddress);
            Console.WriteLine("Properties in SrcUser(original) and SrcUser(after back convertation from DestUser) are equal");

            Console.ReadLine();
        }
    }
}
