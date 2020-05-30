using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

public class Sample
{
    public static void Main(string[] args)
    {
        List<string> files = new List<string>(args);
        HashSet<string> processed = new HashSet<string>();
        int count = files.Count;

        for (int i = 0; i < count; i++)
        {
            string filename = Path.GetRelativePath(Directory.GetCurrentDirectory(), files[i]);

            if (processed.Contains(filename))
            {
                Console.Error.WriteLine("SKIP {0}", filename);
                continue;
            }
            Console.Error.WriteLine("OPEN {0}", filename);
            processed.Add(filename);

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filename);
            }
            catch (XmlException x)
            {
                Console.Error.WriteLine("{0}:{1}:{2}:{3}", filename, "Exception", x.LineNumber, x.Message);
                continue;
            }

            XmlNamedNodeMap nMap = doc.DocumentType.Entities;
            Uri current = new Uri(doc.BaseURI);
            string localPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), current.LocalPath);

            for (int j = 0; j < nMap.Count; j++)
            {
                XmlEntity ent = (XmlEntity) nMap.Item(j);
                Uri related = null;
                Uri.TryCreate(current, ent.SystemId, out related);
                if (related != null)
                {
                    string relatedPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), related.LocalPath);
                    if (!processed.Contains(relatedPath))
                    {
                        files.Add(relatedPath);
                        count++;
                    }
                    Console.WriteLine("{0}:{1}:{2}:{3}", localPath, "EntityInclusion", ent.Name, relatedPath);
                    Console.Error.WriteLine("• {0}", String.Join(",", files));
                }
                else
                {
                    Console.WriteLine("{0}:{1}:{2}:{3}", localPath, "EntityDeclaration", ent.Name, ent.InnerText);
                }
            }

            XmlTextReader reader = null;

            try
            {
                reader = new XmlTextReader(filename);
                reader.WhitespaceHandling = WhitespaceHandling.None;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.EntityReference:
                            Console.WriteLine("{0}:{1}:{2}:{3}", localPath, "EntityInclusion", reader.Name, "");
                            break;
                    }
                }
            }

            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
