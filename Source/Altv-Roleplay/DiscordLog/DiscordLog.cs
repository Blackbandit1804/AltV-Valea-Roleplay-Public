using Discord.Webhook;
using Discord.Webhook.HookRequest;

namespace Altv_Roleplay.Handler
{
    class DiscordLog
    {
        internal static void SendEmbed(string type, string nickname, string text)
        {
            DiscordWebhook hook = new DiscordWebhook();

            switch (type)
            {
                case "adminmenu":
                    hook.HookUrl = "";
                    break;
                case "Command":
                    hook.HookUrl = "";
                    break;
                case "report":
                    hook.HookUrl = "";
                    break;
                case "geldlog":
                    hook.HookUrl = "";
                    break;
                case "itemlog":
                    hook.HookUrl = "";
                    break;
                case "kofferraum":
                    hook.HookUrl = "";
                    break;
                case "frakbank":
                    hook.HookUrl = "";
                    break;
                case "housestorage":
                    hook.HookUrl = "";
                    break;
                case "ooc":
                    hook.HookUrl = "";
                    break;
                case "death":
                    hook.HookUrl = "";
                    break;
                case "ban":
                    hook.HookUrl = "";
                    break;
                case "kick":
                    hook.HookUrl = "";
                    break;
                case "sus":
                    hook.HookUrl = "";
                    break;
                case "event":
                    hook.HookUrl = "";
                    break;
                case "serverstatus":
                    hook.HookUrl = "";
                    break;
                default:
                    hook.HookUrl = "";
                    break;
            }

            if (hook.HookUrl == "") return; //Hier WEB_HOOK nicht ersetzen

            DiscordHookBuilder builder = DiscordHookBuilder.Create(Nickname: nickname, AvatarUrl: "https://cdn.discordapp.com/attachments/865902854652821514/866305404949757952/CGRP_-_Discord1.png");

            DiscordEmbed embed = new DiscordEmbed(
            Title: "Valea - Logs",
            Description: text,
            Color: 0xf54242,
            FooterText: "Valea- Logs",
            FooterIconUrl: "https://cdn.discordapp.com/icons/842873686104211486/c10d4214cbd9b2d575597ab48b68ad40.png?size=96");
            builder.Embeds.Add(embed);

            DiscordHook HookMessage = builder.Build();
            hook.Hook(HookMessage);
        }
    }
}
