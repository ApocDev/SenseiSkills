using SenseiSkills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TestBuddy
{
    public class SkillInfoTree : TreeViewItem
    {
        #region Data Member

       
        public TextBlock _textBlock = null;
        public  SkillInfo skill; 

        #endregion

        #region Properties

        public SkillInfo Skill
        {
            get { return skill; }
            set { skill = value; }
        }

        public string Text
        {
            get { return _textBlock.Text; }
            set { _textBlock.Text = value; }
        }

        #endregion

        #region Constructor

        public SkillInfoTree()
        {
            CreateSkillInfoTreeTemplate();
        }

        #endregion

        #region Private Methods
         public  void CreateSkillInfoTreeTemplate()
         {
             StackPanel stack = new StackPanel();
             stack.Orientation = Orientation.Horizontal;



             //create stack pane;
             FrameworkElementFactory stackPanel = new FrameworkElementFactory(typeof(StackPanel));
             stackPanel.Name = "parentStackpanel";
             stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);


              // Create check box
               FrameworkElementFactory checkBox = new FrameworkElementFactory(typeof(CheckBox));
               checkBox.Name = "chk";
               checkBox.SetValue(CheckBox.NameProperty, "chk");
               checkBox.SetValue(CheckBox.TagProperty , new Binding());
               checkBox.SetValue(CheckBox.MarginProperty, new Thickness(2));
               stackPanel.AppendChild(checkBox);
            
                   // create text
           FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
           label.SetBinding(TextBlock.TextProperty, new Binding());
           label.SetValue(TextBlock.ToolTipProperty, new Binding());
           stackPanel.AppendChild(label);



             Header = stack;
         }
        

        #endregion
    }
}
