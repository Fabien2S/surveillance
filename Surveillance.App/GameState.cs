﻿namespace Surveillance.App
{
    public struct GameState
    {
        public GameCharacter Character { get; set; }
        public GameAction Action { get; set; }
        public string Details { get; set; }
        public string[] Triggers { get; set; }
        
    }

    public struct GameCharacter
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public struct GameAction
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }
}