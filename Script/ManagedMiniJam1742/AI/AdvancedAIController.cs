using ManagedMiniJam1742.AI.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;

namespace ManagedMiniJam1742.AI;

[UClass]
public partial class AAdvancedAIController : AAIController
{
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial TSubclassOf<UMyAITask> DefaultTask { get; set; }

    private Queue<UMyAITask> currentTasks = [];

    private UMyAITask currentTask;

    public override void BeginPlay()
    {
        base.BeginPlay();

        if (!DefaultTask.IsValid)
        {
            DefaultTask = typeof(UIdleTask);
        }
    }

    public Task CreateTask<Task>() where Task : UMyAITask
    {
        var task = NewObject<Task>(this);
        task.Controller = this;
        return task;
    }

    public void AddTask(UMyAITask task)
    {
        currentTasks.Enqueue(task);
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        if (currentTasks.Count <= 0)
        {
            var defaultTask = NewObject(this, DefaultTask);
            defaultTask.Controller = this;
            currentTasks.Enqueue(defaultTask);
        }

        if (currentTask == null || currentTask.Finished)
        {
            if (currentTask != null && currentTask.ContinueTask != null)
            {
                currentTask = currentTask.ContinueTask;
            }
            else
            {
                if (currentTasks.TryDequeue(out UMyAITask task))
                {
                    currentTask = task;
                }
            }
        }

        if (currentTask != null && !currentTask.Finished)
        {
            if (!currentTask.HasStarted)
            {
                currentTask.Start();
            }

            currentTask.Tick(deltaSeconds);
        }
    }

    public void StopTasks()
    {
        currentTask.Complete();
        currentTasks.Clear();
    }

    public bool HasCurrentTask<T>()
    {
        return currentTask.GetType() == typeof(T) || 
            currentTasks.Any(t => t.GetType() == typeof(T));
    }
}
