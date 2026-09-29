using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.DTOs.Dashboard
{
    public class DashboardResponse
    {
        public int TotalTasks { get; set; }

        public int ToDoTasks { get; set; }

        public int InProgressTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int HighPriorityTasks { get; set; }

        public int MediumPriorityTasks { get; set; }

        public int LowPriorityTasks { get; set; }

        public int OverdueTasks { get; set; }

        public List<DashboardTaskResponse> Tasks { get; set; } = new();
    }

    public class DashboardTaskResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public TaskItemStatus Status { get; set; }

        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public int AssignedToId { get; set; }

        public string AssignedToName { get; set; } = string.Empty;

        public int TeamId { get; set; }

        public string TeamName { get; set; } = string.Empty;
    }
}