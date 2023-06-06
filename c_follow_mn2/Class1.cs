using CitizenFX.Core;
using CitizenFX.FiveM;
using System.Dynamic;
using System;
using CitizenFX.FiveM.Native;

namespace c_follow_mn2
{
    public class Class1 : BaseScript
    {
        private Ped[] peds = new Ped[0];
    public Class1()
    {
            EventHandlers["onClientResourceStart"] += Func.Create<string>(OnClientResourceStart);

    }
    private void OnClientResourceStart(string resourceName)
    {
        if (resourceName != Natives.GetCurrentResourceName())
        {
            return;
        }

        Events.TriggerEvent("chat:addSuggestion", "/follow", "Create npc to follow/open menu");

        Natives.RegisterCommand("follow", new Action(follow), false);

        Natives.RegisterNuiCallbackType("c_spawn");
        EventHandlers["__cfx_nui:c_spawn"] += Func.Create<ExpandoObject>(spawn);
        
        Natives.RegisterNuiCallbackType("c_anim");
        EventHandlers["__cfx_nui:c_anim"] += Func.Create<ExpandoObject>(anim);

        Natives.RegisterNuiCallbackType("c_delete");
        EventHandlers["__cfx_nui:c_delete"] += Func.Create(delete);

        Natives.RegisterNuiCallbackType("c_kill");
        EventHandlers["__cfx_nui:c_kill"] += Func.Create(kill);

        Natives.RegisterNuiCallbackType("c_cancel");
        EventHandlers["__cfx_nui:c_cancel"] += Func.Create(cancel);

        Natives.RegisterNuiCallbackType("c_follow");
        EventHandlers["__cfx_nui:c_follow"] += Func.Create(follow_again);
    }
    private async void spawn(dynamic data)
    {
        Natives.SetNuiFocus(false, false);
        string Model = data.model;
        byte cont = Convert.ToByte(data.cont);
        bool armed = data.armed;
        string weapon = data.weapon;
        bool combat = data.combat;

        Ped player = Game.Player.Character;
        Vector3 forward = Natives.GetEntityForwardVector(Natives.PlayerPedId());
        Vector3 coords = Natives.GetEntityCoords(Natives.PlayerPedId(), false);
        peds = new Ped[cont];

        if (Model == "random")
        {
            string[] Names = { "a_m_m_ktown_01", "a_m_y_beachvesp_02", "a_m_y_business_02", "a_m_y_gencaspat_01", "a_m_o_tramp_01", "a_m_m_soucent_01", "g_f_y_vagos_01", "g_f_y_families_01", "g_f_y_ballas_01", "g_m_y_mexgoon_03" };
            Random rnd = new Random();

            for (int i = 0; i < peds.Length; i++)
            {
                byte rand = (byte)rnd.Next(0, Names.Length);
                uint Hash = Natives.GetHashKey(Names[rand]);
                Natives.RequestModel(Hash);
                while (!Natives.HasModelLoaded(Hash))
                {
                    await Wait(100);
                }

                Ped npc = await World.CreatePed((Model)Names[rand], coords + forward * 2);
                peds[i] = npc;
            }
        }
        else
        {
            uint Hash = Natives.GetHashKey(Model);
            Natives.RequestModel(Hash);
            while (!Natives.HasModelLoaded(Hash))
            {
                await Wait(100);
            }
            for (int i = 0; i < peds.Length; i++)
            {
                Ped npc = await World.CreatePed((Model)Model, coords + forward * 2);
                peds[i] = npc;
            }
        }

        foreach (Ped i in peds)
        {
            i.Task.FollowToOffsetFromEntity(player, forward * 2, -1, 10);
            Natives.SetPedRelationshipGroupHash(i.Handle, Natives.GetHashKey("SECURITY_GUARD"));
            Natives.SetPedCombatAbility(i.Handle, 2);

            if (weapon != "")
            {
                Natives.GiveWeaponToPed(i.Handle, Natives.GetHashKey(weapon), 5000, armed, true);
            }

            if (combat)
            {
                Natives.SetPedCombatAbility(i.Handle, 2);
            }
            else
            {
                Natives.SetPedFleeAttributes(i.Handle, 0, true);
                Natives.SetPedCombatAttributes(i.Handle, 17, true);
            }
        }
    }

    private async void anim(dynamic data)
    {
        Natives.SetNuiFocus(false, false);
        string dist = data.dist;
        string anim = data.anim;

        while (!Natives.HasAnimDictLoaded(dist))
        {
            Natives.RequestAnimDict(dist);
            await Wait(100);
        }
        AnimationFlags flags = AnimationFlags.Loop | AnimationFlags.CancelableWithMovement;
        foreach (Ped i in peds)
        {
            i.Task.ClearAllImmediately();
            i.Task.PlayAnimation(dist, anim, -1, -1, flags);
        }

    }

    private void follow_again()
    {
        Natives.SetNuiFocus(false, false);
        Ped player = Game.Player.Character;
        Vector3 forward = Natives.GetEntityForwardVector(Natives.PlayerPedId());
        foreach (Ped i in peds)
        {
            i.Task.ClearAllImmediately();
            i.Task.FollowToOffsetFromEntity(player, forward * 2, -1, 10);
        }
    }

    private void delete()
    {
        Natives.SetNuiFocus(false, false);
        foreach (Ped i in peds)
        {
            i.Delete();
        }
        peds = new Ped[0];
    }
    private void kill()
    {
        Natives.SetNuiFocus(false, false);
        foreach (Ped i in peds)
        {
            i.Kill();
        }
        peds = new Ped[0];
    }

    private void cancel()
    {
        Natives.SetNuiFocus(false, false);
    }

    private void follow()
    {
        Natives.SetNuiFocus(true, true);
        if (peds.Length > 0)
        {
            Natives.SendNuiMessage("{\"action\":\"menu\"}");
        }
        else
        {
            Natives.SendNuiMessage("{\"action\":\"start\"}");
        }
    }
  }
}
