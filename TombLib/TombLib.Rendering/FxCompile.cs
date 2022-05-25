//-----------------------------------------------------------------------
// <copyright file="FxCompile.cs" company="Microsoft">
// Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections;
using System.IO;

namespace Microsoft.Build.Tasks
{
	/// <summary>
	/// Task to support Fxc.exe
	/// </summary>
	public class FxCompile : ToolTask
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public FxCompile()
		{
			// Because FxCop wants it this way.
		}

		#region Inputs

		/// <summary>
		/// Sources to be compiled.
		/// </summary>
		/// <remarks>Required for task to run.</remarks>
		[Required]
		public virtual ITaskItem[] Source
		{
			get => (ITaskItem[])Bag["Sources"];
			set => Bag["Sources"] = value;
		}

		/// <summary>
		/// Gets the collection of parameters used by the derived task class.
		/// </summary>
		/// <value>Parameter bag.</value>
		protected internal Hashtable Bag { get; } = new Hashtable();

		/// <summary>
		/// Specifies the type of shader.  (/T [type]_[model])
		/// <para>ShaderType requires ShaderModel.</para>
		/// </summary>
		/// <remarks>Consider using one of these: "NotSet", "Effect", "Vertex", "Pixel", "Geometry", "Hull", "Domain", "Compute", or "Texture".</remarks>
		public virtual string ShaderType
		{
			get => (string)Bag["ShaderType"];
			set => Bag["ShaderType"] = value.ToLowerInvariant() switch
			{
				"notset" => "",
				"effect" => "/T fx",
				"vertex" => "/T vs",
				"pixel" => "/T ps",
				"geometry" => "/T gs",
				"hull" => "/T hs",
				"domain" => "/T ds",
				"compute" => "/T cs",
				"texture" => "/T tx",
				_ => throw new ArgumentException("ShaderType of " + value + @" is invalid.  Consider using one of these: ""NotSet"", ""Effect"", ""Vertex"", ""Pixel"", ""Geometry"", ""Hull"", ""Domain"", ""Compute"", or ""Texture""."),
			};
		}

		/// <summary>
		/// Specifies the shader model. Some shader types can only be used with recent shader models. (/T [type]_[model])
		/// </summary>
		/// <remarks>ShaderModel requires ShaderType.</remarks>
		public virtual string ShaderModel
		{
			get => (string)Bag["ShaderModel"];
			set => Bag["ShaderModel"] = value;
		}

		/// <summary>
		/// Specifies the contents of assembly language output file. (/Fc, /Fx)
		/// <para>AssemblerOutput requires AssemblerOutputFile.</para>
		/// </summary>
		/// <remarks>Consider using one of these: "Assembly Code" or "Assembly Code and Hex".</remarks>
		public virtual string AssemblerOutput
		{
			get => (string)Bag["AssemblerOutput"];
			set
			{
				bool isValid = value.Equals("Assembly Code", StringComparison.OrdinalIgnoreCase)
					|| value.Equals("Assembly Code and Hex", StringComparison.OrdinalIgnoreCase);

				if (!isValid)
					throw new ArgumentException("AssemblerOutput of " + value + @" is invalid.  Consider using one of these: ""Assembly Code"" or ""Assembly Code and Hex"".");

				Bag["AssemblerOutput"] = value;
			}
		}

		/// <summary>
		/// Specifies file name for assembly code listing file.
		/// </summary>
		/// <remarks>AssemblerOutputFile requires AssemblerOutput.</remarks>
		public virtual string AssemblerOutputFile
		{
			get => (string)Bag["AssemblerOutputFile"];
			set => Bag["AssemblerOutputFile"] = value;
		}

		/// <summary>
		/// Specifies a name for the variable name in the header file. (/Vn [name])
		/// </summary>
		public virtual string VariableName
		{
			get => (string)Bag["VariableName"];
			set => Bag["VariableName"] = value;
		}

		/// <summary>
		/// Specifies a name for header file containing object code. (/Fh [name])
		/// </summary>
		public virtual string HeaderFileOutput
		{
			get => (string)Bag["HeaderFileOutput"];
			set => Bag["HeaderFileOutput"] = value;
		}

		/// <summary>
		/// Specifies a name for object file. (/Fo [name])
		/// </summary>
		public virtual string ObjectFileOutput
		{
			get => (string)Bag["ObjectFileOutput"];
			set => Bag["ObjectFileOutput"] = value;
		}

		/// <summary>
		/// Defines preprocessing symbols for your source file.
		/// </summary>
		public virtual string[] PreprocessorDefinitions
		{
			get => (string[])Bag["PreprocessorDefinitions"];
			set => Bag["PreprocessorDefinitions"] = value;
		}

		/// <summary>
		/// Specifies one or more directories to add to the include path; separate with semi-colons if more than one. (/I[path])
		/// </summary>
		public virtual string[] AdditionalIncludeDirectories
		{
			get => (string[])Bag["AdditionalIncludeDirectories"];
			set => Bag["AdditionalIncludeDirectories"] = value;
		}

