﻿// Authors="Daniel Jonas Møller, Anders Eggers-Krag" License="New BSD License http://sqline.codeplex.com/license"
using System;
using System.Xml.Linq;
using Sqline.ClientFramework.ProviderModel;

namespace Sqline.CodeGeneration.ViewModel {
	public class Parameter {
		private string FName;
		private string FType;
		private bool FNullable;
		private ITypeMapping FTypeMapping;

		public Parameter(XElement element) {
			if (element.Attribute("name") == null) {
				//TODO: Throw error
			}
			if (element.Attribute("type") == null) {
				//TODO: Throw error
			}
			FName = element.Attribute("name").Value;
			FType = element.Attribute("type").Value;
			FTypeMapping = Provider.Current.GetTypeMapping(FType);
			if (element.Attribute("nullable") != null) {
				FNullable = element.Attribute("nullable").Value.Equals("true", StringComparison.OrdinalIgnoreCase);
			}
		}

		public string Name {
			get {
				return FName;
			}
			set {
				FName = value;
			}
		}

		public string Type {
			get {
				return FType;
			}
			set {
				FType = value;
			}
		}

		public bool Nullable {
			get {
				return FNullable;
			}
			set {
				FNullable = value;
			}
		}

		public String CsType {
			get {
				if (FNullable) {
					return FTypeMapping.CSNullable;
				}
				return FTypeMapping.CSType;
			}
		}

		public String CsTypeNonNullable {
			get {
				return FTypeMapping.CSType;
			}
		}
	}
}