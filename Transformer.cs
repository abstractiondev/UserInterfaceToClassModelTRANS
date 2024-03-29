﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using ClassModel_v1_0;
using UserInterface_v1_0;
using ClassesType = ClassModel_v1_0.ClassesType;
using ClassType = ClassModel_v1_0.ClassType;
using CM=ClassModel_v1_0;
using UI=UserInterface_v1_0;

namespace UserInterfaceToClassModelTRANS
{
    public class Transformer
    {
        T LoadXml<T>(string xmlFileName)
        {
            using (FileStream fStream = File.OpenRead(xmlFileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T result = (T)serializer.Deserialize(fStream);
                fStream.Close();
                return result;
            }
        }



	    public Tuple<string, string>[] GetGeneratorContent(params string[] xmlFileNames)
	    {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();
            foreach(string xmlFileName in xmlFileNames)
            {
                UI.UserInterfaceAbstractionType fromAbs = LoadXml<UI.UserInterfaceAbstractionType>(xmlFileName);
                CM.ClassModelAbstractionType  toAbs = TransformAbstraction(fromAbs);
                string xmlContent = WriteToXmlString(toAbs);
                FileInfo fInfo = new FileInfo(xmlFileName);
                string contentFileName = "ClassModelFrom" + fInfo.Name;
                result.Add(Tuple.Create(contentFileName, xmlContent));
            }
	        return result.ToArray();
	    }

        private string WriteToXmlString(CM.ClassModelAbstractionType toAbs)
        {
            XmlSerializer serializer = new XmlSerializer(toAbs.GetType());
            MemoryStream memoryStream = new MemoryStream();
            serializer.Serialize(memoryStream, toAbs);
            byte[] data = memoryStream.ToArray();
            string result = System.Text.Encoding.UTF8.GetString(data);
            return result;
        }

        public static CM.ClassModelAbstractionType TransformAbstraction(UI.UserInterfaceAbstractionType fromAbs)
        {
            CM.ClassModelAbstractionType toAbs = new ClassModelAbstractionType
                                                     {
                                                         Classes = ToClasses(fromAbs.UserInterfaces)
                                                     };
            return toAbs;
        }

        private static ClassesType[] ToClasses(UserInterfaceType[] userInterfaces)
        {
            var result = userInterfaces.Select(ui => new ClassesType
                                                   {
                                                       Class =
                                                           ui.SemanticModel.Select(ToClassModelClass)
                                                           .ToArray(),
                                                       namespaceName = ui.domainName + "." + ui.name
                                                   }).ToArray();
            return result;
        }

        private static CM.ClassType ToClassModelClass(UI.ClassType uiClass)
        {
            return new CM.ClassType
                       {
                           designDesc = uiClass.designDesc,
                           name = uiClass.name,
                           Properties = uiClass.Properties.Select(ToClassModelProperty).ToArray()
                       };
        }

        private static CM.PropertyType ToClassModelProperty(UI.PropertyType prop)
        {
            return new CM.PropertyType
                       {
                           name = prop.name,
                           dataType = prop.dataType
                       };
        }
    }
}
