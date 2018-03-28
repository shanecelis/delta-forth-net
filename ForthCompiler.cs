/*
 * Delta Forth .NET - World's first Forth compiler for the .NET platform
 * Copyright (C)1997-2002 Valer BOCAN, Romania (vbocan@dataman.ro, http://www.dataman.ro)
 * 
 * This program and its source code is distributed in the hope that it will
 * be useful. No warranty of any kind is provided.
 * Please DO NOT distribute modified copies of the source code.
 * 
 * If you like this software, please make a donation to a charity of your choice.
 */

using System;
using System.Collections;
using System.IO;

namespace DeltaForth
{
	/// <summary>
	/// Summary description for SystemLauncher.
	/// </summary>
	class SystemLauncher
	{
		// Compiler command-line options
		static bool bDisplayLogo, bQuiet, bClock, bExe, bCheckStack, bMap;
		static int iForthStackSize = 524288, iReturnStackSize = 1024;
		static string sInput;		// Input filename
		static string sOutputFile;	// Output filename
		static string sOutputDir;	// Output directory

		static void DisplayLogo()
		{
			Console.WriteLine("Delta Forth .NET Compiler, Version 1.0");
			Console.WriteLine("Copyright (C) Valer BOCAN (http://www.dataman.ro). All Rights Reserved.\n\r");
		}

		static void Usage()
		{
			DisplayLogo();
			Console.WriteLine("Usage: DeltaForth.exe <source file> [options]");
			Console.WriteLine("\n\rOptions:");
			Console.WriteLine("/NOLOGO\t\t\tDon't type the logo");
			Console.WriteLine("/QUIET\t\t\tDon't report compiling progress");
			Console.WriteLine("/CLOCK\t\t\tMeasure and report compilation times");
			Console.WriteLine("/EXE\t\t\tCompile to EXE (default)");
			Console.WriteLine("/DLL\t\t\tCompile to DLL");
			Console.WriteLine("/NOCHECK\t\tDisable stack bounds checking");
			Console.WriteLine("/FS:<size>\t\tSpecify Forth stack size (default is 524288 cells)");
			Console.WriteLine("/RS:<size>\t\tSpecify return stack size (default is 1024 cells)");
			Console.WriteLine("/MAP\t\t\tGenerate detailed map information");
			Console.WriteLine("/OUTPUT=<targetfile>\tCompile to file with specified name\n\r\t\t\t(user must provide extension, if any)");
			Console.WriteLine("\n\rDefault source file extension is .4th");
		}

		static void Main(string[] args)
		{
			// Initialize default parameter values
			bDisplayLogo = bExe = bCheckStack = true;
			bQuiet = bClock = bMap = false;
			sOutputFile = sOutputDir = "";

			// Display usage screen if no parameters are given
			if(args.Length < 1)
			{
				Usage();
				return;
			}

			sInput = args[0];

			// Cycle through command line parameters
			for(int i = 1; i < args.Length; i++)
			{
				switch(args[i].ToUpper())
				{
					case "/NOLOGO":
						bDisplayLogo = false;
						break;
					case "/QUIET":
						bQuiet = true;
						break;
					case "/CLOCK":
						bClock = true;
						break;
					case "/EXE":
						bExe = true;
						break;
					case "/DLL":
						bExe = false;
						break;
					case "/NOCHECK":
						bCheckStack = false;
						break;
					case "/MAP":
						bMap = true;
						break;
					default:
						if(args[i].ToUpper().StartsWith("/OUTPUT="))
						{
							sOutputFile = args[i].Substring(8);
						} 
						else
							if(args[i].ToUpper().StartsWith("/FS:"))
						{
							try 
							{
								iForthStackSize = Convert.ToInt32(args[i].Substring(4));
							}
							catch(FormatException)
							{
								Usage();
								return;
							}
						}
						else
							if(args[i].ToUpper().StartsWith("/RS:"))
						{
							try 
							{
								iReturnStackSize = Convert.ToInt32(args[i].Substring(4));
							}
							catch(FormatException)
							{
								Usage();
								return;
							}
						}
						else 
						{
							Usage();
							return;
						}
						break;
				}
			}
			
			// Process input file
			string inDrive = "", inDir = "", inFile = "", inExt = "";
			FileNameSplit(sInput, out inDrive, out inDir, out inFile, out inExt);
			if(inExt == "") sInput = inDrive + inDir + inFile + ".4th";

			// Process parameters
			if(bDisplayLogo) DisplayLogo();
			if(sOutputFile == "")
			{
				sOutputFile = inFile + (bExe ? ".exe" : ".dll");
				sOutputDir = null;
			}
			else 
			{
				FileNameSplit(sOutputFile, out inDrive, out inDir, out inFile, out inExt);
				sOutputFile = inFile + inExt;
				sOutputDir = inDrive + inDir;
				if(sOutputDir == "") sOutputDir = null;
			}
			
			if(!bQuiet) Console.WriteLine("Compiling file '{0}' to '{1}'", sInput, sOutputDir + sOutputFile);

			try 
			{
				ForthCompiler fc = new ForthCompiler(sInput, sOutputFile, sOutputDir, bQuiet, bClock, bExe, bCheckStack, iForthStackSize, iReturnStackSize, bMap);
			} 
			catch(Exception e)
			{
				Console.WriteLine("\n\rCompiling error: {0}", e.Message);
				if(!bQuiet) Console.WriteLine("Operation failed.");
				return;
			}
			if(!bQuiet)	Console.WriteLine("Operation completed successfully.");
		}

