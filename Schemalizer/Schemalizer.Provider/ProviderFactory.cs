﻿// Authors="Daniel Jonas Møller, Anders Eggers-Krag" License="New BSD License http://sqline.codeplex.com/license"
using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Diagnostics;

namespace Schemalizer.ProviderModel {
	public class ProviderFactory {

		public static ISchemalizerProvider Create(string name) {
			Debug.WriteLine(Assembly.GetExecutingAssembly().CodeBase);
			Debug.WriteLine("Schemalizer::ProviderFactory::Create: " + name);
			if (name.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)) {
				Assembly OAssembly = Assembly.Load("Schemalizer.ProviderModel.PostgreSql");
				Type OType = OAssembly.GetType("Schemalizer.ProviderModel.PostgreSql.PostgreSqlProvider");
				return (ISchemalizerProvider)Activator.CreateInstance(OType);
			}
			else if (name.Equals("MySql", StringComparison.OrdinalIgnoreCase)) {
				Assembly OAssembly = Assembly.Load("Schemalizer.ProviderModel.MySql");
				Type OType = OAssembly.GetType("Schemalizer.ProviderModel.MySql.MySqlProvider");
				return (ISchemalizerProvider)Activator.CreateInstance(OType);
			}
            else if (name.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)) {
                Assembly OAssembly = Assembly.Load("Schemalizer.ProviderModel.SqlServer");
                Type OType = OAssembly.GetType("Schemalizer.ProviderModel.SqlServer.SqlServerProvider");
                return (ISchemalizerProvider)Activator.CreateInstance(OType);
            }
            else {
				Assembly OAssembly = Assembly.Load($"Schemalizer.ProviderModel.{name}");
				Type OType = OAssembly.GetType($"Schemalizer.ProviderModel.{name}.{name}Provider");
				return (ISchemalizerProvider)Activator.CreateInstance(OType);
			}
		}
	}
}