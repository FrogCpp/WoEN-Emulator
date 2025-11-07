using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public struct Events
{
    public MethodInfo Start;
    public MethodInfo Update;
}

public class PlayerCodeBuilder
{
    private Assembly _userAssembly;
    private Type _mainType;
    private object _userInstance;
    private MethodInfo _startMethod;
    private MethodInfo _updateMethod;

    public bool CompileAndLoad(string code, Robot targetRobot, out Events methods)
    {
        methods = new Events();
        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = GetReferences();

            var compilation = CSharpCompilation.Create(
                "UserAssembly",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new System.IO.MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var errors = result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString());

                Debug.LogError("Compilation failed:\n" + string.Join("\n", errors));
                return false;
            }

            ms.Seek(0, System.IO.SeekOrigin.Begin);
            _userAssembly = Assembly.Load(ms.ToArray());

            bool outp =  InitializeUserCode(targetRobot, methods);
            return outp;
        }
        catch (Exception e)
        {
            Debug.LogError($"Compilation error: {e}");
            return false;
        }
    }

    private List<MetadataReference> GetReferences()
    {
        var references = new List<MetadataReference>();

        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Robot).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(RobotHardware).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Mathf).Assembly.Location));

        references.Add(MetadataReference.CreateFromFile(typeof(UnityEngine.Object).Assembly.Location));

        return references;
    }

    private bool InitializeUserCode(Robot targetRobot, Events m)
    {
        try
        {
            _mainType = _userAssembly.GetType("main");
            if (_mainType == null)
            {
                Debug.LogError("Class 'main' not found in user code");
                return false;
            }

            _userInstance = Activator.CreateInstance(_mainType);

            _startMethod = _mainType.GetMethod("Start");
            _updateMethod = _mainType.GetMethod("Update");

            var initMethod = _mainType.GetMethod("Init");
            initMethod?.Invoke(_userInstance, new object[] { targetRobot });

            m.Start = _startMethod;
            m.Update = _updateMethod;
            //_startMethod?.Invoke(_userInstance, null);
            //_updateMethod?.Invoke(_userInstance, null);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Initialization error: {e}");
            return false;
        }
    }
}