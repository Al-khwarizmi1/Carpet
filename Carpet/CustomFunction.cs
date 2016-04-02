using ICSharpCode.AvalonEdit.CodeCompletion;
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

    public class CustomFunction
    {
        public CustomFunctionParameter Parameter { get; }
        public string FunctioName { get; }
        public string FunctionBody { get; }

        public string FunctionHeader { get; }
        public string FunctionFooter { get; }

        public string Function => FunctionHeader + FunctionBody + FunctionFooter;


        public CustomFunction(string functioName, CustomFunctionParameter parameter, string functionBody)
        {
            FunctioName = functioName;
            FunctionBody = functionBody;
            Parameter = parameter;

            FunctionHeader = $@"
string {FunctioName}({Parameter.Type.Name} {Parameter.Name})
{{
";

            FunctionFooter = @"
}
";
        }
    }
}