		// Returns the file name components
		static private void FileNameSplit(string FullName, out string Drive, out string Directory, out string FileName, out string Extension)
		{
			int pos;
			Drive = Directory = FileName = Extension = "";

			// Detect 'drive' string
			pos = FullName.IndexOf(':');
			if(pos != -1)
			{
				Drive = FullName.Substring(0, pos + 1);
				FullName = FullName.Remove(0, pos + 1);
			}
			// Detect 'directory' string
			pos = FullName.LastIndexOf('\\');
			if(pos != -1)
			{
				Directory = FullName.Substring(0, pos + 1);
				FullName = FullName.Remove(0, pos + 1);
			}
			// Detect 'file name' and 'extension' strings
			pos = FullName.IndexOf('.');
			if(pos != -1) 
			{
				FileName = FullName.Substring(0, pos);
				Extension = FullName.Substring(pos);
			} 
			else
			{
				FileName = FullName;
				Extension = "";
			}
		}
	}

	// Atom
	// Definition of an atom as used by the Forth parser
	struct ForthAtom
	{
		public string Name;			// Atom name
		public string FileName;		// File where the atom occured
		public int LineNumber;		// Line number at which the atom occured
		// Atom constructor
		public ForthAtom(string p_Name, string p_FileName, int p_LineNumber)
		{
			Name = p_Name; FileName = p_FileName; LineNumber = p_LineNumber;
		}
	}

	// Variable structure
	// Definition of a global variable as used by the Forth syntactic analyzer
	struct ForthVariable
	{
		public string Name;			// Variable name
		public int Size;			// Number of cells required by the variable
		public int Address;			// Address of the variable (computed by the code generator)
		// Variable constructor
		public ForthVariable(string p_Name, int p_Size)
		{
			Name = p_Name; Size = p_Size; Address = 0;
		}
	}

	// Local variable structure
	// Definition of a local variable as used by the Forth syntactic analyzer
	struct ForthLocalVariable
	{
		public string Name;			// Variable name
		public string WordName;		// Word where the variable has been defined
		public int Address;			// Address of the variable (computed by the code generator)
		// Variable constructor
		public ForthLocalVariable(string p_Name, string p_WordName)
		{
			Name = p_Name; WordName = p_WordName; Address = 0;
		}
	}

	// Constant structure
	// Definition of a global constant as used by the Forth syntactic analyzer
	struct ForthConstant
	{
		public string Name;			// Constant name
		public object Value;		// Constant value (can be either string or integer)

		// Constant constructors
		public ForthConstant(string p_Name, object p_Value)
		{
			Name = p_Name; Value = p_Value;
		}
	}

	// ForthWord structure
	// Definition of a word as used by the Forth syntactic analyzer
	class ForthWord
	{
		public string Name;				// Word name
		public ArrayList Definition;	// List of atoms that define the word

		// ForthWord constructor
		public ForthWord(string p_Name)
		{
			Name = p_Name;
			Definition = new ArrayList();
		}
	}

	// ExternalWord structure
	// Definition of an external word as used by the Forth syntactic analyzer
	class ExternalWord
	{
		public string Name;				// Word name
		public string Library;			// Library filename
		public string Class;			// Class name
		public string Method;			// Method name

		// ExternalWord constructor
		public ExternalWord(string p_Name, string p_Library, string p_Class, string p_Method)
		{
			Name = p_Name;
			Library = p_Library;
			Class = p_Class;
			Method = p_Method;
		}
	}

	/// <summary>
	/// Delta Forth - The .NET Forth Compiler
	/// (C) Valer BOCAN (vbocan@dataman.ro)
	/// 
	/// Class ForthCompiler
	/// 
	/// Date of creation:		Wednesday, September  5, 2001
	/// Date of last update:	Tuesday,   January 15, 2002
	/// 
	/// Description:
	///		The ForthCompiler class requires a Forth source file as a parameter and an output file
	///		to generate code to.
	/// </summary>
	class ForthCompiler
	{
		// From the lexcial analyzer
		ArrayList SourceAtoms;		// List of atoms in the source file provided by the parser
		// From the syntactic analizer
		ArrayList GlobalConstants;	// List of globally defined constants
		ArrayList GlobalVariables;	// List of globally defined variables
		ArrayList LocalVariables;	// List of locally defined variables
		ArrayList Words;			// List of Forth words and their contents
		ArrayList ExternalWords;	// List of Forth external words
		string LibraryName;			// Name of the library to be created
		
