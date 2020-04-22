using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Theatrics
{
	internal class Turn
	{
		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int _001D;

		internal List<Phase> _000E = new List<Phase>(7);

		private int _0012 = -1;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		[CompilerGenerated]
		private float _0015;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float _0016;

		internal Bounds _0013;

		internal int _0018;

		internal bool _0009;

		private HashSet<int> _0019 = new HashSet<int>();

		internal int TurnID
		{
			get;
			private set;
		}

		internal float TimeInPhase
		{
			get;
			private set;
		}

		internal float TimeInResolve
		{
			get;
			private set;
		}

		internal Turn()
		{
		}

		internal static bool IsEvasionOrKnockback(AbilityPriority priority)
		{
			return priority == AbilityPriority.Evasion || priority == AbilityPriority.Combat_Knockback;
		}

		internal void _0011(IBitStream _001D)
		{
			int value = TurnID;
			_001D.Serialize(ref value);
			TurnID = value;
			sbyte value2 = (sbyte)_000E.Count;
			_001D.Serialize(ref value2);
			int num = 0;
			while (num < value2)
			{
				while (num >= _000E.Count)
				{
					_000E.Add(new Phase(this));
				}
				while (true)
				{
					switch (5)
					{
					case 0:
						continue;
					}
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					_000E[num].OnSerializeHelper(_001D);
					num++;
					goto IL_007d;
				}
				IL_007d:;
			}
			while (true)
			{
				switch (3)
				{
				default:
					return;
				case 0:
					break;
				}
			}
		}

		internal void _0011(AbilityPriority _001D)
		{
			if (_001D == (AbilityPriority)_0012)
			{
				return;
			}
			while (true)
			{
				switch (2)
				{
				case 0:
					continue;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				TimeInPhase = 0f;
				_0012 = (int)_001D;
				if (_0012 >= 0)
				{
					while (true)
					{
						switch (5)
						{
						case 0:
							continue;
						}
						break;
					}
					if (_0012 < _000E.Count)
					{
						while (true)
						{
							switch (7)
							{
							case 0:
								continue;
							}
							break;
						}
						TheatricsManager.Get().SetAnimatorParamOnAllActors("DecisionPhase", false);
						Phase phase = _000E[_0012];
						phase._001D_000E();
					}
				}
				List<ActorData> actors = GameFlowData.Get().GetActors();
				if (actors != null)
				{
					for (int i = 0; i < actors.Count; i++)
					{
						ActorData actorData = actors[i];
						if (!(actorData != null))
						{
							continue;
						}
						while (true)
						{
							switch (5)
							{
							case 0:
								continue;
							}
							break;
						}
						if (actorData.GetHitPointsAfterResolution() > 0)
						{
							continue;
						}
						while (true)
						{
							switch (6)
							{
							case 0:
								continue;
							}
							break;
						}
						if (actorData.IsModelAnimatorDisabled())
						{
							continue;
						}
						while (true)
						{
							switch (5)
							{
							case 0:
								continue;
							}
							break;
						}
						if (!GameplayData.Get().m_resolveDamageBetweenAbilityPhases)
						{
							while (true)
							{
								switch (6)
								{
								case 0:
									continue;
								}
								break;
							}
							if (!_0004(actorData))
							{
								continue;
							}
						}
						actorData.DoVisualDeath(Sequence.CreateImpulseInfoWithActorForward(actorData));
					}
				}
				if (!NetworkClient.active)
				{
					return;
				}
				while (true)
				{
					switch (2)
					{
					case 0:
						continue;
					}
					if (_001D != AbilityPriority.Combat_Damage)
					{
						return;
					}
					while (true)
					{
						switch (2)
						{
						case 0:
							continue;
						}
						if (!(ClientResolutionManager.Get() != null))
						{
							return;
						}
						while (true)
						{
							switch (6)
							{
							case 0:
								continue;
							}
							if (!ClientResolutionManager.Get().IsWaitingForActionMessages(_001D))
							{
								while (true)
								{
									switch (5)
									{
									case 0:
										continue;
									}
									ClientResolutionManager.Get().OnCombatPhasePlayDataReceived();
									return;
								}
							}
							return;
						}
					}
				}
			}
		}

		internal bool _001A(AbilityPriority _001D)
		{
			TimeInResolve += GameTime.deltaTime;
			if (_0012 >= 7)
			{
				while (true)
				{
					switch (2)
					{
					case 0:
						continue;
					}
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					return false;
				}
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = true;
			int num = _0012;
			if (num < 0)
			{
				while (true)
				{
					switch (4)
					{
					case 0:
						continue;
					}
					Log.Error("Phase index is negative! Code error.");
					return true;
				}
			}
			if (num < _000E.Count)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						continue;
					}
					break;
				}
				if (!_000E[num]._001C(this, ref flag, ref flag2))
				{
					goto IL_0099;
				}
			}
			flag3 = false;
			goto IL_0099;
			IL_0282:
			if (!(GameFlowData.Get() == null))
			{
				while (true)
				{
					switch (2)
					{
					case 0:
						continue;
					}
					break;
				}
				if (GameFlowData.Get().IsResolutionPaused())
				{
					goto IL_02cb;
				}
				while (true)
				{
					switch (7)
					{
					case 0:
						continue;
					}
					break;
				}
			}
			TimeInPhase += GameTime.deltaTime;
			goto IL_02cb;
			IL_02cb:
			bool flag4;
			if (!flag4)
			{
				while (true)
				{
					switch (6)
					{
					case 0:
						continue;
					}
					break;
				}
				if (!_0004(_001D) && NetworkClient.active && !_0019.Contains(_0012))
				{
					while (true)
					{
						switch (7)
						{
						case 0:
							continue;
						}
						break;
					}
					_0019.Add(_0012);
					GameEventManager.Get().FireEvent(GameEventManager.EventType.TheatricsAbilitiesEnd, null);
				}
			}
			return flag4;
			IL_0099:
			bool flag5 = TimeInPhase >= GameFlowData.Get().m_resolveTimeoutLimit * 0.8f;
			if (flag5)
			{
				while (true)
				{
					switch (1)
					{
					case 0:
						continue;
					}
					break;
				}
				string text = ServerClientUtils.GetCurrentActionPhase().ToString();
				Log.Error("Theatrics: phase: " + text + " timed out for turn " + TurnID + ",  timeline index " + _0012);
			}
			flag4 = true;
			if (!flag3)
			{
				while (true)
				{
					switch (2)
					{
					case 0:
						continue;
					}
					break;
				}
				if (!flag5)
				{
					if (flag)
					{
						while (true)
						{
							switch (2)
							{
							case 0:
								continue;
							}
							break;
						}
						if (!flag2)
						{
							while (true)
							{
								switch (7)
								{
								case 0:
									continue;
								}
								break;
							}
							if (GameFlowData.Get().activeOwnedActorData != null)
							{
								while (true)
								{
									switch (5)
									{
									case 0:
										continue;
									}
									break;
								}
								if (!GameFlowData.Get().activeOwnedActorData.IsDead())
								{
									while (true)
									{
										switch (2)
										{
										case 0:
											continue;
										}
										break;
									}
									InterfaceManager.Get().DisplayAlert(StringUtil.TR("HiddenAction", "Global"), Color.white);
								}
							}
							goto IL_0282;
						}
					}
					InterfaceManager.Get().CancelAlert(StringUtil.TR("HiddenAction", "Global"));
					goto IL_0282;
				}
			}
			flag4 = false;
			TheatricsManager.Get().no_op("Theatrics: finished timeline index " + _0012 + " with duration " + TimeInPhase + " @absolute time " + GameTime.time);
			if (TheatricsManager.DebugLog)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				TheatricsManager.LogForDebugging("Phase Finished: " + _0012);
			}
			goto IL_02cb;
		}

		internal void _0011(ActorData _001D, Object _000E, GameObject _0012)
		{
			if (this._0012 < 0)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						break;
					default:
						if (1 == 0)
						{
							/*OpCode not supported: LdMemberToken*/;
						}
						return;
					}
				}
			}
			if (this._0012 >= 7)
			{
				while (true)
				{
					switch (3)
					{
					default:
						return;
					case 0:
						break;
					}
				}
			}
			Phase phase = this._000E[this._0012];
			if (phase == null)
			{
				return;
			}
			while (true)
			{
				switch (3)
				{
				case 0:
					continue;
				}
				for (int i = 0; i < phase.animations.Count; i++)
				{
					ActorAnimation actorAnimation = phase.animations[i];
					if (!(actorAnimation.Actor == _001D))
					{
						continue;
					}
					while (true)
					{
						switch (7)
						{
						case 0:
							continue;
						}
						break;
					}
					if (!actorAnimation.PlaybackState2OrLater_zq)
					{
						continue;
					}
					while (true)
					{
						switch (3)
						{
						case 0:
							continue;
						}
						break;
					}
					if (!actorAnimation._0014_000E())
					{
						continue;
					}
					while (true)
					{
						switch (5)
						{
						case 0:
							continue;
						}
						actorAnimation._000D_000E(_001D, _000E, _0012);
						if (actorAnimation.ParentAbilitySeqSource != null)
						{
							while (true)
							{
								switch (3)
								{
								case 0:
									continue;
								}
								actorAnimation._0008_000E(_001D, _000E, _0012);
								return;
							}
						}
						return;
					}
				}
				while (true)
				{
					switch (3)
					{
					default:
						return;
					case 0:
						break;
					}
				}
			}
		}

		internal void _0011(Sequence _001D, ActorData _000E, ActorModelData.ImpulseInfo _0012, ActorModelData.RagdollActivation _0015 = ActorModelData.RagdollActivation.HealthBased)
		{
			if (this._0012 >= 7)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						break;
					default:
						if (1 == 0)
						{
							/*OpCode not supported: LdMemberToken*/;
						}
						return;
					}
				}
			}
			bool flag = false;
			bool flag2 = false;
			if (this._0012 >= 0)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				Phase phase = this._000E[this._0012];
				if (phase != null)
				{
					int num = 0;
					while (true)
					{
						if (num < phase.animations.Count)
						{
							ActorAnimation actorAnimation = phase.animations[num];
							if (actorAnimation.Actor == _001D.Caster)
							{
								while (true)
								{
									switch (6)
									{
									case 0:
										continue;
									}
									break;
								}
								if (actorAnimation.HasSameSequenceSource(_001D))
								{
									while (true)
									{
										switch (4)
										{
										case 0:
											continue;
										}
										break;
									}
									if (actorAnimation._000D_000E(_001D, _000E, _0012, _0015))
									{
										while (true)
										{
											switch (5)
											{
											case 0:
												continue;
											}
											break;
										}
										flag = true;
										break;
									}
									goto IL_00e7;
								}
							}
							if (actorAnimation.Actor == _000E && !actorAnimation._0014_000E() && actorAnimation.PlaybackState2OrLater_zq)
							{
								flag2 = true;
							}
							goto IL_00e7;
						}
						while (true)
						{
							switch (1)
							{
							case 0:
								continue;
							}
							break;
						}
						break;
						IL_00e7:
						num++;
					}
				}
			}
			if (flag)
			{
				return;
			}
			while (true)
			{
				switch (3)
				{
				case 0:
					continue;
				}
				if (!_001D.RequestsHitAnimation(_000E))
				{
					return;
				}
				while (true)
				{
					switch (2)
					{
					case 0:
						continue;
					}
					ActorModelData actorModelData = _000E.GetActorModelData();
					if (actorModelData != null)
					{
						if (flag2)
						{
							while (true)
							{
								switch (2)
								{
								case 0:
									continue;
								}
								break;
							}
							if (!actorModelData.CanPlayDamageReactAnim())
							{
								goto IL_0177;
							}
							while (true)
							{
								switch (7)
								{
								case 0:
									continue;
								}
								break;
							}
						}
						if (_001A(_000E))
						{
							_000E.PlayDamageReactionAnim(_001D.m_customHitReactTriggerName);
						}
					}
					goto IL_0177;
					IL_0177:
					if (_0015 == ActorModelData.RagdollActivation.None)
					{
						return;
					}
					while (true)
					{
						switch (3)
						{
						case 0:
							continue;
						}
						if (_0004(_000E))
						{
							while (true)
							{
								switch (7)
								{
								case 0:
									continue;
								}
								_000E.DoVisualDeath(_0012);
								return;
							}
						}
						return;
					}
				}
			}
		}

		internal Bounds _0011(Phase _001D, int _000E, out bool _0012)
		{
			_0012 = true;
			bool flag = _001D == null;
			ActorData actorData = GameFlowData.Get().activeOwnedActorData;
			if (actorData == null)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				List<ActorData> actors = GameFlowData.Get().GetActors();
				if (actors != null)
				{
					while (true)
					{
						switch (7)
						{
						case 0:
							continue;
						}
						break;
					}
					if (actors.Count != 0)
					{
						actorData = actors[0];
						goto IL_0087;
					}
					while (true)
					{
						switch (3)
						{
						case 0:
							continue;
						}
						break;
					}
				}
				Log.Error("No actors found to create Abilities Bounds.");
				return default(Bounds);
			}
			goto IL_0087;
			IL_0087:
			BoardSquare boardSquare = Board.Get().GetBoardSquare(actorData.transform.position);
			Bounds cameraBounds = boardSquare.CameraBounds;
			Vector3 center = cameraBounds.center;
			center.y = 0f;
			Bounds result = cameraBounds;
			bool flag2 = true;
			for (int i = 0; i < this._000E.Count; i++)
			{
				Phase phase = this._000E[i];
				if (_001D != null)
				{
					while (true)
					{
						switch (2)
						{
						case 0:
							continue;
						}
						break;
					}
					if (_001D != phase)
					{
						continue;
					}
					while (true)
					{
						switch (1)
						{
						case 0:
							continue;
						}
						break;
					}
				}
				for (int j = 0; j < phase.animations.Count; j++)
				{
					ActorAnimation actorAnimation = phase.animations[j];
					if (_000E >= 0)
					{
						while (true)
						{
							switch (5)
							{
							case 0:
								continue;
							}
							break;
						}
						if (actorAnimation.playOrderIndex != _000E)
						{
							continue;
						}
					}
					if (!actorAnimation._0014_000E())
					{
						continue;
					}
					while (true)
					{
						switch (4)
						{
						case 0:
							continue;
						}
						break;
					}
					if (actorAnimation._000C_000E())
					{
						continue;
					}
					while (true)
					{
						switch (1)
						{
						case 0:
							continue;
						}
						break;
					}
					if (actorAnimation.GetSymbol0013())
					{
						continue;
					}
					while (true)
					{
						switch (7)
						{
						case 0:
							continue;
						}
						break;
					}
					Bounds bound = actorAnimation._0020;
					if (_001D.Index == AbilityPriority.Evasion && actorAnimation.Actor != null)
					{
						while (true)
						{
							switch (4)
							{
							case 0:
								continue;
							}
							break;
						}
						ActorTeamSensitiveData teamSensitiveData_authority = actorAnimation.Actor.TeamSensitiveData_authority;
						if (teamSensitiveData_authority != null)
						{
							while (true)
							{
								switch (3)
								{
								case 0:
									continue;
								}
								break;
							}
							teamSensitiveData_authority.EncapsulateVisiblePathBound(ref bound);
						}
					}
					if (flag2)
					{
						result = bound;
						flag2 = false;
					}
					else
					{
						result.Encapsulate(bound);
					}
					_0012 = false;
				}
				while (true)
				{
					switch (4)
					{
					case 0:
						continue;
					}
					break;
				}
				if (!flag)
				{
					continue;
				}
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				if (!flag2)
				{
					while (true)
					{
						switch (3)
						{
						case 0:
							continue;
						}
						break;
					}
					break;
				}
			}
			return result;
		}

		internal bool _0011()
		{
			return _0004(AbilityPriority.INVALID);
		}

		internal bool _0004(AbilityPriority _001D)
		{
			for (int i = (int)(_001D + 1); i < _000E.Count; i++)
			{
				if (_000B((AbilityPriority)i))
				{
					return true;
				}
			}
			while (true)
			{
				switch (2)
				{
				case 0:
					continue;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				return false;
			}
		}

		internal bool _000B(AbilityPriority _001D)
		{
			int result;
			if (_001D >= AbilityPriority.Prep_Defense && (int)_001D < _000E.Count)
			{
				while (true)
				{
					switch (6)
					{
					case 0:
						continue;
					}
					break;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				result = ((_000E[(int)_001D].animations.Count > 0) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		internal bool _0003(AbilityPriority _001D)
		{
			if (_001D >= AbilityPriority.Prep_Defense)
			{
				while (true)
				{
					switch (4)
					{
					case 0:
						continue;
					}
					break;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				if ((int)_001D < _000E.Count)
				{
					return _000E[(int)_001D]._001C();
				}
			}
			return false;
		}

		private bool _0011(ActorData _001D)
		{
			return _0011(_001D, _0012);
		}

		internal bool _0011(ActorData _001D, int _000E)
		{
			if (_001D.HitPoints <= 0)
			{
				while (true)
				{
					switch (1)
					{
					case 0:
						break;
					default:
						if (1 == 0)
						{
							/*OpCode not supported: LdMemberToken*/;
						}
						return true;
					}
				}
			}
			if (_000E >= 7)
			{
				return _001D.GetHitPointsAfterResolution() <= 0;
			}
			int num = 0;
			for (int i = 0; i <= _000E; i++)
			{
				Dictionary<int, int> dictionary = this._000E[i].ActorIndexToDeltaHP;
				if (dictionary == null)
				{
					continue;
				}
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				if (dictionary.ContainsKey(_001D.ActorIndex))
				{
					while (true)
					{
						switch (3)
						{
						case 0:
							continue;
						}
						break;
					}
					num += dictionary[_001D.ActorIndex];
				}
			}
			while (true)
			{
				switch (4)
				{
				case 0:
					continue;
				}
				return _001D.HitPoints + _001D.AbsorbPoints + num <= 0;
			}
		}

		internal bool _001A(ActorData _001D)
		{
			if (_001D.IsModelAnimatorDisabled())
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						break;
					default:
						if (1 == 0)
						{
							/*OpCode not supported: LdMemberToken*/;
						}
						return false;
					}
				}
			}
			if (_0012 > 0)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						continue;
					}
					break;
				}
				if (_0012 < _000E.Count)
				{
					while (true)
					{
						switch (3)
						{
						case 0:
							continue;
						}
						break;
					}
					List<ActorAnimation> animations = _000E[_0012].animations;
					for (int i = 0; i < animations.Count; i++)
					{
						ActorAnimation actorAnimation = animations[i];
						if (!(actorAnimation.Actor == _001D))
						{
							continue;
						}
						while (true)
						{
							switch (3)
							{
							case 0:
								continue;
							}
							break;
						}
						if (!actorAnimation.PlaybackState2OrLater_zq)
						{
							continue;
						}
						while (true)
						{
							switch (7)
							{
							case 0:
								continue;
							}
							break;
						}
						if (actorAnimation.AnimationFinished)
						{
							continue;
						}
						while (true)
						{
							switch (1)
							{
							case 0:
								continue;
							}
							return false;
						}
					}
					while (true)
					{
						switch (1)
						{
						case 0:
							continue;
						}
						break;
					}
				}
			}
			return true;
		}

		internal bool _0004(ActorData _001D, int _000E = 0, int _0012 = -1)
		{
			if (_001D.GetHitPointsAfterResolution() + _000E <= 0)
			{
				while (true)
				{
					switch (2)
					{
					case 0:
						continue;
					}
					break;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				if (!_001D.IsModelAnimatorDisabled())
				{
					while (true)
					{
						switch (5)
						{
						case 0:
							continue;
						}
						break;
					}
					if (_0011(_001D))
					{
						while (true)
						{
							switch (4)
							{
							case 0:
								continue;
							}
							break;
						}
						if (this._0012 >= 3)
						{
							int num = this._0012;
							int num2 = this._0012;
							while (true)
							{
								if (num < this._000E.Count)
								{
									while (true)
									{
										switch (7)
										{
										case 0:
											continue;
										}
										break;
									}
									if (num >= 0)
									{
										while (true)
										{
											switch (1)
											{
											case 0:
												continue;
											}
											break;
										}
										if (num == 5 && this._000E[num]._001D_000E(_001D))
										{
											while (true)
											{
												switch (2)
												{
												case 0:
													break;
												default:
													return false;
												}
											}
										}
										List<ActorAnimation> animations = this._000E[num].animations;
										for (int i = 0; i < animations.Count; i++)
										{
											ActorAnimation actorAnimation = animations[i];
											if (_0012 >= 0)
											{
												if (actorAnimation.SeqSource.RootID == _0012)
												{
													continue;
												}
												while (true)
												{
													switch (2)
													{
													case 0:
														continue;
													}
													break;
												}
											}
											if (actorAnimation.Actor == _001D)
											{
												while (true)
												{
													switch (7)
													{
													case 0:
														continue;
													}
													break;
												}
												if (actorAnimation._0014_000E())
												{
													goto IL_0130;
												}
												while (true)
												{
													switch (1)
													{
													case 0:
														continue;
													}
													break;
												}
											}
											if (!actorAnimation._0008_000E(_001D))
											{
												continue;
											}
											goto IL_0130;
											IL_0130:
											return false;
										}
										while (true)
										{
											switch (2)
											{
											case 0:
												continue;
											}
											break;
										}
										if (num > num2)
										{
											while (true)
											{
												switch (6)
												{
												case 0:
													continue;
												}
												break;
											}
											if (this._000E[num]._000E_000E(_001D))
											{
												while (true)
												{
													switch (2)
													{
													case 0:
														break;
													default:
														return false;
													}
												}
											}
										}
									}
								}
								num++;
								if (num >= 7)
								{
									break;
								}
								while (true)
								{
									switch (1)
									{
									case 0:
										break;
									default:
										goto end_IL_0185;
									}
									continue;
									end_IL_0185:
									break;
								}
								if (GameplayData.Get().m_resolveDamageBetweenAbilityPhases)
								{
									while (true)
									{
										switch (3)
										{
										case 0:
											continue;
										}
										break;
									}
									break;
								}
							}
							if (ClientResolutionManager.Get() != null && ClientResolutionManager.Get().HasUnexecutedHitsOnActor(_001D, _0012))
							{
								while (true)
								{
									switch (4)
									{
									case 0:
										break;
									default:
										return false;
									}
								}
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool _001A()
		{
			if (_0012 < _000E.Count)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					break;
				}
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				if (_0012 >= 0)
				{
					while (true)
					{
						switch (4)
						{
						case 0:
							continue;
						}
						break;
					}
					List<ActorAnimation> animations = _000E[_0012].animations;
					for (int i = 0; i < animations.Count; i++)
					{
						if (!animations[i].cinematicCamera)
						{
							continue;
						}
						if (animations[i].State != ActorAnimation.PlaybackState._0012)
						{
							while (true)
							{
								switch (1)
								{
								case 0:
									continue;
								}
								break;
							}
							if (animations[i].State != ActorAnimation.PlaybackState._0015)
							{
								while (true)
								{
									switch (5)
									{
									case 0:
										continue;
									}
									break;
								}
								if (animations[i].State != ActorAnimation.PlaybackState._0016)
								{
									continue;
								}
								while (true)
								{
									switch (2)
									{
									case 0:
										continue;
									}
									break;
								}
							}
						}
						return true;
					}
				}
			}
			return false;
		}
	}
}
