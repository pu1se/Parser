﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.BL;

namespace Parser.Tests
{
    [TestClass]
    public class TataTravelParserTests
    {
        [TestMethod]
        public void Foo()
        {
            new TataTravelParser().GetBestPrice();
        }
    }
}
