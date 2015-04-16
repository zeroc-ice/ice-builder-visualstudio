using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ZeroC.IceVisualStudio
{
    public class ErrorProvider
    {
        public List<ErrorTask> Errors
        {
            get;
            private set;
        }

        public ErrorListProvider ErrorListProvider
        {
            get;
            private set;
        }

        public ErrorProvider(IServiceProvider serviceProvider)
        {
            Errors = new List<ErrorTask>();
            ErrorListProvider = new Microsoft.VisualStudio.Shell.ErrorListProvider(serviceProvider);
            ErrorListProvider.ProviderName = "Ice Builder Error Provider";
            ErrorListProvider.ProviderGuid = new Guid("B8DA84E8-7AE3-4c71-8E43-F273A20D40D1");
            ErrorListProvider.Show();
        }

        public void Add(IVsHierarchy hierarchy,
                        string file,
                        TaskErrorCategory category,
                        int line, int column, string text)
        {
            ErrorTask task = new ErrorTask();
            task.ErrorCategory = category;
            task.Line = line - 1;
            task.Column = column - 1;
            if (hierarchy != null)
            {
                task.HierarchyItem = hierarchy;
            }
            //task.Navigate += new EventHandler(errorTaskNavigate);
            task.Document = file;
            task.Category = TaskCategory.BuildCompile;
            task.Text = text;

            ErrorListProvider.Tasks.Add(task);
        }

    }
}
