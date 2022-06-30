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

public class LoadModTest : MonoBehaviour
{
    public string ModPath;

    private void Awake()
    {
        ModPath = Application.persistentDataPath + "/Mods";
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
            if (filePath.EndsWith(".cs"))
            {
                ReadCSFile(filePath);
            }
        }
    }

    public static void ReadCSFile(string filePath)
    {
        Debug.Log($"Reading from: {filePath}");
		
		MetadataReference[] references = new MetadataReference[]
		{
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Enum).Assembly.Location),

			MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),

			MetadataReference.CreateFromFile(typeof(Color).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(ColliderType).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Debug).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(CachedBHEResources).Assembly.Location),

		};

		string fileContent = File.ReadAllText(filePath);

		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);

		CSharpCompilation newCompiledScript = CSharpCompilation.Create("Mod_Test_Compiled.cs", syntaxTrees: new[] { syntaxTree }, references: references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using (var ms = new MemoryStream())
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
}

//public class CSCompiler : MonoBehaviour
//{
//	void Start()
//	{
//		var assembly = Compile(@"
//		using UnityEngine;

//		public class Test
//		{
//			public static void Foo()
//			{
//				Debug.Log(""Hello, World!"");
//			}
//		}");

//		var method = assembly.GetType("Test").GetMethod("Foo");
//		var del = (Action)Delegate.CreateDelegate(typeof(Action), method);
//		del.Invoke();
//	}

//	public static Assembly Compile(string source)
//	{
//		CSharpCodeProvider provider = new CSharpCodeProvider();
//		CompilerParameters param = new CompilerParameters();

//		// Add ALL of the assembly references
//		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
//		{
//			param.ReferencedAssemblies.Add(assembly.Location);
//		}

//		// Add specific assembly references
//		//param.ReferencedAssemblies.Add("System.dll");
//		//param.ReferencedAssemblies.Add("CSharp.dll");
//		//param.ReferencedAssemblies.Add("UnityEngines.dll");

//		// Generate a dll in memory
//		param.GenerateExecutable = false;
//		param.GenerateInMemory = true;

//		// Compile the source
//		CompilerResults result = provider.CompileAssemblyFromSource(param, source);

//		if (result.Errors.Count > 0)
//		{
//			var msg = new StringBuilder();
//			foreach (CompilerError error in result.Errors)
//			{
//				msg.AppendFormat("Error ({0}): {1}\n",
//					error.ErrorNumber, error.ErrorText);
//			}
//			throw new Exception(msg.ToString());
//		}

//		// Return the assembly
//		return result.CompiledAssembly;
//	}
//}
