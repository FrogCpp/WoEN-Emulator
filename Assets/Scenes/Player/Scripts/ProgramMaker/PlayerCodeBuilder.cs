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
    private ConsoleController _console;


    public PlayerCodeBuilder(ConsoleController c)
    {
        _console = c;
    }

    public bool CompileProject(string projectPath, Robot targetRobot, out Events methods)
    {
        methods = new Events();
        try
        {
            var compiler = new ProjectCompiler(projectPath, _console);

            var syntaxTrees = new List<SyntaxTree>();
            foreach (var file in compiler.SourceFiles)
            {
                try
                {
                    string code = File.ReadAllText(file);
                    var syntaxTree = CSharpSyntaxTree.ParseText(code, path: file);
                    syntaxTrees.Add(syntaxTree);
                    _console.msg($"✅ Загружен файл: {Path.GetFileName(file)}");
                }
                catch (Exception e)
                {
                    _console.Error($"❌ Ошибка загрузки файла {file}: {e}");
                    return false;
                }
            }

            if (syntaxTrees.Count == 0)
            {
                _console.Error("❌ В проекте не найдено .cs файлов");
                return false;
            }

            var references = GetReferences();
            var compilation = CSharpCompilation.Create(
                "UserAssembly",
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var errors = result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => $"{d.Location.GetLineSpan()} - {d.GetMessage()}");

                _console.Error("Ошибки компиляции:\n" + string.Join("\n", errors));
                return false;
            }

            ms.Seek(0, SeekOrigin.Begin);
            _userAssembly = Assembly.Load(ms.ToArray());

            _console.Log($"✅ Проект скомпилирован! Сборка: {_userAssembly.FullName}");

            return InitializeUserCode(targetRobot, out methods);
        }
        catch (Exception e)
        {
            _console.Error($"Ошибка компиляции проекта: {e}");
            methods = new Events();
            return false;
        }
    }





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

                _console.Error("Compilation failed:\n" + string.Join("\n", errors));
                return false;
            }

            ms.Seek(0, System.IO.SeekOrigin.Begin);
            _userAssembly = Assembly.Load(ms.ToArray());

            bool outp =  InitializeUserCode(targetRobot, out methods);
            return outp;
        }
        catch (Exception e)
        {
            _console.Error($"Compilation error: {e}");
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
            _console.msg("UnityEngine.UIElements not found - skipping");
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
                _console.Error("Class 'main' not found in user code");
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
            _console.Error($"Initialization error: {e}");
            return false;
        }
    }
}

public class ProjectCompiler
{
    public string ProjectPath { get; private set; }
    public List<string> SourceFiles { get; private set; }
    private ConsoleController _console;

    public ProjectCompiler(string projectPath, ConsoleController c)
    {
        ProjectPath = projectPath;
        SourceFiles = new List<string>();

        _console = c;

        try
        {
            var csFiles = Directory.GetFiles(ProjectPath, "*.cs", SearchOption.AllDirectories);
            SourceFiles.AddRange(csFiles);

            foreach (var file in SourceFiles)
            {
            }
        }
        catch (Exception e)
        {
            _console.Error($"Ошибка поиска файлов: {e}");
        }
    }
}