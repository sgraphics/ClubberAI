using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubberAI.ServiceDefaults.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Category
    {
        public string name { get; set; }
        public List<Group> groups { get; set; }
    }

    public class Channel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string playlist { get; set; }
        public string emoji { get; set; }
    }

    public class Group
    {
        public int id { get; set; }
        public string name { get; set; }
        public string playlist { get; set; }
        public List<Channel> channels { get; set; }
    }

    public class GetChannelsResponse
    {
        public ChannelsData data { get; set; }
    }
    public class ChannelsData
    {
        public List<Category> categories { get; set; }
    }
}
