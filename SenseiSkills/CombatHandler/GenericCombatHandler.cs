﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Buddy.BladeAndSoul.Game;
using Buddy.BladeAndSoul.Game.Objects;
using Buddy.Coroutines;

using log4net;

namespace SenseiSkills.CombatHandler
{
	class GenericCombatHandler : ICombatHandler
	{
		private static readonly ILog Log = LogManager.GetLogger("[SenseiSkills][Generic]");
		
		private SkillInfo _currentChain;


		private SkillInfo _nextSkill;
		bool breakCC;

		readonly List<string> cclist = new List<string> {"Knockdown"};
		private int chainPosition;
		bool inBlock;
		private readonly SenseiProfile profile;
		readonly List<string> sslist = new List<string> {"Backstep", "Eclipse"};

		public GenericCombatHandler(SenseiProfile profile)
		{
			this.profile = profile;
		}

		#region ICombatHandler Members

		public async Task Combat()
		{
			Log.Info("Combat Called===============");

			Stopwatch sw = Stopwatch.StartNew();
			if (await DecideNextSkill() == false)
			{
				Log.InfoFormat("Elapsed {0}ms deciding skill", sw.ElapsedMilliseconds);
			}
			else
			{
				Log.InfoFormat("Elapsed {0}ms deciding skill", sw.ElapsedMilliseconds);
				//await Coroutine.Yield();
				Log.InfoFormat("Cast Skill {0}", _nextSkill.skillName);
				/*
                    if (await CombatUtils.waitGCD(_nextSkill.skillName))
                    {

                    }
                    else
                    {
                        return;
                    }*/
				sw = Stopwatch.StartNew();
				await FireNextSkill();
				Log.InfoFormat("Elapsed {0}ms firing skill", sw.ElapsedMilliseconds);
			}

			Log.Info("Combat Finished===============");
		}

