﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sqline.Tests.DataAccess.DataItems;
using Sqline.Tests.DataAccess;
using System.Configuration;

namespace Sqline.Tests.UnitTests {

	[TestClass]
	public static class Initialize {

		[AssemblyInitialize]
		public static void InitializeTests(TestContext context) {
			DA.Initialize(ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString);
		}
	}
}
