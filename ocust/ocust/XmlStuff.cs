using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace ocust
{
    public class XmlStuff
    {
        public bool m_Success = true;

        public bool validateXml(String infile)
        {
            //First we create the xmltextreader
            XmlTextReader xmlr = new XmlTextReader(infile);
            //We pass the xmltextreader into the xmlvalidatingreader
            //'This will validate the xml doc with the schema file
            //'NOTE the xml file it self points to the schema file
            XmlValidatingReader xmlvread = new XmlValidatingReader(xmlr);
            //
            //      //                      ' Set the validation event handler
            xmlvread.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            m_Success = true; //'make sure to reset the success var
            //
            //      //' Read XML data
            while (xmlvread.Read()) { }

            //'Close the reader.
            xmlvread.Close();

            //'The validationeventhandler is the only thing that would set m_Success to false
            return m_Success;
        }

        public void ValidationCallBack(Object sender, ValidationEventArgs args)
        {
            //'Display the validation error.  This is only called on error
            m_Success = false; //'Validation failed
            //writertbox("Validation error: " + args.Message);
            App.addDebugLine("Validation Error: " + args.Message);
        }

        public bool validateSchema(String infilename)
        {
            //this function will validate the schema file (xsd)

            XmlSchema myschema;
            m_Success = true; //'make sure to reset the success var
            StreamReader sr = new StreamReader(infilename);
            try
            {
                //sr = new StreamReader(infilename);
                myschema = XmlSchema.Read(sr, new ValidationEventHandler(ValidationCallBack));
                //'This compile statement is what ususally catches the errors
                myschema.Compile(new ValidationEventHandler(ValidationCallBack));
            }
            finally
            {
                sr.Close();
            }
            return m_Success;
        }
    }
}