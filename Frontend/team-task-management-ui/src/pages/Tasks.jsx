import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";
import "./Tasks.css";

function Tasks() {
  const navigate = useNavigate();

  const [tasks, setTasks] = useState([]);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [priorityFilter, setPriorityFilter] = useState("All");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // User role
  const [userRole, setUserRole] = useState("User");

  const getStatusName = (status) => {
    switch (status) {
      case 1:
        return "To Do";
      case 2:
        return "In Progress";
      case 3:
        return "Done";
      default:
        return "Unknown";
    }
  };

  const getPriorityName = (priority) => {
    switch (priority) {
      case 1:
        return "Low";
      case 2:
        return "Medium";
      case 3:
        return "High";
      default:
        return "Unknown";
    }
  };

  const formatDate = (date) => {
    if (!date) return "-";

    return new Date(date).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const loadTasks = async () => {
    try {
      setLoading(true);
      setError("");

      const response = await api.get("/Tasks");

      setTasks(response.data || []);
    } catch (err) {
      console.error("Failed to load tasks:", err);

      setError(
        err.response?.data?.message ||
          "Unable to load your tasks. Please try again."
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const token = localStorage.getItem("accessToken");

    if (!token) {
      navigate("/login");
      return;
    }

    // Read role from JWT
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));

      const role =
        payload.role ||
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] ||
        "User";

      setUserRole(role);
    } catch (error) {
      console.error("Unable to read user role:", error);
      setUserRole("User");
    }

    loadTasks();
  }, [navigate]);

  // Admin and Manager can create tasks
  const canManage =
    userRole === "Admin" || userRole === "Manager";

  const filteredTasks = useMemo(() => {
    return tasks.filter((task) => {
      const statusName = getStatusName(task.status);
      const priorityName = getPriorityName(task.priority);

      const matchesSearch =
        task.title?.toLowerCase().includes(search.toLowerCase()) ||
        task.description?.toLowerCase().includes(search.toLowerCase()) ||
        task.team?.name?.toLowerCase().includes(search.toLowerCase()) ||
        task.teamName?.toLowerCase().includes(search.toLowerCase());

      const matchesStatus =
        statusFilter === "All" || statusName === statusFilter;

      const matchesPriority =
        priorityFilter === "All" || priorityName === priorityFilter;

      return matchesSearch && matchesStatus && matchesPriority;
    });
  }, [tasks, search, statusFilter, priorityFilter]);

  return (
    <div className="tasks-page">
      <div className="tasks-header">
        <div>
          <button
            type="button"
            className="back-button"
            onClick={() => navigate("/dashboard")}
          >
            ← Back
          </button>

          <h1>My Tasks</h1>

          <p>
            View and manage tasks assigned to you.
          </p>
        </div>

        <div className="tasks-header-actions">
          {canManage && (
            <button
              type="button"
              className="create-task-button"
              onClick={() => navigate("/tasks/create")}
            >
              + Create Task
            </button>
          )}

          <button
            className="tasks-refresh-button"
            onClick={loadTasks}
            type="button"
          >
            ↻ Refresh
          </button>
        </div>
      </div>

      <div className="tasks-toolbar">
        <div className="tasks-search">
          <span>⌕</span>

          <input
            type="text"
            placeholder="Search tasks..."
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </div>

        <select
          value={statusFilter}
          onChange={(event) =>
            setStatusFilter(event.target.value)
          }
        >
          <option value="All">All Status</option>
          <option value="To Do">To Do</option>
          <option value="In Progress">In Progress</option>
          <option value="Done">Done</option>
        </select>

        <select
          value={priorityFilter}
          onChange={(event) =>
            setPriorityFilter(event.target.value)
          }
        >
          <option value="All">All Priority</option>
          <option value="High">High</option>
          <option value="Medium">Medium</option>
          <option value="Low">Low</option>
        </select>
      </div>

      {loading && (
        <div className="tasks-state">
          <div className="tasks-spinner"></div>
          <p>Loading your tasks...</p>
        </div>
      )}

      {!loading && error && (
        <div className="tasks-state tasks-error">
          <h3>Something went wrong</h3>

          <p>{error}</p>

          <button
            type="button"
            onClick={loadTasks}
          >
            Try Again
          </button>
        </div>
      )}

      {!loading &&
        !error &&
        filteredTasks.length === 0 && (
          <div className="tasks-state">
            <div className="empty-icon">✓</div>

            <h3>No tasks found</h3>

            <p>
              {search ||
              statusFilter !== "All" ||
              priorityFilter !== "All"
                ? "Try changing your filters."
                : "You currently have no tasks assigned to you."}
            </p>
          </div>
        )}

      {!loading &&
        !error &&
        filteredTasks.length > 0 && (
          <div className="tasks-card">
            <div className="tasks-card-header">
              <div>
                <h2>Assigned Tasks</h2>

                <span>
                  {filteredTasks.length} task
                  {filteredTasks.length !== 1
                    ? "s"
                    : ""}
                </span>
              </div>
            </div>

            <div className="tasks-table-wrapper">
              <table className="tasks-table">
                <thead>
                  <tr>
                    <th>Task</th>
                    <th>Team</th>
                    <th>Status</th>
                    <th>Priority</th>
                    <th>Due Date</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  {filteredTasks.map((task) => (
                    <tr key={task.id}>
                      <td>
                        <div className="task-title-cell">
                          <strong>{task.title}</strong>

                          {task.description && (
                            <span>
                              {task.description}
                            </span>
                          )}
                        </div>
                      </td>

                      <td>
                        {task.team?.name ||
                          task.teamName ||
                          "No team"}
                      </td>

                      <td>
                        <span
                          className={`task-status status-${task.status}`}
                        >
                          {getStatusName(task.status)}
                        </span>
                      </td>

                      <td>
                        <span
                          className={`task-priority priority-${task.priority}`}
                        >
                          {getPriorityName(task.priority)}
                        </span>
                      </td>

                      <td>
                        {formatDate(task.dueDate)}
                      </td>

                      <td>
                        <button
                          type="button"
                          className="task-view-button"
                          onClick={() =>
                            navigate(
                              `/tasks/${task.id}`
                            )
                          }
                        >
                          View
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
    </div>
  );
}

export default Tasks;