		/// <summary>
		/// Suppresses the display of the startup banner and information message. (/nologo)
		/// </summary>
		public virtual bool SuppressStartupBanner
		{
			get => GetBoolParameterWithDefault("SuppressStartupBanner", false);
			set => Bag["SuppressStartupBanner"] = value;
		}

		/// <summary>
		/// Specifies the name of the entry point for the shader. (/E[name])
		/// </summary>
		public virtual string EntryPointName
		{
			get => (string)Bag["EntryPointName"];
			set => Bag["EntryPointName"] = value;
		}

		/// <summary>
		/// Treats all compiler warnings as errors. For a new project, it may be best to use /WX in all compilations;
		/// resolving all warnings will ensure the fewest possible hard-to-find code defects.
		/// </summary>
		public virtual bool TreatWarningAsError
		{
			get => GetBoolParameterWithDefault("TreatWarningAsError", false);
			set => Bag["TreatWarningAsError"] = value;
		}

		/// <summary>
		/// Disable optimizations. /Od implies /Gfp though output may not be identical to /Od /Gfp.
		/// </summary>
		public virtual bool DisableOptimizations
		{
			get => GetBoolParameterWithDefault("DisableOptimizations", false);
			set => Bag["DisableOptimizations"] = value;
		}

		/// <summary>
		/// Enable debugging information.
		/// </summary>
		public virtual bool EnableDebuggingInformation
		{
			get => GetBoolParameterWithDefault("EnableDebuggingInformation", false);
			set => Bag["EnableDebuggingInformation"] = value;
		}

		/// <summary>
		/// Path to Windows SDK
		/// </summary>
		public string SdkToolsPath
		{
			get => (string)Bag["SdkToolsPath"];
			set => Bag["SdkToolsPath"] = value;
		}

		/// <summary>
		/// Name of Fxc.exe
		/// </summary>
		protected override string ToolName => "Fxc.exe";

		#endregion Inputs

		/// <summary>
		/// Returns a string with those switches and other information that can't go into a response file and
		/// must go directly onto the command line.
		/// </summary>
		/// <remarks>Called after ValidateParameters and SkipTaskExecution.</remarks>
		protected override string GenerateCommandLineCommands()
		{
			var commandLineBuilder = new CommandLineBuilderExtension();
			AddCommandLineCommands(commandLineBuilder);

			return commandLineBuilder.ToString();
		}

		/// <summary>
		/// Returns the command line switch used by the tool executable to specify the response file
		/// Will only be called if the task returned a non empty string from GetResponseFileCommands.
		/// </summary>
		/// <remarks>Called after ValidateParameters, SkipTaskExecution and GetResponseFileCommands.</remarks>
		protected override string GenerateResponseFileCommands()
		{
			var commandLineBuilder = new CommandLineBuilderExtension();
			AddResponseFileCommands(commandLineBuilder);

			return commandLineBuilder.ToString();
		}

		/// <summary>
		/// Fills the provided CommandLineBuilderExtension with those switches and other information that can go into a response file.
		/// </summary>
		protected internal virtual void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
		{ }

		/// <summary>
		/// Add Command Line Commands
		/// </summary>
		protected internal void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
		{
			// Order of these affect the order of the command line

			commandLine.AppendSwitchIfNotNull("/I ", AdditionalIncludeDirectories, "");
			commandLine.AppendSwitch(SuppressStartupBanner ? "/nologo" : string.Empty);
			commandLine.AppendSwitchIfNotNull("/E", EntryPointName);
			commandLine.AppendSwitch(TreatWarningAsError ? "/WX" : string.Empty);

			// Switch cannot be null
			if (ShaderType != null && ShaderModel != null)
			{
				// Shader Model and Type are one switch
				commandLine.AppendSwitch(ShaderType + "_" + ShaderModel);
			}

			commandLine.AppendSwitchIfNotNull("/D ", PreprocessorDefinitions, "");
			commandLine.AppendSwitchIfNotNull("/Fh ", HeaderFileOutput);
			commandLine.AppendSwitchIfNotNull("/Fo ", ObjectFileOutput);

			// Switch cannot be null
			if (AssemblerOutput != null)
				commandLine.AppendSwitchIfNotNull(AssemblerOutput, AssemblerOutputFile);

			commandLine.AppendSwitchIfNotNull("/Vn ", VariableName);
			commandLine.AppendSwitch(DisableOptimizations ? "/Od" : string.Empty);
			commandLine.AppendSwitch(EnableDebuggingInformation ? "/Zi" : string.Empty);

			commandLine.AppendSwitchIfNotNull("", Source, " ");
		}

		/// <summary>
		/// Fullpath to the fxc.exe
		/// </summary>
		/// <returns>Fullpath to fxc.exe, if found.  Otherwise empty or null.</returns>
		protected override string GenerateFullPathToTool()
			=> Path.Combine(SdkToolsPath, ToolName);

		/// <summary>
		/// Get a bool parameter and return a default if its not present
		/// in the hash table.
		/// </summary>
		/// <owner>JomoF</owner>
		protected internal bool GetBoolParameterWithDefault(string parameterName, bool defaultValue)
		{
			object obj = Bag[parameterName];
			return (obj == null) ? defaultValue : (bool)obj;
		}
	}
}
