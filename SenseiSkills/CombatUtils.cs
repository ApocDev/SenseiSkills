using Buddy.BladeAndSoul.Game;
using Buddy.BladeAndSoul.Game.Objects;
using Buddy.Engine;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseiSkills
{
    static class CombatUtils
    {
        private static ILog Log = LogManager.GetLogger("[SenseiSkills]");

        public static List<Effect> getNpcEffects(Actor target)
        {
            try
            {
                List<Effect> targetEffects  = target.Effects.ToList();

                Log.Info("Target Effect count: " + targetEffects.Count);

                foreach (Effect effect in targetEffects.ToList())
                {
                    //do stuff with the debuffs
                    Log.InfoFormat("Target {0} Effect==> {1} Stack {2}", target.Name, effect.Name, effect.StackCount);
                    // Log.Info(effect.Dump());
                }

                return targetEffects;
            }
            catch (Exception ex)
            {

                Log.Error("Effect Issue = " + ex.Message);
                return new List<Effect>();

            }
          
        }

        public static bool hasEffect(Actor target,String effect)
        {
            try
            {
                List<Effect> targetEffects = target.Effects.ToList();

                if (targetEffects.Where(e => e.Name.Equals(effect)).Count() > 0)
                {
                    return true;
                }
                else { return false; }

                
            }
            catch (Exception ex)
            {

                Log.Error("Effect Issue = " + ex.Message);
                return false;

            }
        }


        public static Effect getEffect(Actor target, String effect)
        {
            try
            {
                List<Effect> targetEffects = target.Effects.ToList();

                if (targetEffects.Where(e => e.Name.Equals(effect)).Count() > 0)
                {
                    return targetEffects.Where(e => e.Name.Equals(effect)).First();
                }
                else { return null; }


            }
            catch (Exception ex)
            {

                Log.Error("Effect Issue = " + ex.Message);
                return null;

            }
        }



        

        public static bool effectInList(Actor target, List<String> effects)
        {



          

            try
            {
                List<Effect> targetEffects = target.Effects.ToList();

                if (targetEffects.Where(e => { return effects.Contains(e.Name); }).Count() > 0)
                {
                    return true;
                }
                else { return false; }


            }
            catch (Exception ex)
            {

                Log.Error("Effect Issue = " + ex.Message);
                return false;

            }
        }



        public static Actor closestEnemy(bool agro)
        {
            GameEngine.AttachedProcess.Memory.ClearCache();
            using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
            {
                //Log.Info("Getting Actors");
                IEnumerable<Actor> actors = GameManager.Actors.Where(e =>e.IsValid && (e.CurrentTargetId == GameManager.LocalPlayer.Id|| (GameManager.SummonedMinion.IsValid && e.CurrentTargetId == GameManager.SummonedMinion.Id))  && e.IsHostile);
                
                actors = actors.OrderBy(e => e.Distance);
                //Log.Info("Sorted Actors by Distance");
                if (actors.Count() > 0)
                {
                    Log.InfoFormat("Got actors {0}", actors.Count());
                    return actors.First();
                }
            }

            return null;

        }


        public static bool isBlockable(Actor target)
        {

            GameEngine.AttachedProcess.Memory.ClearCache();
            using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
            {
                if (target.CurrentTargetId == GameManager.LocalPlayer.Id)
                {
                    //Log.Info("OMG is targetting me ");


                    if (target.IsCasting)
                    {
                        //Log.Info("OMG is casting something...");

                        try
                        {
                            var myAction = GameManager.LocalPlayer.CurrentActions.First();
                            //Log.Info("Player Action ++" + myAction.Dump());

                            var targetAction = GameManager.LocalPlayer.CurrentTarget.CurrentActions.First();
                           // Log.Info("Target Action --" + targetAction.Dump());


                            if (targetAction.TimeLeft < myAction.TimeLeft)
                            {
                                Log.InfoFormat("Should hold block as skill can be blocked with skill {0}",myAction.SkillName);
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Info("Problem getting current action");
                        }



                    }



                }
            }
            return false; //we are good for the time being.


        }

        public static bool willBlock(Actor target,String skillName)
        {

            GameEngine.AttachedProcess.Memory.ClearCache();
            using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
            {
                if (target.CurrentTargetId == GameManager.LocalPlayer.Id)
                {
                   

                    if (target.IsCasting)
                    {
                       

                        try
                        {
                           

                            var targetAction = GameManager.LocalPlayer.CurrentTarget.CurrentActions.First();
                            //Log.Info("Target Action " + targetAction.Dump());

                            var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
                            int channelTime = skill.Record.ExecDuration1;


                            Log.InfoFormat("Skil Channel Time  {0}>= than skill remaining time {1}", channelTime, targetAction.TimeLeft.Milliseconds);
                            if (channelTime >= targetAction.TimeLeft.Milliseconds )
                            {
                                Log.Info("Try to cast block in time");
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Info("Problem getting current action");
                        }



                    }



                }
            }
            return false; //we are good for the time being.


        }


        /*
        public static bool isDanger(Actor target)
        {

            GameEngine.AttachedProcess.Memory.ClearCache();
            using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
            {
                if (target.CurrentTargetId == GameManager.LocalPlayer.Id)
                {
                    //Log.Info("OMG is targetting me ");


                    if (target.IsCasting)
                    {
                       // Log.Info("OMG is casting something...");


                      

                        foreach (var action in GameManager.LocalPlayer.CurrentTarget.CurrentActions)
                        {
                            Log.Info("Target Action =>"+action.Dump());
                            if ( action.TimeLeft < TimeSpan.FromMilliseconds(250))
                            {
                                return true;
                            }
                        }

                        

                      
                    }



                }
            }
                return false; //we are good for the time being.
            

        }*/

        public static bool validateConditions(SkillInfo skill,Actor target,bool inBlock)
        {

            Log.DebugFormat("Checking Condition for skill {0}", skill.skillName);
            bool rtn = true;

            try
            {

                foreach (SkillCondition condition in skill.conditions)
                {

                    if(inBlock && skill.breakBlock == false)
                    {
                        Log.Debug("Player Blocking and skill doest break block");
                        return false;
                    }


                    if (condition.type == ConditionType.NONE)
                    {
                        //rtn=true;
                    }

                    if (condition.type == ConditionType.DONTUSE)
                    {
                        return false;
                    }

                    if (condition.type == ConditionType.DISTLT)
                    {
                        Log.DebugFormat("Dist to TG {0} <= {1}", target.Distance / 50, condition.conditionAmount);
                        if ((target.Distance / 50) <= condition.conditionAmount)
                        { }
                        else
                            return false;

                    }

                    if (condition.type == ConditionType.DISTGT)
                    {
                        Log.DebugFormat("Dist to GT {0} >= {1}", target.Distance / 50, condition.conditionAmount);
                        if ((target.Distance / 50) >= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.HPGT && !condition.conditionSelf)
                    {
                        Log.DebugFormat("Target HealtPCT to GT {0} >= {1}", target.HealthPercent, condition.conditionAmount);
                        if (target.HealthPercent >= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.HPLT && !condition.conditionSelf)
                    {
                        Log.DebugFormat("Target HealtPCT to LT {0} <= {1}", target.HealthPercent, condition.conditionAmount);
                        if (target.HealthPercent <= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.HPGT && condition.conditionSelf)
                    {
                        Log.DebugFormat("Self HealtPCT to GT {0} >= {1}", GameManager.LocalPlayer.HealthPercent, condition.conditionAmount);
                        if (GameManager.LocalPlayer.HealthPercent >= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.HPLT && condition.conditionSelf)
                    {
                        Log.DebugFormat("Self HealtPCT to LT {0} <= {1}", GameManager.LocalPlayer.HealthPercent, condition.conditionAmount);
                        if (GameManager.LocalPlayer.HealthPercent <= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.FOCUSGT )
                    {
                        Log.DebugFormat("Target Focus to LT {0} <= {1}", GameManager.LocalPlayer.Focus, condition.conditionAmount);
                        if (GameManager.LocalPlayer.Focus <= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.FOCUSLT )
                    {
                        Log.DebugFormat("Self Focus to GT {0} >= {1}", GameManager.LocalPlayer.Focus, condition.conditionAmount);
                        if (GameManager.LocalPlayer.Focus >= condition.conditionAmount)
                        { }
                        else return false;

                    }

                    if (condition.type == ConditionType.STACK && !condition.conditionSelf)
                    {
                        Effect ef = getEffect(target, condition.conditionName);
                        if (ef != null)
                        {
                            Log.DebugFormat("Effect {0} with stacks (tgt) {1} >= {2}", ef.Name, ef.StackCount, condition.stackCount);
                            if (ef.StackCount >= condition.stackCount)
                            {
                                // return true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                    }


                    if (condition.type == ConditionType.STACK && condition.conditionSelf)
                    {
                        Effect ef = getEffect(GameManager.LocalPlayer, condition.conditionName);
                        if (ef != null)
                        {
                            Log.DebugFormat("Effect {0} with stacks (self) {1} >= {2}", ef.Name, ef.StackCount, condition.stackCount);
                            if (ef.StackCount >= condition.stackCount)
                            {
                                //return true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                    }


                    if (condition.type == ConditionType.STANCE)
                    {
                        Log.DebugFormat("Player Stance {0} required Stance {1}", GameManager.LocalPlayer.Stance.ToString(), condition.conditionName);
                        if (condition.conditionName.Equals(GameManager.LocalPlayer.Stance.ToString()))
                        {
                            //return true;
                        }
                        else
                        {
                            return false;
                        }

                    }

                    if (condition.type == ConditionType.EFFECT && !condition.conditionSelf)
                    {
                        Effect ef = getEffect(target, condition.conditionName);
                        if (ef != null)
                        {
                            Log.DebugFormat("Effect {0}  (tgt) with stacks {1} ", ef.Name, ef.StackCount);

                            //return true;

                        }
                        else
                            return false;

                    }

                    if (condition.type == ConditionType.EFFECT && condition.conditionSelf)
                    {
                        Effect ef = getEffect(GameManager.LocalPlayer, condition.conditionName);
                        if (ef != null)
                        {
                            Log.DebugFormat("Effect {0}  (self) with stacks {1} ", ef.Name, ef.StackCount);

                            //return true;

                        }
                        else
                            return false;

                    }

                    if (condition.type == ConditionType.DEADSUMMON)
                    {
                        //Log.InfoFormat("Summon (dead) is valid= {0}", GameManager.SummonedMinion != null);
                        if (GameManager.SummonedMinion == null || (GameManager.SummonedMinion.IsValid && GameManager.SummonedMinion.HealthPercent==0))
                        {
                            //return true;
                        }
                        else
                        {
                            return false;
                        }

                    }

                    if (condition.type == ConditionType.SUMMONALIVE)
                    {
                        //                  Log.InfoFormat("Summon is valid= {0}", GameManager.SummonedMinion!=null);
                        if (GameManager.SummonedMinion != null && GameManager.SummonedMinion.IsValid && GameManager.SummonedMinion.HealthPercent > 0)
                        {
                            Log.DebugFormat("Summon Info Name: {0} hp: {1}", GameManager.SummonedMinion.Name, GameManager.SummonedMinion.HealthPercent);

                        }
                        else
                            return false;

                            //return true;
                       

                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error("Invalid Condition" + ex.Message);
                rtn = false;
            }

            return rtn;


         
           
        }


        /*
        public static bool move1 = false;
        public static void moveTest()
        {
            GameEngine.AttachedProcess.Memory.ClearCache();
            using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
            {
                Log.Info(GameManager.PlayerInput.Dump());
                if (CombatUtils.move1 == false)
                {
                    GameManager.PlayerInput.ToggleKeepWalking();
                }
            }

           *
        }*/

    }
}
