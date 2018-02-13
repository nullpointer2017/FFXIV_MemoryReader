﻿using System;
using System.Collections.Generic;
using TamanegiMage.FFXIV_MemoryReader.Model;

namespace TamanegiMage.FFXIV_MemoryReader.Core
{
    partial class Memory
    {
        private const int combatantDataSize = 16192; //0x3F40

        internal unsafe List<Model.CombatantV1> GetCombatantsV1()
        {
            List<CombatantV1> result = new List<CombatantV1>();

            try
            {
                int num = 344;
                int sz = 8;
                byte[] source = GetByteArray(Pointers[PointerType.MobArray].Address, sz * num);
                if (source == null || source.Length == 0) { return result; }

                for (int i = 0; i < num; i++)
                {
                    IntPtr p;
                    fixed (byte* bp = source) p = new IntPtr(*(Int64*)&bp[i * sz]);

                    if (!(p == IntPtr.Zero))
                    {
                        byte[] c = GetByteArray(p, combatantDataSize);
                        CombatantV1 combatant = GetCombatantFromByteArray(c);
                        if (combatant.type != ObjectType.PC && combatant.type != ObjectType.Monster)
                        {
                            continue;
                        }
                        if (combatant.ID != 0 && combatant.ID != 3758096384u && !result.Exists((CombatantV1 x) => x.ID == combatant.ID))
                        {
                            combatant.Order = i;
                            result.Add(combatant);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            
            return result;
        }

        public unsafe CombatantV1 GetCombatantFromByteArray(byte[] source)
        {
            int offset = 0;
            CombatantV1 combatant = new CombatantV1();
            fixed (byte* p = source)
            {
                combatant.Name = GetStringFromBytes(source, 48);
                combatant.ID = *(uint*)&p[116];
                combatant.OwnerID = *(uint*)&p[132];
                if (combatant.OwnerID == 3758096384u)
                {
                    combatant.OwnerID = 0u;
                }
                combatant.type = (ObjectType)p[140];
                combatant.EffectiveDistance = p[146];

                offset = 160;
                combatant.PosX = *(Single*)&p[offset];
                combatant.PosZ = *(Single*)&p[offset + 4];
                combatant.PosY = *(Single*)&p[offset + 8];
                combatant.Heading  = *(Single*)&p[offset + 16];

                combatant.TargetID = *(uint*)&p[5680];

                offset = 5808;
                if (combatant.type == ObjectType.PC || combatant.type == ObjectType.Monster)
                {
                    combatant.CurrentHP = *(int*)&p[offset + 8];
                    combatant.MaxHP = *(int*)&p[offset + 12];
                    combatant.CurrentMP = *(int*)&p[offset + 16];
                    combatant.MaxMP = *(int*)&p[offset + 20];
                    combatant.CurrentTP = *(short*)&p[offset + 24];
                    combatant.MaxTP = 1000;
                    combatant.Job = p[offset + 62];
                    combatant.Level = p[offset + 64];

                    // Status aka Buff,Debuff
                    combatant.Statuses = new List<StatusV1>();
                    const int StatusEffectOffset = 5992;
                    const int statusSize = 12;

                    int statusCountLimit = 60;
                    if (combatant.type == ObjectType.PC) statusCountLimit = 30;

                    var statusesSource = new byte[statusCountLimit * statusSize];
                    Buffer.BlockCopy(source, StatusEffectOffset, statusesSource, 0, statusCountLimit * statusSize);
                    for (var i = 0; i < statusCountLimit; i++)
                    {
                        var statusBytes = new byte[statusSize];
                        Buffer.BlockCopy(statusesSource, i * statusSize, statusBytes, 0, statusSize);
                        var status = new StatusV1
                        {
                            StatusID = BitConverter.ToInt16(statusBytes, 0),
                            Stacks = statusBytes[2],
                            Duration = BitConverter.ToSingle(statusBytes, 4),
                            CasterID = BitConverter.ToUInt32(statusBytes, 8),
                            IsOwner = false,
                        };

                        if (status.IsValid())
                        {
                            combatant.Statuses.Add(status);
                        }
                    }

                    // Cast
                    combatant.Casting = new CastV1
                    {
                        ID = *(short*)&p[6372],
                        TargetID = *(uint*)&p[6384],
                        Progress = *(Single*)&p[6420],
                        Time = *(Single*)&p[6424],
                    };
                }
                else
                {
                    combatant.CurrentHP =
                    combatant.MaxHP =
                    combatant.CurrentMP =
                    combatant.MaxMP =
                    combatant.MaxTP =
                    combatant.CurrentTP = 0;
                    combatant.Statuses = new List<StatusV1>();
                    combatant.Casting = new CastV1();
                }
            }
            return combatant;
        }

    }
}
