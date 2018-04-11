using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vespertan.VisualStudio.Extensions.TemplateEditor
{
    public class WizardInfoWrapper
    {
        private static Type WizardInfoType => Type.GetType("Vespertan.VisualStudio.Template.Common.WizardInfo, Vespertan.VisualStudio.Template.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad2b617f2d3eb0d1");
        public static string Name => (string)WizardInfoType?.GetProperty(nameof(Name))?.GetValue(null);
        public static Dictionary<string, string> InputReplacementDictionary => (Dictionary<string, string>)WizardInfoType?.GetProperty(nameof(InputReplacementDictionary))?.GetValue(null);
        public static Dictionary<string, string> EvaluatedReplacementDictionary => (Dictionary<string, string>)WizardInfoType?.GetProperty(nameof(EvaluatedReplacementDictionary))?.GetValue(null);
        public static Dictionary<string, object> Data => (Dictionary<string, object>)WizardInfoType?.GetProperty(nameof(Data))?.GetValue(null);
        public static List<string> Log => (List<string>)WizardInfoType?.GetProperty(nameof(Log))?.GetValue(null);
    }
}
