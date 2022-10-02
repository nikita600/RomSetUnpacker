namespace RomSetUnpacker.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }

        public abstract string Description { get; }
    }
}