		public async Task Pull()
		{
			inBlock = false;

			//Log.Info("Rest Called===============");
			if (profile.attackTarget)
			{
				//PULL
				Actor target = null;
				try
				{
					target = GameManager.LocalPlayer.CurrentTarget;

					if (target == null)
					{
						target = CombatUtils.ClosestEnemy(true);
					}

					if (CombatUtils.ValidateTarget(target))
					{
					}
					else
					{
						return;
					}

					CombatUtils.TurnToActor(target);

					Log.Info("Init Combat to " + target.Name + " Dis:" + (target.Distance / 50));

					if ((target.Distance / 50) < profile.gapCloseRange)
					{
						Log.Info("Try to PULL Close for range " + (target.Distance / 50));
						foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.PULL)).ToList())
						{
							if (await CombatUtils.ValidateConditions(skill, target, inBlock))
							{
								_nextSkill = skill;
								await FireNextSkill();
								return;
							}
						}
					}
				}
				catch (Exception ex)
				{
					//Log.Error("NO TARGET FOUND (pull)= " + ex.Message);
				}
			}
		}

		public async Task Loot()
		{
			//Log.Info("Moving Around");
			//await StrafeAround();

			//Log.InfoFormat("Player HP {0} MAX HP {1} PCTHP {2}", GameManager.LocalPlayer.Health, GameManager.LocalPlayer.MaxHealth, GameManager.LocalPlayer.HealthPercent);
			//Log.Info("Heal Called===============");

			if (GameManager.LocalPlayer.IsCasting)
			{
				await Coroutine.Sleep(100);
				return;
			}

			await CombatUtils.DoLoot();
		}

		#endregion

		public async Task<bool> DecideNextSkill()
		{
			Log.Info("Deciding Next Skill");
			Log.Debug("Actor count (for refresh)" + GameManager.Actors.ToList().Count);
			breakCC = false;


			//Log.InfoFormat("Current Player Stance {0}", GameManager.LocalPlayer.Stance);

			List<Effect> targetEffects = new List<Effect>();
			List<Effect> selfEffects = new List<Effect>();

			Actor target = null;
			try
			{
				Log.Info("Checking current target...");
				target = GameManager.LocalPlayer.CurrentTarget;

				if (CombatUtils.ValidateTarget(target))
				{
				}
				else
				{
					return false;
				}

				Log.Info("Killing " + target.Name + " Dis:" + (target.Distance / 50));
				Log.InfoFormat("Creature Type: " + target.CreatureType);
			}
			catch (Exception ex)
			{
				//Log.Error("NO TARGET FOUND (combat) = "+ex.Message);
				_nextSkill = null;
				_currentChain = null;
				chainPosition = 0;
				return false;
			}

			try
			{
				selfEffects = GameManager.LocalPlayer.Effects.ToList();

				Log.DebugFormat("Self Effect count: " + selfEffects.Count);

				breakCC = CombatUtils.EffectInList(GameManager.LocalPlayer, cclist);
				Log.Info("Lets Break CC =" + breakCC);

				foreach (Effect effect in selfEffects.ToList())
				{
					Log.DebugFormat("Self Effect==> {0} Stack {1}", effect.Name, effect.StackCount);


					// Log.Info(effect.Dump());
				}
			}
			catch (Exception ex)
			{
				Log.Error("PROBLEM GETTING EFECTS = " + ex.Message);
			}

			if (CombatUtils.KeepBlock(target, profile.skillList.Where(i => i.type.Equals(SkillType.BLOCK))) && GameManager.LocalPlayer.IsCasting)
			{
				inBlock = true;
			}
			else
			{
				inBlock = false;
			}

			if (_currentChain != null && _currentChain.channeledSkill && CombatUtils.KeepChannel(_currentChain.skillName))
			{
				return false;
			}

			try
			{
				Log.Info("Evaluating EVADE");
				foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.EVADE)))
				{
					if (await CombatUtils.ValidateConditions(skill, target, inBlock))
					{
						_nextSkill = skill;
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Problem Performing EVADE");
			}

			try
			{
				Log.Info("Evaluating Block");

				foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.BLOCK)))
				{
					if (CombatUtils.CanBlock(target, skill.skillName) && await CombatUtils.ValidateConditions(skill, target, inBlock))
					{
						Log.Info("Can Cast Block...");
						_nextSkill = skill;
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Problem Performing Block");
			}

			try
			{
				//CC BREAK
				if (breakCC)
				{
					Log.Info("Evaluating Break CC");
					foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.CCBREAK)))
					{
						if (await CombatUtils.ValidateConditions(skill, target, inBlock))
						{
							_nextSkill = skill;
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Problem Performing BreakCC");
			}

			//Chain?

			if (_currentChain != null)
			{
				Log.InfoFormat("Evaluating Chain for skill {0} in pos {1}", _currentChain.skillName, chainPosition);
				//should chain 

				try
				{
					Log.InfoFormat("Chains {0} for skill {1}", _currentChain.skillName, _currentChain.chainSkill.Count);
					if (_currentChain.chainSkill.Count > chainPosition)
					{
						if (await CombatUtils.ValidateConditions(_currentChain.chainSkill.ElementAt(chainPosition), target, inBlock))
						{
							_nextSkill = _currentChain.chainSkill.ElementAt(chainPosition);
							chainPosition++;
							return true;
						}
					}
					else
					{
						_nextSkill = null;
						_currentChain = null;
						chainPosition = 0;
						return false;
					}
				}
				catch (Exception ex)
				{
					Log.Error("Problem Performing Chain (prolly no chain)");
				}
			}

			try
			{
				Log.Info("Evaluating Regular DPS");
				foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.DPS)))
				{
					if (await CombatUtils.ValidateConditions(skill, target, inBlock))
					{
						_nextSkill = skill;
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Problem Performing Regular DPS");
			}

			//DEFAULT

			try
			{
				Log.Info("Evaluating DEFAULT DPS");
				foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.DEFAULT)))
				{
					if (await CombatUtils.ValidateConditions(skill, target, inBlock))
					{
						_nextSkill = skill;
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Problem Performing Defautl");
			}

			return false;
		}


		async Task<bool> FireNextSkill()
		{
			if (_nextSkill == null)
			{
				return false;
			}

			Actor target = GameManager.LocalPlayer.CurrentTarget;

			try
			{
				CombatUtils.TurnToActor(target);
			}
			catch (Exception ex)
			{
				Log.Info("Problem Turning to Target");
				return false;
			}

			if (await ExecuteSkill(_nextSkill, target))
			{
				var skill = GameManager.LocalPlayer.GetSkillByName(_nextSkill.skillName);
				var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);
				var castResultSummon = SkillUseError.Unknown;

				if (GameManager.SummonedMinion != null && GameManager.SummonedMinion.IsValid)
				{
					castResultSummon = skill.ActorCanCastResult(GameManager.SummonedMinion);
				}

				Log.Info(_nextSkill.skillName + " CanCast result: " + castResult);
				if ((castResult <= SkillUseError.None) && (castResultSummon <= SkillUseError.None))
				{
					Log.InfoFormat("Skill {0} didnt fire", _nextSkill.skillName);
					_nextSkill = null;
					return true;
				}
				Log.InfoFormat("Skill {0} did fire", _nextSkill.skillName);
				_currentChain = _nextSkill;
				_nextSkill = null;
				return false;
			}
			_nextSkill = null;
			_currentChain = null;
			chainPosition = 0;
			return false;
		}


		async Task<bool> ExecuteSS(string skillName)
		{
			//dumpSkillsnActions();
			//Keys hotkey = Keys.R;
			int castDuration = profile.minCastTime;

			//Log.Info("Cheking skill: " + skillName);

			var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
			if (skill == null)
			{
				Log.Info(skillName + " not available");
				return false;
			}

			var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);

			// Log.Info("Cast Duration " + skill.CastDuration);

			Log.Info(skillName + " CanCast result: " + castResult + "Range min:" + skill.MinRange + " Max:" + skill.MaxRange);
			if (!(castResult <= SkillUseError.None) && castResult != SkillUseError.LinkFailed)
				return false;

			Log.Info("+++Casting " + skillName + " on key " + "SS" + " with sleep: " + castDuration);
			InputManager.PressKey(Keys.S);
			await Coroutine.Sleep(100);
			InputManager.PressKey(Keys.S);
			await Coroutine.Sleep(castDuration);
			return true;
		}


		async Task<bool> ExecuteSkill(SkillInfo skillInfo, Actor target)
		{
			if (sslist.Contains(skillInfo.skillName))
			{
				return await ExecuteSS(skillInfo.skillName);
			}

			int castDuration = profile.minCastTime;

			//Log.Info("Cheking skill: " + skillName);

			var skill = GameManager.LocalPlayer.GetSkillByName(skillInfo.skillName);
			if (skill == null)
			{
				//Log.Info(skillName + " not available");
				return false;
			}

			if (await CombatUtils.BasicSkillCondition(skill, skillInfo, target))
			{
				skill.Cast();

				int fullCast = skill.CastDuration; //TODO check if we can anicancel lateron

				/*
                if (skillInfo.channeledSkill)
                {
                    fullCast = skill.Record.ExecDuration1 + skill.Record.ExecDuration2
                       + skill.Record.ExecDuration3 + skill.Record.ExecDuration4 + skill.Record.ExecDuration5;

                }*/

				if (fullCast > castDuration)
				{
					castDuration = fullCast;
				}

				Log.Warn("+++Casting " + skillInfo.skillName + " with sleep: " + castDuration);
				await Coroutine.Sleep(castDuration);
				Log.Info("Done waiting " + castDuration);

				return true;
			}
			return false;
		}
	}
}