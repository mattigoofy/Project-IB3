namespace WagentjeApp.Models
{
    public class Traject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TrajectCommand> Commands { get; set; }
    }

    public class TrajectCommand
    {
        public string Action { get; set; }
        public int Duration { get; set; }
        public string Name { get; set; }

        public TrajectCommand(string action, int duration, string name)
        {
            Action = action;
            Duration = duration;
            Name = name;
        }
    }
}
