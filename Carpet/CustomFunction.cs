using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using System;
using System.Collections.Generic;

namespace Carpet
{
    public class CustomFunctionParameter
    {
        public Type Type { get; }
        public string Name { get; }
        public IList<ICompletionData> CompletionData { get; }

        public CustomFunctionParameter(Type type, string name)
        {
            CompletionData = new List<ICompletionData>();

            Type = type;
            Name = name;

            LoadCompletionData();
        }

        private void LoadCompletionData()
        {
            foreach (var field in Type.GetFields())
            {
                CompletionData.Add(new AutoCompletionData(field.Name));
            }
        }
    }

    public static class PredefinedCustomFunctionParameter
    {
        public static CustomFunctionParameter File = new CustomFunctionParameter(typeof(CarpetFileInfo), "file");
        public static CustomFunctionParameter Dir = new CustomFunctionParameter(typeof(CarpetDirectoryInfo), "dir");
    }

    public class GlobalParam<T>
    {
        public T g;
    }

    public class CustomFunction<T>
    {
        public CustomFunctionParameter Parameter { get; }
        public string FunctioName { get; }
        public string FunctionBody { get; set; }

        public string FunctionHeader { get; }
        public string FunctionFooter { get; }

        public string Function => FunctionHeader + FunctionBody + FunctionFooter;

        private ScriptRunner<object> FunctionDelegate { get; set; }

        public string Invoke(T parameter)
        {
            if (FunctionDelegate == null)
            {
                FunctionDelegate = CSharpScript.Create<string>(Function, ScriptOptions.Default, typeof(GlobalParam<T>)).ContinueWith($"{FunctioName}(g)")
                    .CreateDelegate();
            }
            return (string)FunctionDelegate.Invoke(new GlobalParam<T> { g = parameter }).Result;
        }

        public bool Test()
        {
            CSharpScript.Create<string>(Function, ScriptOptions.Default.WithReferences(typeof(Carpet.CarpetWatchInfo).Assembly)
                .WithReferences(typeof(Carpet.CarpetDirectoryInfo).Assembly), typeof(T)).CreateDelegate();
            return true;
        }

        public CustomFunction(string functioName, CustomFunctionParameter parameter, string functionBody)
        {
            FunctioName = functioName;
            FunctionBody = functionBody;
            Parameter = parameter;

            FunctionHeader = $@"
string {FunctioName}(Carpet.{Parameter.Type.Name} {Parameter.Name})
{{
";

            FunctionFooter = @"
}
";
        }
    }
}
