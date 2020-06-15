namespace Surveillance.App
{
    public struct GameState
    {
        public GameCharacter Character { get; set; }
        public GameAction Action { get; set; }
        
        public string State { get; set; }
        public string Details { get; set; }
        
        public string[] Triggers { get; set; }

        public override string ToString()
        {
            return $"{nameof(Character)}: {Character}, {nameof(Action)}: {Action}, {nameof(Triggers)}: {Triggers}";
        }
    }

    public struct GameCharacter
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Name)}: {Name}";
        }
    }

    public struct GameAction
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Name)}: {Name}";
        }
    }
}