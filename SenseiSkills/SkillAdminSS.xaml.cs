using Buddy.BladeAndSoul.Game;
using log4net;
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



            String profileData = Helper.readFromFile();

            SenseiProfile profile = ((JObject)JsonConvert.DeserializeObject(profileData)).ToObject<SenseiProfile>();

            Log.Info("Loaded " + profile.skillList.Count + " Skill Definition");

            skillList = profile.skillList;
            rangedClass.IsChecked = profile.rangedClass;
            minCast.Text = profile.minCastTime.ToString();


            TreeViewItem root = new TreeViewItem();
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

            TreeViewItem evadeSkills = new TreeViewItem();
            evadeSkills.Header = "EVADE";
            evadeSkills.IsExpanded = true;


            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.EVADE)).ToList())
            {
                SkillInfoTree skillTr = addLeaf(skill);
                evadeSkills.Items.Add(skillTr);

            }

            root.Items.Add(gapClose);
            root.Items.Add(ccBreak);
            root.Items.Add(dps);
            root.Items.Add(defaulSkills);
            root.Items.Add(evadeSkills);

            skillTree.Items.Add(root);


        }

        List<SkillInfo> skillList = new List<SkillInfo>();
        private static ILog Log = LogManager.GetLogger("SenseiSkills");

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {


            skillList = new List<SkillInfo>();


            TreeViewItem root = (TreeViewItem)skillTree.Items.GetItemAt(0);



            TreeViewItem gapClose = (TreeViewItem)root.Items.GetItemAt(0);

            foreach (SkillInfoTree itm in gapClose.Items)
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


            TreeViewItem evadeSkills = (TreeViewItem)root.Items.GetItemAt(4); ;



            foreach (SkillInfoTree itm in evadeSkills.Items)
            {
                SkillInfo skill = addSkillCombo(itm);
                skillList.Add(skill);

            }



            Log.Info("Saving " + skillList.Count + " Skill Definition");

            SenseiProfile profile = new SenseiProfile();
            profile.minCastTime = int.Parse(minCast.Text);
            profile.rangedClass = (bool)rangedClass.IsChecked;
            profile.skillList = skillList;


            String profileData = JsonConvert.SerializeObject(profile, Formatting.Indented);
            Helper.writeToFile(profileData);

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

            newSkill.ignoreSkillError = (bool)sklignore.IsChecked;

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
                case "EVADE":
                    return SkillType.EVADE;
                case "CC":
                    return SkillType.CC;
                case "HEAL":
                    return SkillType.HEAL;
                case "DEFAULT":
                    return SkillType.DEFAULT;

            }

            return SkillType.DEFAULT;
        }



        private void sklkey_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Log.InfoFormat("Moving UP");
            TreeViewItem currentNode = (TreeViewItem)skillTree.SelectedItem;

            ItemsControl parent = GetSelectedTreeViewItemParent(currentNode);

            TreeViewItem treeitem = parent as TreeViewItem;



            int currentPos = treeitem.Items.IndexOf(currentNode);

            Log.InfoFormat("Parent {0}, Node To Move {1} from Pos {2} to Pos {3}", treeitem.Header.ToString(), currentNode.Header.ToString(), currentPos, currentPos - 1);

            treeitem.Items.Remove(currentNode);
            treeitem.Items.Insert(currentPos - 1, currentNode);



        }

        public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is System.Windows.Controls.TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Log.InfoFormat("Moving UP");
            TreeViewItem currentNode = (TreeViewItem)skillTree.SelectedItem;

            ItemsControl parent = GetSelectedTreeViewItemParent(currentNode);

            TreeViewItem treeitem = parent as TreeViewItem;

            int currentPos = treeitem.Items.IndexOf(currentNode);
            Log.InfoFormat("Parent {0}, Node To Move {1} from Pos {2} to Pos {3}", treeitem.Header.ToString(), currentNode.Header.ToString(), currentPos, currentPos + 1);

            treeitem.Items.Remove(currentNode);
            treeitem.Items.Insert(currentPos + 1, currentNode);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Log.InfoFormat("Remove");
            TreeViewItem currentNode = (TreeViewItem)skillTree.SelectedItem;

            ItemsControl parent = GetSelectedTreeViewItemParent(currentNode);

            TreeViewItem treeitem = parent as TreeViewItem;

            Log.InfoFormat("Parent {0}, Node To Remove {1} ", treeitem.Header.ToString(), currentNode.Header.ToString());

            treeitem.Items.Remove(currentNode);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            dumpSkillsnActions();
        }


        public void dumpSkillsnActions()
        {

            Log.Info("Actions=====");
            List<Buddy.BladeAndSoul.Game.Action> actions = GameManager.LocalPlayer.CurrentActions.ToList();

            foreach (Buddy.BladeAndSoul.Game.Action action in actions)
            {
                Log.InfoFormat("Action {0}, Duration {1}", action.SkillName, action.Duration);

            }


            Log.Info("Skills=====");
            List<Buddy.BladeAndSoul.Game.Skill> skills = GameManager.LocalPlayer.CurrentSkills.ToList();

            foreach (Buddy.BladeAndSoul.Game.Skill skill in skills)
            {
                Log.InfoFormat("Skill {0}, Key {1}",skill.Name,skill.ShortcutKeyClassic);
            }
            Log.Info("Done=====");
        }


    }
}
