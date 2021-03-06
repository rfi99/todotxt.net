﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ToDoLib
{
    /// <summary>
    /// A thin data access abstraction over the actual todo.txt file
    /// </summary>
    public class TaskList
    {
        // It may look like an overly simple approach has been taken here, but it's well considered. This class
        // represents *the file itself* - when you call a method it should be as though you directly edited the file.
        // This reduces the liklihood of concurrent update conflicts by making each action as autonomous as possible.
        // Although this does lead to some extra IO, it's a small price for maintaining the integrity of the file.

        // NB, this is not the place for higher-level functions like searching, task manipulation etc. It's simply 
        // for CRUDing the todo.txt file. 
        
        List<Task> _tasks;
        string _filePath;

        public List<Task> Tasks { get { return _tasks; } }

        public TaskList(string filePath)
        {
            _filePath = filePath;
            ReloadTasks();
        }

        public void ReloadTasks()
        {
            Log.Debug("Loading tasks from {0}", _filePath);

            try
            {
                _tasks = new List<Task>();
                foreach (var line in File.ReadAllLines(_filePath))
                    _tasks.Add(new Task(line));

                Log.Debug("Finished loading tasks from {0}", _filePath);
            }
            catch (IOException ex)
            {
                var msg = "There was a problem trying to read from your todo.txt file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public void Add(Task task)
        {
            try
            {
                var output = task.ToString();

                Log.Debug("Adding task '{0}'", output);

                var text = File.ReadAllText(_filePath);
                if (text.Length > 0 && !text.EndsWith(Environment.NewLine))
                    output = Environment.NewLine + output;

                File.AppendAllLines(_filePath, new string[] { output });

                Log.Debug("Task '{0}' added", output);

                ReloadTasks();
            }
            catch (IOException ex)
            {
                var msg = "An error occurred while trying to add your task to the task list file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }

        }

        public void Delete(Task task)
        {
            try
            {
                Log.Debug("Deleting task '{0}'", task.ToString());

                ReloadTasks(); // make sure we're working on the latest file
                
                if (_tasks.Remove(_tasks.First(t => t.Raw == task.Raw)))
                    File.WriteAllLines(_filePath, _tasks.Select(t => t.ToString()));
                
                Log.Debug("Task '{0}' deleted", task.ToString());

                ReloadTasks();
            }
            catch (IOException ex)
            {
                var msg = "An error occurred while trying to remove your task from the task list file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

      
        public void Update(Task currentTask, Task newTask)
        {
            try
            {
                Log.Debug("Updating task '{0}' to '{1}'", currentTask.ToString(), newTask.ToString());

                ReloadTasks();
                var currentIndex = _tasks.IndexOf(_tasks.First(t => t.Raw == currentTask.Raw));

                _tasks[currentIndex] = newTask;

                File.WriteAllLines(_filePath, _tasks.Select(t => t.ToString()));

                Log.Debug("Task '{0}' updated", currentTask.ToString());

                ReloadTasks();
            }
            catch (IOException ex)
            {
                var msg = "An error occurred while trying to update your task int the task list file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }
    }
}
