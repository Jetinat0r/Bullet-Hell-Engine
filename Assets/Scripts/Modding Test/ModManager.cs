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
    public string ModPath;
	public List<MetadataReference> references = new List<MetadataReference>();

    private void Awake()
    {
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

		Debug.Log(references.ToString());
    }

    // Start is called before the first frame update
    public void Start()
    {
		//Debug.Log(Application.persistentDataPath);

        if (!Directory.Exists(ModPath))
        {
            Directory.CreateDirectory(ModPath);
        }

        List<string> modFilePaths = new List<string>(Directory.GetFiles(ModPath));
        foreach (string filePath in modFilePaths)
        {
			/*
            if (filePath.EndsWith(".cs"))
            {
                ReadCSFile(filePath);
            }
			*/

            if (filePath.EndsWith(".dll"))
            {
				ImportDllFile(filePath);
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
				TryReflection(assembly);
			}
		}
		//Assembly assembly = new();

		//Debug.LogWarning(assembly.GetTypes());

		//MethodInfo method = assembly.GetType("Test_Mod").GetMethod("Test");
		//Action del = (Action)Delegate.CreateDelegate(typeof(Action), method);
		//del.Invoke();


		//Debug.Log(fileContent);
	}

	public static void TryReflection(Assembly assembly)
    {
		Type type = assembly.GetType("Test_Mod");
		object obj = Activator.CreateInstance(type);
		type.InvokeMember("Test",
			BindingFlags.Default | BindingFlags.InvokeMethod,
			null,
			obj,
			new object[] { });
	}

	public static void ImportDllFile(string filePath)
    {
		Assembly assembly = Assembly.LoadFrom(filePath);

		Type type = assembly.GetType("BHE_Example_Mod.ExampleMod");
		object obj = Activator.CreateInstance(type);
		type.InvokeMember("Test",
			BindingFlags.Default | BindingFlags.InvokeMethod,
			null,
			obj,
			new object[] { });
	}
}