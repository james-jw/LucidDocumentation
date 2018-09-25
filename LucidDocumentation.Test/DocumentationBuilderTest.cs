using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LucidDocumentation.Test
{
    [TestClass]
    public class DocumentationBuilderTest 
    {
        [TestMethod]
        public void BuildDocumentationTest()
        {
            var path = @"..\..\..\LucidDocumentation\bin\Debug\netstandard2.0\LucidDocumentation.dll";
            path = @"C:\Projects\LucidUnits\LucidUnits\bin\Release\netstandard2.0\LucidUnits.dll";
            path = @"C:\Projects\SE.Web.ContextualFeatureApi\SE.Web.ContextualFeatureApi\bin\Debug\SE.Web.ContextualFeatureApi.dll";

            var docBuilder = new DocumentationBuilder(@"C:\Projects\SE.Web.ContextualFeatureApi\packages\");
            var documentation = docBuilder.BuildDocumentation(Path.GetFullPath(path));

            Assert.AreEqual("", documentation);
        }
    }
}
