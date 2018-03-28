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
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace DeltaForth
{
	/// <summary>
	/// Delta Forth - The .NET Forth Compiler
	/// (C) Valer BOCAN (vbocan@dataman.ro)
	/// 
	/// Class ForthParser
	/// 
	/// Date of creation:		Wednesday, September  5, 2001
	/// Date of last update:	Monday,    September 10, 2001
	/// 
	/// Description:
	///	
	/// </summary>
	public class ForthParser
	{
		private bool CommentMode = false;		// Comment mode (TRUE if atoms should be ignored)
		private ArrayList ProcessedFiles;		// Processed file list (so we don't process the same file multiple times in case of circular references)
		public ArrayList SourceAtoms;			// List of atoms in source files (exported)

		// ForthParser constructor - Builds up the internal atom table
		// Input:  SourceFileName - the name of the source file to be processed
		// Output: A list of atoms (global variable)
		public ForthParser(string SourceFileName)
		{
			// Initialize the list of processed files
			ProcessedFiles = new ArrayList();
			// Get the list of atoms in the current source file
			SourceAtoms = ProcessSourceFile(SourceFileName);
			// Process "load" directives
			ProcessLoadDirectives();
		}

		// ProcessSourceFile - Builds up the internal atom table
		// Input:  SourceFileName - the name of the source file to be processed
		// Output: A list of atoms
		private ArrayList ProcessSourceFile(string SourceFileName)
		{
			// Check whether we already visited this file
			if(ProcessedFiles.Contains(SourceFileName))
				throw new Exception(SourceFileName + " is referenced more than once in 'LOAD' statements.");

			ArrayList AtomList = new ArrayList();		// List to hold the atoms parsed from the file

			int LineNumber = 1;
			StreamReader source = new StreamReader(SourceFileName);
			string line = source.ReadLine();
			while(line != null)
			{
				ProcessSourceLine(line, SourceFileName, LineNumber++, AtomList);
				line = source.ReadLine();
			}
			source.Close();

			// Add the file to the processed file list
			ProcessedFiles.Add(SourceFileName);

			return AtomList;
		}

		// ProcessSourceLine - Parses a line from the source file
		// Input:  SourceLine - the line to be processed
		//		   SourceFileName - the name of the currently processed file
		//		   LineNumber - current line number
		//		   AtomList - the atom list
		// Output: None
		private void ProcessSourceLine(string SourceLine, string SourceFileName, int LineNumber, ArrayList AtomList)
		{
			string atom;

			// The regular expression matches display strings (." "), dump strings (" ") and individual atoms
			Regex reg = new Regex("\\.\"[^\"]*\"|\"[^\"]*\"|\\S+");
			Match match = reg.Match(SourceLine);
			while (match.Success)
			{
				atom = match.ToString();	// Get the current atom

				if(atom.StartsWith(@"\")) // Deal with the single line comment (if found, drop the line)
						return;
				if(atom.StartsWith("("))  // Begin multi-line comment
				{
					CommentMode = true;
					match = match.NextMatch();	// Advance to the next atom
					continue;
				}
				if(atom.EndsWith(")"))  // End multi-line comment
				{
						CommentMode = false;
						match = match.NextMatch();	// Advance to the next atom
						continue;
				}

				if (!CommentMode) AtomList.Add(new ForthAtom(atom, SourceFileName, LineNumber));
				match = match.NextMatch();
			}
		}

		// ProcessLoadDirectives - Parses the atom list and recursively processes the LOAD directives
		// Input:  None
		// Output: Modifies the 'SourceAtoms' global variable
		private void ProcessLoadDirectives()
		{
			string IncludeFile;		// Name of the file to parse
			int LoadPosition;		// Position of the "LOAD" directive in the array

			while(true)
			{
				LoadPosition = -1;	// Initially not found
				// Get the position of the 'LOAD' directive in the atom list
				for(int i = 0; i < SourceAtoms.Count; i++)
				{
					ForthAtom atom = (ForthAtom)SourceAtoms[i];
					if(atom.Name.ToLower() == "load")
					{
						LoadPosition = i;
						break;
					}
				}
				if(LoadPosition == -1) break;	// No need to continue (not found)

				try 
				{
					IncludeFile = ((ForthAtom)SourceAtoms[LoadPosition + 1]).Name;			// Get the name of the file to include
				}
				catch(Exception)
				{
					ForthAtom atom = (ForthAtom)SourceAtoms[LoadPosition];
					throw new Exception("Missing name of the include file. (" + atom.FileName + "," + atom.LineNumber + ")" );	// Signal error if no file name is supplied
				}
				
				// Merge the atoms for the two files
				SourceAtoms.RemoveRange(LoadPosition, 2);
				ArrayList NewAtoms = ProcessSourceFile(IncludeFile);
				SourceAtoms.InsertRange(LoadPosition, NewAtoms);
			}
		}
	}
}
