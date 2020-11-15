using Newtonsoft.Json;

namespace satgen2
{
    public static class Extensions
    {
        public static string ToJSON(this object msg)
        {
            return JsonConvert.SerializeObject(msg, Formatting.Indented,new JsonSerializerSettings 
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}