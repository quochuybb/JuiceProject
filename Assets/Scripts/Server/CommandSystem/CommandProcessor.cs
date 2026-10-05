using System.Collections.Generic;
using UnityEngine;

public class CommandProcessor
{
    private Queue<ICommand> commandQueue = new Queue<ICommand>();

    public void Enqueue(ICommand command)
    {
        commandQueue.Enqueue(command);
    }

    public void ExecuteAll(GameRoomContext context)
    {
        Debug.Log($"[CommandProcessor] Executing {commandQueue.Count} commands");

        while (commandQueue.Count > 0)
        {
            ICommand cmd = commandQueue.Dequeue();
            cmd.Execute(context);
        }
    }

    public void Clear()
    {
        commandQueue.Clear();
    }
}
