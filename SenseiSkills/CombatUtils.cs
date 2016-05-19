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
                IEnumerable<Actor> actors = GameManager.Actors.Where(e =>e.IsValid && e.CurrentTargetId == GameManager.LocalPlayer.Id && e.IsHostile);
                
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


        public static bool isDanger(Actor target)
        {
            Actor targetTarget = target.CurrentTarget;

            if(targetTarget.Equals(GameManager.LocalPlayer))
            {
                Log.Info("OMG is targetting me ");
                return true;
               
            }else
            {
                return false; //we are good for the time being.
            }

        }

        public static bool validateConditions(SkillInfo skill,Actor target)
        {

            Log.InfoFormat("Checking Condition for skill {0}", skill.skillName);
            bool rtn = true;

            try
            {

                foreach (SkillCondition condition in skill.conditions)
                {


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
                        Log.InfoFormat("Dist to TG {0} <= {1}", target.Distance / 50, condition.conditionAmount);
                        if ((target.Distance / 50) <= condition.conditionAmount)
                        { }
                        else
                            return false;

                    }

                    if (condition.type == ConditionType.DISTGT)
                    {
                        Log.InfoFormat("Dist to GT {0} >= {1}", target.Distance / 50, condition.conditionAmount);
                        if ((target.Distance / 50) >= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.STACK && !condition.conditionSelf)
                    {
                        Effect ef = getEffect(target, condition.conditionName);
                        if (ef != null)
                        {
                            Log.InfoFormat("Effect {0} with stacks (tgt) {1} >= {2}", ef.Name, ef.StackCount, condition.stackCount);
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
                            Log.InfoFormat("Effect {0} with stacks (self) {1} >= {2}", ef.Name, ef.StackCount, condition.stackCount);
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
                        Log.InfoFormat("Player Stance {0} required Stance {1}", GameManager.LocalPlayer.Stance.ToString(), condition.conditionName);
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
                            Log.InfoFormat("Effect {0}  (tgt) with stacks {1} ", ef.Name, ef.StackCount);

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
                            Log.InfoFormat("Effect {0}  (self) with stacks {1} ", ef.Name, ef.StackCount);

                            //return true;

                        }
                        else
                            return false;

                    }

                    if (condition.type == ConditionType.DEADSUMMON)
                    {
                        Log.InfoFormat("PCSummonedId {0}", GameManager.LocalPlayer.PcSummonedId);
                        if (GameManager.LocalPlayer.PcSummonedId == 0)
                        {
                            //return true;
                        }
                        else
                        {
                            return false;
                        }

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

    }
}
