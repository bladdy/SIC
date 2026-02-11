namespace SIC.Backend.Hubs
{
    public static class SignalRGroups
    {
        public static string Chat(string ownerPhone, string contactPhone)
            => $"chat-{ownerPhone}-{contactPhone}";

        public static string EventInbox(string ownerPhone, string eventCode)
            => $"event-inbox-{ownerPhone}-{eventCode}";
    }
}