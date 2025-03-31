using System.Diagnostics;
using System.Reactive;
using PicView.Avalonia.UI;
using ReactiveUI;

namespace PicView.Avalonia.Functions;

public static class FunctionsHelper
{
    /// <summary>
    ///     Creates a ReactiveCommand from a Task with built-in error handling.
    /// </summary>
    /// <param name="task">The task to execute when the command is invoked.</param>
    /// <param name="canExecute">An optional observable determining when the command can execute.</param>
    /// <returns>A ReactiveCommand with configured error handling.</returns>
    public static ReactiveCommand<Unit, Unit> CreateReactiveCommand(
        Func<Task> task,
        IObservable<bool>? canExecute = null)
    {
        var cmd = ReactiveCommand.CreateFromTask(task, canExecute);

        cmd.ThrownExceptions
            .Subscribe(ex =>
            {
                _ = TooltipHelper.ShowTooltipMessageAsync(ex.Message);
                Debug.WriteLine($"Error in command: {ex}");
            });

        return cmd;
    }

    /// <summary>
    ///     Creates a parameterized ReactiveCommand from a Task with built-in error handling.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter passed to the task.</typeparam>
    /// <param name="task">The task to execute when the command is invoked, accepting a parameter.</param>
    /// <param name="canExecute">An optional observable determining when the command can execute.</param>
    /// <returns>A ReactiveCommand with configured error handling that accepts a parameter.</returns>
    public static ReactiveCommand<TParam, Unit> CreateReactiveCommand<TParam>(
        Func<TParam, Task> task,
        IObservable<bool>? canExecute = null)
    {
        var cmd = ReactiveCommand.CreateFromTask(task, canExecute);

        cmd.ThrownExceptions
            .Subscribe(ex =>
            {
                _ = TooltipHelper.ShowTooltipMessageAsync(ex.Message);
                Debug.WriteLine($"Error in command: {ex}");
            });

        return cmd;
    }

    /// <summary>
    ///     Creates a ReactiveCommand from a synchronous action with built-in error handling.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">An optional observable determining when the command can execute.</param>
    /// <returns>A ReactiveCommand with configured error handling.</returns>
    public static ReactiveCommand<Unit, Unit> CreateReactiveCommand(
        Action execute,
        IObservable<bool>? canExecute = null)
    {
        var cmd = ReactiveCommand.Create(execute, canExecute);

        cmd.ThrownExceptions
            .Subscribe(ex =>
            {
                _ = TooltipHelper.ShowTooltipMessageAsync(ex.Message);
                Debug.WriteLine($"Error in command: {ex}");
            });

        return cmd;
    }

    /// <summary>
    ///     Creates a parameterized ReactiveCommand from a synchronous action with built-in error handling.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter passed to the action.</typeparam>
    /// <param name="execute">The action to execute when the command is invoked, accepting a parameter.</param>
    /// <param name="canExecute">An optional observable determining when the command can execute.</param>
    /// <returns>A ReactiveCommand with configured error handling that accepts a parameter.</returns>
    public static ReactiveCommand<TParam, Unit> CreateReactiveCommand<TParam>(
        Action<TParam> execute,
        IObservable<bool>? canExecute = null)
    {
        var cmd = ReactiveCommand.Create(execute, canExecute);

        cmd.ThrownExceptions
            .Subscribe(ex =>
            {
                _ = TooltipHelper.ShowTooltipMessageAsync(ex.Message);
                Debug.WriteLine($"Error in command: {ex}");
            });

        return cmd;
    }
}