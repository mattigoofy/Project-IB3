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
        public double Duration { get; set; }
        public string Name { get; set; }
        public int Speed { get; set; }

        public TrajectCommand(string action, double duration, string name, int speed)
        {
            Action = action;
            Duration = duration;
            Name = name;
            Speed = speed;
        }
    }
}
