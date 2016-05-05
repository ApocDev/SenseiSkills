﻿using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SenseiSkills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestBuddy
{
    /// <summary>
    /// Interaction logic for SkillAdmin.xaml
    /// </summary>
    public partial class SkillAdminSS : System.Windows.Controls.UserControl
    {
        public SkillAdminSS()
        {
            Log.Info("Init SASS");

            InitializeComponent();

           

            String skillData = Helper.readFromFile();
            skillList = ((JArray)JsonConvert.DeserializeObject(skillData)).ToObject< List < SkillInfo >  >();


            Log.Info("Loaded " + skillList.Count + " Skill Definition");



            TreeViewItem root  = new TreeViewItem();
            root.Header = "Skill List";
            root.IsExpanded = true;


            TreeViewItem gapClose = new TreeViewItem();
            gapClose.Header = "GAPCLOSER";
            gapClose.IsExpanded = true;

            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.GAPCLOSER)).ToList())
            {
                SkillInfoTree skillTr = addLeaf(skill);
                gapClose.Items.Add(skillTr);

            }


            TreeViewItem ccBreak = new TreeViewItem();
            ccBreak.Header = "CCBREAK";
            ccBreak.IsExpanded = true;
            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.CCBREAK)).ToList())
            {
                SkillInfoTree skillTr = addLeaf(skill);
                ccBreak.Items.Add(skillTr);

            }
          

            TreeViewItem dps = new TreeViewItem();
            dps.Header = "DPS";
            dps.IsExpanded = true;

            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.DPS)).ToList())
            {
                SkillInfoTree skillTr = addLeaf(skill);
                dps.Items.Add(skillTr);

            }

            TreeViewItem defaulSkills = new TreeViewItem();
            defaulSkills.Header = "DEFAULT";
            defaulSkills.IsExpanded = true;


            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.DEFAULT)).ToList())
            {
                SkillInfoTree skillTr = addLeaf(skill);
                defaulSkills.Items.Add(skillTr);

            }

            root.Items.Add(gapClose);
            root.Items.Add(ccBreak);
            root.Items.Add(dps);
            root.Items.Add(defaulSkills);
            
            skillTree.Items.Add(root);


        }

        List<SkillInfo> skillList = new List<SkillInfo>();
        private static ILog Log = LogManager.GetLogger("SenseiSkills");

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {


            skillList = new List<SkillInfo>();


           TreeViewItem root= (TreeViewItem) skillTree.Items.GetItemAt(0);



           TreeViewItem gapClose = (TreeViewItem)root.Items.GetItemAt(0);
        
            foreach(SkillInfoTree itm in gapClose.Items)
            {
                SkillInfo skill = addSkillCombo(itm);
                skillList.Add(skill);

            }

           TreeViewItem ccBreak = (TreeViewItem)root.Items.GetItemAt(1);

           foreach (SkillInfoTree itm in ccBreak.Items)
           {
               SkillInfo skill = addSkillCombo(itm);
               skillList.Add(skill);

           }

           TreeViewItem dps = (TreeViewItem)root.Items.GetItemAt(2);


           foreach (SkillInfoTree itm in dps.Items)
           {
               SkillInfo skill = addSkillCombo(itm);
               skillList.Add(skill);

           }
          

           TreeViewItem defaulSkills = (TreeViewItem)root.Items.GetItemAt(3); ;



           foreach (SkillInfoTree itm in defaulSkills.Items)
           {
               SkillInfo skill = addSkillCombo(itm);
               skillList.Add(skill);

           }
          





            Log.Info("Saving " + skillList.Count + " Skill Definition");
            String skillData = JsonConvert.SerializeObject(skillList);
            Helper.writeToFile(skillData);

        }


        public SkillInfo addSkillCombo(SkillInfoTree skill)
        {
            SkillInfo sk = skill.Skill;

            if (skill.Items.Count == 1)
            {
                SkillInfo cmb = addSkillCombo((SkillInfoTree)skill.Items.GetItemAt(0));
                cmb.type = sk.type;
                sk.chainSkill.Add(cmb);
            }

            return sk;

        
        }

        public SkillInfoTree addLeaf(SkillInfo skill) 
        {
            SkillInfoTree skillTr = new SkillInfoTree();
            skillTr.Header = skill.skillName;
            skillTr.Skill = skill;


            if (skill.chainSkill != null)
            {
                foreach (SkillInfo sk in skill.chainSkill)
                {
                    SkillInfoTree rtn = addLeaf(sk);
                    skillTr.Items.Add(rtn);
                }

            }
            return skillTr;
        
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            TreeViewItem currentNode = (TreeViewItem)skillTree.SelectedItem;

            SkillInfo newSkill = new SkillInfo();
            newSkill.skillName = sklname.Text;
           
            newSkill.ignoreSkillError =(bool) sklignore.IsChecked;

            if (skillTree.SelectedItem is SkillInfoTree)
            {
                //Should be as a ChainSkill
                newSkill.type = ((SkillInfoTree)skillTree.SelectedItem).skill.type;
                ((SkillInfoTree)skillTree.SelectedItem).skill.chainSkill.Add(newSkill);
            }
            else
            {
                newSkill.type = getSkillType(((TreeViewItem)skillTree.SelectedItem).Header.ToString());
            }

          

            SkillInfoTree skillTr = addLeaf(newSkill);
            skillTr.IsExpanded = true;

            currentNode.Items.Add(skillTr);
                
            
        }


        public SkillType getSkillType(String label)
        {
            switch (label)
            {
                case "GAPCLOSER":
                    return SkillType.GAPCLOSER;
                case "CCBREAK":
                    return SkillType.CCBREAK;
                case "DPS":
                    return SkillType.DPS;
                case "DEFAULT":
                    return SkillType.DEFAULT;

            }

            return SkillType.DEFAULT;
        }

       

        private void sklkey_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }



}
