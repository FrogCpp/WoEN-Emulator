using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public struct Events
{
    public object userInstance;
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

            bool outp =  InitializeUserCode(targetRobot, out methods);
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
        references.Add(MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location));

        references.Add(MetadataReference.CreateFromFile(typeof(UnityEngine.Object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(UnityEngine.Debug).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(UnityEngine.GameObject).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(UnityEngine.Mathf).Assembly.Location));

        try
        {
            var netstandardPath = Assembly.Load("netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51").Location;
            references.Add(MetadataReference.CreateFromFile(netstandardPath));
        }
        catch
        {
            var netstandardPath = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "netstandard.dll");
            if (File.Exists(netstandardPath))
            {
                references.Add(MetadataReference.CreateFromFile(netstandardPath));
            }
        }

        try
        {
            var uiElementsAssembly = Assembly.Load("UnityEngine.UIElementsModule");
            references.Add(MetadataReference.CreateFromFile(uiElementsAssembly.Location));
        }
        catch
        {
            Debug.LogWarning("UnityEngine.UIElements not found - skipping");
        }

        references.Add(MetadataReference.CreateFromFile(typeof(Robot).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(RobotHardware).Assembly.Location));

        return references;
    }

    private bool InitializeUserCode(Robot targetRobot, out Events m)
    {
        m = new Events();
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
            m.userInstance = _userInstance;

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Initialization error: {e}");
            return false;
        }
    }
}