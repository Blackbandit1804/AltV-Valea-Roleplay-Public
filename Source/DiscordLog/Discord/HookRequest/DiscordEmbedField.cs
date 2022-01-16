using Newtonsoft.Json.Linq;


namespace Discord.Webhook.HookRequest
{
    public class DiscordEmbedField
    {
        public DiscordEmbedField(string Name, string Value, bool Line = true)
        {
            JObject fieldData = new JObject();
            fieldData.Add("name", Name);
            fieldData.Add("value", Value);
            fieldData.Add("inline", Line);

            JsonData = fieldData;
        }

        public JObject JsonData { get; private set; }
    }
}