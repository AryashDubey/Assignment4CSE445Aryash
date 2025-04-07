using System;
using System.Xml.Schema;
using System.Xml;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConsoleApp1
{
    public class Program
    {
        public static string urlForGithub = "https://AryashDubey.github.io/Hotels.xml";
        public static string errorUrlForGithub = "https://AryashDubey.github.io/HotelsErrors.xml"; 
        public static string xsdUrlForGithub = "https://AryashDubey.github.io/Hotels.xsd"; 

        public static void Main(string[] args)Ï
        {
            string endRes = urlVerificationForGH(urlForGithub, xsdUrlForGithub);
            Console.WriteLine("URL Verification for Github: " + endRes);
            endRes = urlVerificationForGH(errorUrlForGithub, xsdUrlForGithub);
            Console.WriteLine("URL Verification for Github: " + endRes);

            endRes = Xml2Json(urlForGithub);
            Console.WriteLine("XML to JSON for Github: " + endRes);
        }

        public static string urlVerificationForGH(string urlForGithub, string xsdUrlForGithub)
        {
            try
            {
                XmlReaderSettings rederSettings = new XmlReaderSettings();
                rederSettings.ValidationType = ValidationType.Schema;
                rederSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                
                StringBuilder customErrMessages = new StringBuilder();
                rederSettings.ValidationEventHandler += (sender, e) => {
                    customErrMessages.AppendLine($"Line: {e.Exception.LineNumber}, Position: {e.Exception.LinePosition}, Error: {e.Message}");
                };

                using (WebClient clt = new WebClient())
                {
                    string xsdContent = clt.DownloadString(xsdUrlForGithub);
                    using (StringReader strRdr = new StringReader(xsdContent))
                    {
                        using (XmlReader schRdr = XmlReader.Create(strRdr))
                        {
                            XmlSchema schma = XmlSchema.Read(schRdr, null);
                            rederSettings.Schemas.Add(schma);
                        }
                    }
                }

                using (WebClient clt = new WebClient())
                {
                    string contentForTheXml = clt.DownloadString(urlForGithub);
                    using (StringReader strRdr = new StringReader(contentForTheXml))
                    {
                        using (XmlReader rdr = XmlReader.Create(strRdr, rederSettings))
                        {
                            while (rdr.Read()) { }
                        }
                    }
                }

                if (customErrMessages.Length == 0)
                {
                    return "No Error";
                }
                else
                {
                    return customErrMessages.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"We got an error: {ex.Message}";
            }
        }

        public static string Xml2Json(string urlForGithub)
        {
            try
            {
                string contentForTheXml;
                using (WebClient clt = new WebClient())
                {
                    contentForTheXml = clt.DownloadString(urlForGithub);
                }

                XmlDocument docForTheXml = new XmlDocument();
                docForTheXml.LoadXml(contentForTheXml);

                string jsTxt = JsonConvert.SerializeXmlNode(docForTheXml, Newtonsoft.Json.Formatting.Indented, true);
                
                return jsTxt;
            }
            catch (Exception ex)
            {
                return $"We got an error: {ex.Message}";
            }
        }
    }
}