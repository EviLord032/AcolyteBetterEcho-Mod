using System.Runtime.CompilerServices;
using EntityStates.Acolyte.Backpack;
using RoR2;
using UnityEngine.Networking;

namespace AcolyteBetterEcho
{
    /// <summary>
    /// Gameplay changes for the Anima Echo special:
    /// 1. Activating Echo resets every skill (incl. Blade Slice quickstrike charges and Sacrifice).
    /// 2. Sacrifice grants more Convictions while Echo is the selected special.
    /// 3. Convictions gained during the Echo Litany are buffered and granted when it ends.
    /// </summary>
    internal static class EchoChanges
    {
        private const string AnimaEchoNameToken = "ACOLYTE_SPECIAL_ALT_TRANSFORM_NAME";

        private class ControllerState
        {
            public int bufferedSouls;
            // Sacrifice is reset once the Echo skill override has left the special slot
            public bool pendingSpecialReset;
        }

        private static readonly ConditionalWeakTable<AcolyteFormController, ControllerState> states =
            new ConditionalWeakTable<AcolyteFormController, ControllerState>();

        internal static void Init()
        {
            On.EntityStates.Acolyte.Weapon.Sacrifice.OnEnter += Sacrifice_OnEnter;
            On.EntityStates.Acolyte.Sacrifice.SacrificeIdle.FixedUpdate += SacrificeIdle_FixedUpdate;
            On.EntityStates.Acolyte.Backpack.BaseBackpackState.OnEnter += BaseBackpackState_OnEnter;
            On.EntityStates.Acolyte.Backpack.BaseBackpackState.OnExit += BaseBackpackState_OnExit;
            On.RoR2.AcolyteFormController.AddSouls += AcolyteFormController_AddSouls;
        }

        private static ControllerState GetState(AcolyteFormController controller)
        {
            return states.GetOrCreateValue(controller);
        }

        internal static bool IsEchoSelected(AcolyteFormController controller)
        {
            GenericSkill slot = controller ? controller.transformSkillSlot : null;
            return slot && slot.skillDef && slot.skillDef.skillNameToken == AnimaEchoNameToken;
        }

        private static void Sacrifice_OnEnter(On.EntityStates.Acolyte.Weapon.Sacrifice.orig_OnEnter orig, EntityStates.Acolyte.Weapon.Sacrifice self)
        {
            // soulValue is read on the server when the Sacrifice orb fires
            if (IsEchoSelected(self.outer.GetComponent<AcolyteFormController>()))
            {
                self.soulValue = AcolyteBetterEchoPlugin.EchoSacrificeConvictions.Value;
            }
            orig(self);
        }

        private static void SacrificeIdle_FixedUpdate(On.EntityStates.Acolyte.Sacrifice.SacrificeIdle.orig_FixedUpdate orig, EntityStates.Acolyte.Sacrifice.SacrificeIdle self)
        {
            orig(self);

            if (!self.controller)
            {
                return;
            }
            ControllerState state = GetState(self.controller);
            if (state.pendingSpecialReset)
            {
                // The special slot keeps a separate cooldown per override, so wait until
                // SacrificeReady has removed the Echo override before resetting Sacrifice
                GenericSkill special = self.outer.GetComponent<SkillLocator>()?.special;
                if (!special)
                {
                    state.pendingSpecialReset = false;
                }
                else if (special.skillDef == special.baseSkill)
                {
                    special.Reset();
                    state.pendingSpecialReset = false;
                }
                else if (special.skillDef != self.controller.transformSkillSlot?.skillDef)
                {
                    // Another override (e.g. Essence of Heresy) holds the slot: Sacrifice isn't
                    // there to reset, so drop the reset rather than firing it later
                    state.pendingSpecialReset = false;
                }
            }
        }

        private static void BaseBackpackState_OnEnter(On.EntityStates.Acolyte.Backpack.BaseBackpackState.orig_OnEnter orig, BaseBackpackState self)
        {
            AcolyteFormController controller = self.outer.GetComponent<AcolyteFormController>();
            if (controller)
            {
                GetState(controller).bufferedSouls = 0;
            }

            orig(self);

            if (self is BackpackActivate && IsEchoSelected(controller) && Util.HasEffectiveAuthority(self.outer.gameObject))
            {
                ResetSkills(self.outer.GetComponent<SkillLocator>());
                GetState(controller).pendingSpecialReset = true;
            }
        }

        private static void ResetSkills(SkillLocator skillLocator)
        {
            if (!skillLocator)
            {
                return;
            }
            // Primary stock is the Blade Slice quickstrike charges
            if (skillLocator.primary)
            {
                skillLocator.primary.Reset();
            }
            if (skillLocator.secondary)
            {
                skillLocator.secondary.Reset();
            }
            if (skillLocator.utility)
            {
                skillLocator.utility.Reset();
            }
        }

        private static void BaseBackpackState_OnExit(On.EntityStates.Acolyte.Backpack.BaseBackpackState.orig_OnExit orig, BaseBackpackState self)
        {
            orig(self);

            AcolyteFormController controller = self.outer.GetComponent<AcolyteFormController>();
            if (!controller)
            {
                return;
            }
            ControllerState state = GetState(controller);
            // Never fire a Sacrifice reset left over from this Litany at a random later moment
            state.pendingSpecialReset = false;
            if (!NetworkServer.active)
            {
                return;
            }
            int buffered = state.bufferedSouls;
            state.bufferedSouls = 0;
            // inTransformation is false now, so our AddSouls hook won't buffer this again.
            // Vanilla AddSouls caps the total at maxSoul, so anything above 7 is dropped
            if (buffered > 0 && controller.characterBody && controller.characterBody.healthComponent.alive)
            {
                controller.AddSouls(buffered);
            }
        }

        private static void AcolyteFormController_AddSouls(On.RoR2.AcolyteFormController.orig_AddSouls orig, AcolyteFormController self, int amount)
        {
            if (NetworkServer.active && self.isEchoForm && IsEchoSelected(self))
            {
                GetState(self).bufferedSouls += amount;
                return;
            }
            orig(self, amount);
        }
    }
}
