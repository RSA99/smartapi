﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class BasicAlignmentTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<BasicAlignment>.TestConversionWithoutExcludedValues(BasicAlignmentUtils.ToRQLString,
                                                                BasicAlignmentUtils.ToBasicAlignment);
        }
    }
}