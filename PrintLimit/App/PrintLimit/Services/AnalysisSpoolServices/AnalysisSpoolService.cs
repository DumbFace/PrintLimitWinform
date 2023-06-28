using PrintLimit.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PrintLimit.Services.AnalysisSpoolServices
{
    class AnalysisSpoolService : IAnalysisSpoolService
    {
        public PrintJobModel GetCopyDuplexPaperSize(ZipArchive archive)
        {
            PrintJobModel infoPrintJob = new PrintJobModel();
            // Tìm các entry liên quan đến thư mục "Metadata"
            var entriesInMetadataDirectory = archive.Entries
                .Where(entry => entry.FullName.StartsWith("Metadata" + "/"));
            // var metaDataEntry = archive.GetEntry("Metadata");

            XmlDocument doc = new XmlDocument();
            if (entriesInMetadataDirectory != null)
            {
                foreach (var entry in entriesInMetadataDirectory)
                {
                    if (entry != null)
                    {
                        try
                        {
                            using (Stream stream = entry.Open())
                            {
                                doc.Load(stream);

                                XmlNode parameterInitNode = doc.SelectSingleNode("//psf:ParameterInit[@name='psk:JobCopiesAllDocuments']", GetNamespaceManager(doc));
                                if (parameterInitNode != null)
                                {
                                    XmlNode valueNode = parameterInitNode.SelectSingleNode("psf:Value", GetNamespaceManager(doc));
                                    if (valueNode != null)
                                    {
                                        int value = Convert.ToInt32(valueNode.InnerText);

                                        infoPrintJob.Copies = value;
                                    }
                                }
                                XmlNode oneSided = doc.SelectSingleNode("//psf:Option[@name='psk:OneSided']", GetNamespaceManager(doc));
                                XmlNode twoSidedLong = doc.SelectSingleNode("//psf:Option[@name='psk:TwoSidedLongEdge']", GetNamespaceManager(doc));
                                XmlNode twoSidedShort = doc.SelectSingleNode("//psf:Option[@name='psk:TwoSidedShortEdge']", GetNamespaceManager(doc));

                                if (oneSided != null)
                                {
                                    XmlAttribute nameAttribute = oneSided.Attributes["name"];
                                    if (nameAttribute != null)
                                    {
                                        string nameValue = nameAttribute.Value;
                                        string duplex = nameValue.Split(':')[1];
                                        infoPrintJob.Duplex = duplex == "OneSided" ? false : true;
                                    }
                                }

                                if (twoSidedLong != null)
                                {
                                    XmlAttribute nameAttribute = twoSidedLong.Attributes["name"];
                                    if (nameAttribute != null)
                                    {
                                        string nameValue = nameAttribute.Value;
                                        string duplex = nameValue.Split(':')[1];
                                        infoPrintJob.Duplex = duplex == "TwoSidedLongEdge" ? true : false;
                                    }
                                }

                                if (twoSidedShort != null)
                                {
                                    XmlAttribute nameAttribute = twoSidedShort.Attributes["name"];
                                    if (nameAttribute != null)
                                    {
                                        string nameValue = nameAttribute.Value;
                                        string duplex = nameValue.Split(':')[1];
                                        infoPrintJob.Duplex = duplex == "TwoSidedShortEdge" ? true : false;
                                    }
                                }

                                XmlNode pageMediaSize = doc.SelectSingleNode("//psf:Feature[@name='psk:PageMediaSize']", GetNamespaceManager(doc));
                                if (pageMediaSize != null)
                                {
                                    if (pageMediaSize.InnerXml.ToLower().Contains("a4"))
                                    {
                                        infoPrintJob.PaperSize = "A4 8.27\" x 11.69\"";
                                    }
                                    else if (pageMediaSize.InnerXml.ToLower().Contains("letter"))
                                    {
                                        infoPrintJob.PaperSize = "Letter 8.5\" x 11\"";
                                    }
                                    else if (pageMediaSize.InnerXml.ToLower().Contains("a5"))
                                    {
                                        infoPrintJob.PaperSize = "A5 5.83\" x 8.27\"";
                                    }
                                    else if (pageMediaSize.InnerXml.ToLower().Contains("legal"))
                                    {
                                        infoPrintJob.PaperSize = "Legal 8.5\" x 14\"";
                                    }
                                    else if (pageMediaSize.InnerXml.ToLower().Contains("executive"))
                                    {
                                        infoPrintJob.PaperSize = "Executive 7.25\" x 10.5\"";
                                    }         
                                }


                            }
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine("Exp: " + ex.Message);
                        }

                    }
                }
                // Console.WriteLine()
            }



            return infoPrintJob;
        }

        public int GetTotalPages(ZipArchive archive)
        {
            int totalPages = 0;
            int i = 1;

            var entriesPages = archive.Entries.Where(
                       entry =>
                       entry.FullName.StartsWith("Documents/1/Pages/")
                       );

            foreach (var item in entriesPages)
            {
                if (item.FullName.Contains($"{i}.fpage"))
                {
                    totalPages++;
                    i++;
                }
            }



            return totalPages;
        }


        public static XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("psf", "http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework");
            nsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            nsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            nsManager.AddNamespace("ns0000", "http://schemas.monotypeimaging.com/ptpc/2006/1");
            nsManager.AddNamespace("psk", "http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords");

            return nsManager;
        }


    }
}
