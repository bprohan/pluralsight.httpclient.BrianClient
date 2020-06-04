using System;

namespace BrianClient
{
    public class GuidService
    {
        private readonly Guid ServiceGuid;
        public GuidService()
        {
            ServiceGuid = new Guid();
        }

        public string GetGuid => ServiceGuid.ToString();
    }
}
