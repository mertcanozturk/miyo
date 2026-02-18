using System.Collections.Generic;

namespace Miyo.Core.Commands
{
    public class CommandHistory
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly int _maxHistory;

        public int Count => _undoStack.Count;
        public bool CanUndo => _undoStack.Count > 0;

        public CommandHistory(int maxHistory = 20)
        {
            _maxHistory = maxHistory;
        }

        public void Execute(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);

            if (_undoStack.Count > _maxHistory)
                TrimOldest();
        }

        public void Undo()
        {
            if (_undoStack.TryPop(out var command))
                command.Undo();
        }

        public void Clear()
        {
            _undoStack.Clear();
        }

        private void TrimOldest()
        {
            var temp = new Stack<ICommand>();
            while (_undoStack.Count > _maxHistory)
            {
                temp.Push(_undoStack.Pop());
            }
            _undoStack.Clear();
            while (temp.Count > 0)
            {
                _undoStack.Push(temp.Pop());
            }
        }
    }
}
