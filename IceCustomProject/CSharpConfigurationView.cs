using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio;

namespace IceCustomProject
{
    public partial class CSharpConfigurationView : UserControl
    {
        public CSharpConfigurationView()
        {
            InitializeComponent();
        }

        public virtual void Initialize(Control parent, Rectangle rect)
        {
            base.SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
            base.Parent = parent;
        }

        public int ProcessAccelerator(ref Message keyboardMessage)
        {
            if (Control.FromHandle(keyboardMessage.HWnd).PreProcessMessage(ref keyboardMessage))
            {
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }
    }
}
