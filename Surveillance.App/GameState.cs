namespace Surveillance.App
{
    public struct GameState
    {
        public string Character { get; set; }
        public string CharacterIcon { get; set; }
        
        public string Action { get; set; }
        public string ActionIcon { get; set; }

        public string[] Triggers { get; set; }
    }
}