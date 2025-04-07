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
        public static string xmlURL = "https://AryashDubey.github.io/Assignment4CSE445Aryash/Hotels.xml";
        public static string xmlErrorURL = "https://AryashDubey.github.io/Assignment4CSE445Aryash/HotelsErrors.xml"; 
        public static string xsdURL = "https://AryashDubey.github.io/Assignment4CSE445Aryash/Hotels.xsd"; 

        public static void Main(string[] args)
        {
           string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        public static string Verification(string xmlUrl, string xsdUrl)
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

                WebClient clt = null;
                StringReader strRdr = null;
                StringReader strRdr2 = null;
                XmlReader rdr = null;

                try
                {
                    clt = new WebClient();
                    string xsdContent = clt.DownloadString(xsdUrl);
                    strRdr = new StringReader(xsdContent);
                    XmlSchema schma = null;

                    try
                    {
                        schma = XmlSchema.Read(strRdr, null);
                        rederSettings.Schemas.Add(schma);
                    }
                    finally
                    {
                        if (strRdr != null)
                        {
                            strRdr.Dispose();
                        }
                    }

                    string contentForTheXml = clt.DownloadString(xmlUrl);
                    strRdr2 = new StringReader(contentForTheXml);
                    try
                    {
                        rdr = XmlReader.Create(strRdr2, rederSettings);
                        while (rdr.Read()) { }
                    }
                    finally
                    {
                        if (rdr != null)
                        {
                            rdr.Close();
                        }
                        if (strRdr2 != null)
                        {
                            strRdr2.Dispose();
                        }
                    }
                }
                finally
                {
                    if (clt != null)
                    {
                        clt.Dispose();
                    }
                }

                if (customErrMessages.Length > 0)
                {
                    return customErrMessages.ToString();
                }
                else
                {
                    return "No Error";
                }
            }
            catch (WebException ex)
            {
                return $"Network error: {ex.Message}";
            }
            catch (XmlException ex)
            {
                return $"XML parsing error: Line {ex.LineNumber}, Position {ex.LinePosition}, {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public static string Xml2Json(string xmlUrl)
        {
            WebClient clt = null;
            try
            {
                clt = new WebClient();
                string contentForTheXml = clt.DownloadString(xmlUrl);

                XmlDocument docForTheXml = new XmlDocument();
                docForTheXml.LoadXml(contentForTheXml);

                string jsonText = JsonConvert.SerializeXmlNode(docForTheXml, Newtonsoft.Json.Formatting.Indented, true);
                
                jsonText = jsonText.Replace("\"@Rating\"", "\"_Rating\"");
                jsonText = jsonText.Replace("\"@NearstAirport\"", "\"_NearstAirport\"");
                
                return jsonText;
            }
            catch (Exception ex)
            {
                return $"Error converting XML to JSON: {ex.Message}";
            }
            finally
            {
                if (clt != null)
                {
                    clt.Dispose();
                }
            }
        }
    }
}