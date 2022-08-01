using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Scripting;
//using Microsoft.CodeAnalysis.Scripting;

using System;
//using System.Collections.Immutable;
//using System.CodeDom;
//using System.CodeDom.Compiler;
using System.Reflection;
//using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

public class ModManager : MonoBehaviour
{
	public static ModManager instance;

    public string ModPath;
	public List<MetadataReference> references = new List<MetadataReference>();

	public List<Mod> LoadedMods = new List<Mod>();

	//Converts the object into a singleton and adds references for the .cs files compiled at runtime
    private void Awake()
    {
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

		ModPath = Application.persistentDataPath + "/Mods";

		//Generates references for the .cs files compiled at runtime to use
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			try
			{
				references.Add(MetadataReference.CreateFromFile(assembly.Location));
			}
			catch
			{
				//Debug.LogError($"{assembly.ToString()}");
			}
		}
    }

	//Loads all of the mods located in the Mods folder at the PersistentDataPath
	//Loads them in the following order:
	public void LoadMods()
    {
		if (!Directory.Exists(ModPath))
		{
			Directory.CreateDirectory(ModPath);
		}

		//Recursively grabs the files of the desired types, choosing lower level files first in an attempt to find and load things like projectile files before the mods that rely on them
		List<string> preCompFilePaths = new List<string>();
		GetFilesRecursively(ModPath, ".dll", ref preCompFilePaths);
		List<string> runtimeCompFilePaths = new List<string>();
		GetFilesRecursively(ModPath, ".cs", ref runtimeCompFilePaths);

		foreach (string filePath in preCompFilePaths)
		{
			ImportDllFile(filePath);
		}

		foreach (string filePath in runtimeCompFilePaths)
		{
			ReadCSFile(filePath);
		}
	}

	//Recursively gets all files with the specified ending from the specified path and returns them via the inputted list
	//Directories and files are first sorted by name, so if that is important, name your files and folders accordingly
	//Files are grabbed "bottom up," meaning for a given directory, it searches all sub directories first and then works its
	//way up before moving on to the next directory
	public static void GetFilesRecursively(string curPath, string fileEnding, ref List<string> filePaths)
	{
		string[] directories = Directory.GetDirectories(curPath);
		SortPathsByName(ref directories);
		foreach (string newPath in directories)
		{
			GetFilesRecursively(newPath, fileEnding, ref filePaths);
		}

		string[] files = Directory.GetFiles(curPath);
		SortPathsByName(ref files);
		foreach (string file in files)
		{
			if (file.EndsWith(fileEnding))
			{
				//Debug.LogError($"Grabbing: {file}");
				filePaths.Add(file);
			}
		}
	}

	public static void SortPathsByName(ref string[] paths)
    {
		Array.Sort(paths);
		//foreach (var file in paths)
        //{
			//Debug.LogError($"Sorted: {file}");
        //}
    }

	//Loads a .cs file and compiles it using all current assemblies, and then, if compilation was successful, attempts to convert the file into a mod and then add it to the LoadedMods list
	//If the file does not contain a Mod, it will still compile, but will not be added to LoadedMods and instead will just sit in the list of assemblies
	//Note: The resulting assembly has IsFullyTrusted = true, so it can be REALLY dangerous
	public void ReadCSFile(string filePath)
    {
		Debug.Log($"Reading from: {filePath}");

		string fileContent = File.ReadAllText(filePath);

		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);

		CSharpCompilation newCompiledScript = CSharpCompilation.Create("TestProjEffect.cs", syntaxTrees: new[] { syntaxTree }, references: references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using (MemoryStream ms = new MemoryStream())
		{
			EmitResult result = newCompiledScript.Emit(ms);

			if (!result.Success)
			{
				IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
					diagnostic.IsWarningAsError ||
					diagnostic.Severity == DiagnosticSeverity.Error);

				foreach (Diagnostic diagnostic in failures)
				{
					Debug.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
				}
			}
			else
			{
				ms.Seek(0, SeekOrigin.Begin);
				Assembly assembly = Assembly.Load(ms.ToArray());
				references.Add(MetadataReference.CreateFromStream(ms));

				Mod newMod = ConvertToMod(assembly);
				AttemptAddMod(newMod);
			}
		}
	}

	//Converts the given assembly to a Mod type object if it can.
	//If the assembly is not a mod, returns null
	public static Mod ConvertToMod(Assembly assembly)
    {
		foreach(Type t in assembly.GetTypes())
        {
			if(t.BaseType == typeof(Mod))
            {
				object obj = Activator.CreateInstance(t);

				return (Mod)obj;
			}
        }

		//Debug.LogError($"Given assembly ({assembly}) does not contain a Mod!");
		return null;
	}

	//Imports the dll file as an assembly, adds a metadata reference to it, and, if it is a mod, calls its OnLoad() and adds it to the LoadedMods list
	public void ImportDllFile(string filePath)
    {
		Assembly assembly = Assembly.LoadFrom(filePath);

		references.Add(MetadataReference.CreateFromFile(filePath));

		Mod newMod = ConvertToMod(assembly);
		AttemptAddMod(newMod);
	}

	//Attempts to add the given Mod to the LoadedMods list and calls its OnLoad method
	//If the mod is null or its name is already taken, this will neither call the OnLoad nor add the mod to the list
	public void AttemptAddMod(Mod newMod)
    {
		if (newMod != null)
		{
			if(!DoesModExist(newMod))
            {
				newMod.OnLoad();
				LoadedMods.Add(newMod);
            }
            else
            {
				Debug.LogError($"Mod with name {newMod.ModName} already exists!");
            }
			
		}
	}

	//Returns whether or not a mod with the same name as the one passed in exists in the LoadedMods list
	public bool DoesModExist(Mod mod)
    {
		return LoadedMods.Any(p => p.ModName == mod.ModName);
	}

	//Returns whether or not a mod with the given ModName exists
	public bool DoesModExist(string modName)
	{
		return LoadedMods.Any(p => p.ModName == modName);
	}
}