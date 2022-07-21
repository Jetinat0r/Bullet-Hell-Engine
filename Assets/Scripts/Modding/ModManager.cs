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

		//references.AddRange(GenerateInitialMetadataReferences());
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

	public void LoadMods()
    {
		if (!Directory.Exists(ModPath))
		{
			Directory.CreateDirectory(ModPath);
		}

		List<string> modFilePaths = new List<string>(Directory.GetFiles(ModPath));
		foreach (string filePath in modFilePaths)
		{
			if (filePath.EndsWith(".dll"))
			{
				ImportDllFile(filePath);
			}

			if (filePath.EndsWith(".cs"))
			{
				//ReadCSFile(filePath);
			}
		}
	}

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

				Mod newMod = TryReflection(assembly);
				AttemptAddMod(newMod);
			}
		}
	}

	public static Mod TryReflection(Assembly assembly)
    {
		foreach(Type t in assembly.GetTypes())
        {
			if(t.BaseType == typeof(Mod))
            {
				object obj = Activator.CreateInstance(t);
				t.InvokeMember("OnLoad",
					BindingFlags.Default | BindingFlags.InvokeMethod,
					null,
					obj,
					new object[] { });

				return (Mod)obj;
			}
        }

		return null;
	}

	public void ImportDllFile(string filePath)
    {
		Assembly assembly = Assembly.LoadFrom(filePath);

		references.Add(MetadataReference.CreateFromFile(filePath));

		Mod newMod = TryReflection(assembly);
		AttemptAddMod(newMod);
	}

	public void AttemptAddMod(Mod newMod)
    {
		if (newMod != null)
		{
			LoadedMods.Add(newMod);
		}
	}
}