		public ForthCompiler(string SourceFileName, string TargetFileName, string TargetDirectory, bool bQuiet, bool bClock, bool bExe, bool bCheckStack, int iForthStackSize, int iReturnStackSize, bool bMap)
		{
			// Initialize start time
			DateTime dtStart = DateTime.Now;
			TimeSpan ts1, ts2, ts3;


			// Parse the source file
			if(!bQuiet) Console.Write("Parsing file...\t\t  ");
			ForthParser fp = new ForthParser(SourceFileName);
			ts1 = (DateTime.Now - dtStart);
			if(!bQuiet) 
			{
				if(bClock) Console.WriteLine("Done in {0} ms", ts1.Milliseconds);
				else Console.WriteLine("Done");
			}
			SourceAtoms = fp.SourceAtoms;
			// Perform a syntactic analysis of the source atoms
			if(!bQuiet) Console.Write("Analyzing source atoms... ");
			ForthSyntacticAnalyzer fsa = new ForthSyntacticAnalyzer(SourceAtoms);
			ts2 = (DateTime.Now - dtStart);
			if(!bQuiet) 
			{
				if(bClock) Console.WriteLine("Done in {0} ms", ts2.Milliseconds);
				else Console.WriteLine("Done");
			}
			fsa.GetMetaData(out LibraryName, out GlobalConstants, out GlobalVariables, out LocalVariables, out Words, out ExternalWords);
			// Generate code
			if(!bQuiet) Console.Write("Generating code...\t  ");
			ForthCodeGenerator fcg = new ForthCodeGenerator(TargetFileName, TargetDirectory, LibraryName, GlobalConstants, GlobalVariables, LocalVariables, Words, ExternalWords, bExe, bCheckStack, iForthStackSize, iReturnStackSize);
			fcg.DoGenerateCode();
			ts3 = (DateTime.Now - dtStart);
			if(!bQuiet) 
			{
				if(bClock) Console.WriteLine("Done in {0} ms", ts3.Milliseconds);
				else Console.WriteLine("Done");
			}
			if(!bQuiet) 
			{
				if(bClock) Console.WriteLine("Total compiling time:\t  {0} ms", ts1.Milliseconds + ts2.Milliseconds + ts3.Milliseconds);
				Console.WriteLine();
				if(GlobalConstants.Count > 0) Console.WriteLine("Constants:\t\t{0}", GlobalConstants.Count);
				if(GlobalVariables.Count > 0) Console.WriteLine("Global variables:\t{0}", GlobalVariables.Count);
				if(LocalVariables.Count > 0) Console.WriteLine("Local variables:\t{0}", LocalVariables.Count);
				if(Words.Count > 0) Console.WriteLine("Words:\t\t\t{0}", Words.Count);
				if(ExternalWords.Count > 0)Console.WriteLine("External words:\t\t{0}", ExternalWords.Count);
				Console.WriteLine();
			}
			// Display detailed map data (/MAP option)
			if(bMap && !bQuiet)
			{
				DisplayMapInformation();
			}
		}

		private void DisplayMapInformation()
		{
			int i;

			Console.WriteLine("--- Map Information -------------------------------------\n\r");
			Console.WriteLine("* Global constants:");
			// Display global constants
			for(i = 0; i < GlobalConstants.Count; i++) 
			{
				ForthConstant fc = (ForthConstant)GlobalConstants[i];
				if(fc.Value.GetType() != typeof(string)) 
				{
					Console.WriteLine("{0} = {1}", fc.Name, fc.Value);
				} 
				else
				{
					Console.WriteLine("{0} = \"{1}\"", fc.Name, fc.Value);
				}
			}
			// Display global variables
			Console.WriteLine("\n* Global variables:");
			for(i = 0; i < GlobalVariables.Count; i++) 
			{
				ForthVariable fv = (ForthVariable)GlobalVariables[i];
				Console.WriteLine("{0} = (Addr:{1}, Size:{2})", fv.Name, fv.Address, fv.Size);
			}
			// Display local variables
			Console.WriteLine("\n* Local variables:");
			for(i = 0; i < LocalVariables.Count; i++) 
			{
				ForthLocalVariable flv = (ForthLocalVariable)LocalVariables[i];
				Console.WriteLine("{0} = (Addr:{1}, Word:{2})", flv.Name, flv.Address, flv.WordName);
			}
			// Display external words
			Console.WriteLine("\n* External words:");
			for(i = 0; i < ExternalWords.Count; i++) 
			{
				ExternalWord ew = (ExternalWord)ExternalWords[i];
				Console.WriteLine("{0} = (Library:{1}, Class:{2}, Method:{3})", ew.Name, ew.Library, ew.Class, ew.Method);
			}
			
			Console.WriteLine();
		}
	}
}
