using CitizenFX.Core;
using CitizenFX.FiveM;
using System.Collections.Generic;
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
            Debug.WriteLine("not working");
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);

    }
    private void OnClientResourceStart(string resourceName)
    {
        if (resourceName != Natives.GetCurrentResourceName())
        {
            return;
        }

        Events.TriggerEvent("chat:addSuggestion", "/follow", "Create npc to follow/open menu");


        Natives.RegisterCommand("follow", new Action<int, List<object>, string>(follow), false);

        //TriggerEvent("chat:addSuggestion", "/follow-anim", "Tast play anim for NPC or Reset following", new[] 
        //{
        // new { name = "dist", help = "dist"},
        // new { name = "anim", help = "anim"},
        //});

        //Natives.RegisterCommand("follow-anim", new Action<int, List<object>, string>(follow_anim), false);

        Natives.RegisterNuiCallbackType("c_spawn");
        EventHandlers["__cfx_nui:c_spawn"] += new Action<ExpandoObject>(spawn);

        Natives.RegisterNuiCallbackType("c_anim");
        EventHandlers["__cfx_nui:c_anim"] += new Action<ExpandoObject>(anim);

        Natives.RegisterNuiCallbackType("c_delete");
        EventHandlers["__cfx_nui:c_delete"] += new Action(delete);

        Natives.RegisterNuiCallbackType("c_kill");
        EventHandlers["__cfx_nui:c_kill"] += new Action(kill);

        Natives.RegisterNuiCallbackType("c_cancel");
        EventHandlers["__cfx_nui:c_cancel"] += new Action(cancel);

        Natives.RegisterNuiCallbackType("c_follow");
        EventHandlers["__cfx_nui:c_follow"] += new Action(follow_again);
    }
    private async void spawn(dynamic data)
    {
        Natives.SetNuiFocus(false, false);
        String Model = data.model;
        byte cont = Convert.ToByte(data.cont);
        bool armed = data.armed;
        string weapon = data.weapon;
        bool combat = data.combat;

        Ped player = Game.Player.Character;
        peds = new Ped[cont];

        if (Model == "random")
        {
            String[] Names = { "a_m_m_ktown_01", "a_m_y_beachvesp_02", "a_m_y_business_02", "a_m_y_gencaspat_01", "a_m_o_tramp_01", "a_m_m_soucent_01", "g_f_y_vagos_01", "g_f_y_families_01", "g_f_y_ballas_01", "g_m_y_mexgoon_03" };
            Random rnd = new Random();

            for (int i = 0; i < peds.Length; i++)
            {
                Byte rand = (byte)rnd.Next(0, Names.Length);
                uint Hash = (uint)Natives.GetHashKey(Names[rand]);
                Natives.RequestModel(Hash);
                while (!Natives.HasModelLoaded(Hash))
                {
                    await BaseScript.Delay(100);
                }

                Ped npc = await World.CreatePed((Model)Names[rand], player.Position + (player.ForwardVector * 2));
                //npc.Task.LookAt(player);
                //npc.Task.FollowToOffsetFromEntity(player, (player.ForwardVector * 2), -1, 10);

                //Natives.SetPedAsGroupMember(npc.Handle, Natives.GetPedGroupIndex(npc.Handle));
                //Natives.SetPedCombatAbility(npc.Handle, 2);
                peds[i] = npc;
            }
        }
        else
        {
            uint Hash = (uint)Natives.GetHashKey(Model);
            Natives.RequestModel(Hash);
            while (!Natives.HasModelLoaded(Hash))
            {
                await BaseScript.Delay(100);
            }
            for (int i = 0; i < peds.Length; i++)
            {
                Ped npc = await World.CreatePed((Model)Model, player.Position + (player.ForwardVector * 2));
                //npc.Task.LookAt(player);
                //npc.Task.FollowToOffsetFromEntity(player, (player.ForwardVector * 2), -1, 10);

                //Natives.SetPedAsGroupMember(npc.Handle, Natives.GetPedGroupIndex(npc.Handle));
                //Natives.SetPedCombatAbility(npc.Handle, 2);
                peds[i] = npc;
            }
        }

        foreach (Ped i in peds)
        {
            i.Task.FollowToOffsetFromEntity(player, (player.ForwardVector * 2), -1, 10);
            //Natives.SetPedAsGroupMember(i.Handle, Natives.GetPedGroupIndex(peds[0].Handle));
            Natives.SetPedRelationshipGroupHash(i.Handle, (uint)Natives.GetHashKey("SECURITY_GUARD"));
            Natives.SetPedCombatAbility(i.Handle, 2);

            if (weapon != "")
            {
                Natives.GiveWeaponToPed(i.Handle, (uint)Natives.GetHashKey(weapon), 5000, armed, true);
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
        String dist = data.dist;
        String anim = data.anim;

        while (!Natives.HasAnimDictLoaded(dist))
        {
            Natives.RequestAnimDict(dist);
            await BaseScript.Delay(100);
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
        foreach (Ped i in peds)
        {
            i.Task.ClearAllImmediately();
            i.Task.FollowToOffsetFromEntity(player, (player.ForwardVector * 2), -1, 10);
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

    private void follow(int source, List<object> args, string raw)
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

        //if ((bool)args.Any())
        //{
        //    if (peds.Length != 0)
        //    {
        //        foreach (Ped i in peds)
        //        {
        //            i.Delete();
        //        }
        //    }
        //    byte cont = 4;
        //    if (args.ElementAtOrDefault(1) != null)
        //    {
        //        if (byte.TryParse(args[1].ToString(), out _))
        //        {
        //            cont = Convert.ToByte(args[1]);
        //            if (cont < 0 && cont > 40) //zmienić !!!!
        //            {
        //                cont = 4;
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    foreach (Ped i in peds)
        //    {
        //        i.Delete();
        //    }
        //    peds = new Ped[0];
        //}

    }

    //private async void follow_anim(int source, List<object> args, string raw)
    //{
    //    String ani1 = "", ani2 = "";
    //    if (args.Count >= 2)
    //    {
    //        ani1 = args[0].ToString();
    //        ani2 = args[1].ToString();
    //    }
    //    if (ani1 == "" && ani2 == "")
    //    {
    //        Ped player = Game.Player.Character;
    //        foreach (Ped i in peds)
    //        {
    //            i.Task.ClearAllImmediately();
    //            i.Task.FollowToOffsetFromEntity(player, (player.ForwardVector * 2), -1, 10);
    //        }
    //    }
    //    while (!Natives.HasAnimDictLoaded(ani1))
    //    {
    //        Natives.RequestAnimDict(ani1);
    //        await BaseScript.Delay(100);
    //    }
    //    AnimationFlags flags = AnimationFlags.Loop | AnimationFlags.CancelableWithMovement;
    //    foreach (Ped i in peds)
    //    {
    //        i.Task.ClearAllImmediately();
    //        i.Task.PlayAnimation(ani1, ani2, -1, -1, flags);
    //    }

    //}
}
}
