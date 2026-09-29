
import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import dashboardService from "../services/dashboardService";
import "./Dashboard.css";

function Icon({ name, size = 20 }) {
  const icons = {
    dashboard: (
      <>
        <rect x="3" y="3" width="7" height="7" rx="1.5" />
        <rect x="14" y="3" width="7" height="7" rx="1.5" />
        <rect x="3" y="14" width="7" height="7" rx="1.5" />
        <rect x="14" y="14" width="7" height="7" rx="1.5" />
      </>
    ),

    tasks: (
      <>
        <rect x="4" y="3" width="16" height="18" rx="2" />
        <path d="M8 8h8M8 12h8M8 16h5" />
      </>
    ),

    teams: (
      <>
        <circle cx="9" cy="8" r="3" />
        <circle cx="17" cy="9" r="2.5" />
        <path d="M3.5 20c.5-3.5 2.5-5 5.5-5s5 1.5 5.5 5M15 14.5c2.8-.1 4.7 1.5 5.2 4.5" />
      </>
    ),

    users: (
      <>
        <circle cx="9" cy="8" r="3" />
        <circle cx="17" cy="9" r="2.5" />
        <path d="M3.5 20c.5-3.5 2.5-5 5.5-5s5 1.5 5.5 5" />
        <path d="M15 14.5c2.8-.1 4.7 1.5 5.2 4.5" />
      </>
    ),

    comments: (
      <>
        <path d="M4 5.5A2.5 2.5 0 0 1 6.5 3h11A2.5 2.5 0 0 1 20 5.5v8a2.5 2.5 0 0 1-2.5 2.5H11l-4.5 4v-4h0A2.5 2.5 0 0 1 4 13.5z" />
        <path d="M8 8h8M8 11.5h5" />
      </>
    ),

    notification: (
      <>
        <path d="M18 9a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9" />
        <path d="M10 21h4" />
      </>
    ),

    settings: (
      <>
        <circle cx="12" cy="12" r="3" />
        <path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-1.8 1.8-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.6V20h-2.6v-.1a1.7 1.7 0 0 0-1-1.6 1.7 1.7 0 0 0-1.9.3l-.1.1-1.8-1.8.1-.1a1.7 1.7 0 0 0 .3-1.9 1.7 1.7 0 0 0-1.6-1H6v-2.6h.1a1.7 1.7 0 0 0 1.6-1 1.7 1.7 0 0 0-.3-1.9l-.1-.1 1.8-1.8.1.1a1.7 1.7 0 0 0 1.9.3 1.7 1.7 0 0 0 1-1.6V5h2.6v.1a1.7 1.7 0 0 0 1 1.6 1.7 1.7 0 0 0 1.9-.3l.1-.1 1.8 1.8-.1.1a1.7 1.7 0 0 0-.3 1.9 1.7 1.7 0 0 0 1.6 1h.1v2.6h-.1a1.7 1.7 0 0 0-1.6 1z" />
      </>
    ),

    logout: (
      <>
        <path d="M10 4H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h4" />
        <path d="M14 8l4 4-4 4M18 12H9" />
      </>
    ),

    search: (
      <>
        <circle cx="10.5" cy="10.5" r="6.5" />
        <path d="m16 16 5 5" />
      </>
    ),

    check: <path d="m5 12 4 4L19 6" />,

    clock: (
      <>
        <circle cx="12" cy="12" r="9" />
        <path d="M12 7v5l3 2" />
      </>
    ),

    progress: (
      <>
        <circle cx="12" cy="12" r="9" />
        <path d="M12 3a9 9 0 0 1 8.5 6" />
      </>
    ),

    calendar: (
      <>
        <rect x="3" y="5" width="18" height="16" rx="2" />
        <path d="M16 3v4M8 3v4M3 10h18" />
      </>
    ),

    bolt: <path d="m13 2-9 12h7l-1 8 9-12h-7z" />,
  };

  return (
    <svg
      className="svg-icon"
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      {icons[name]}
    </svg>
  );
}

export default function Dashboard() {
  const navigate = useNavigate();

  const [userName, setUserName] = useState("User");
  const [userRole, setUserRole] = useState("User");

  const [dashboardData, setDashboardData] = useState(null);
  const [activeFilter, setActiveFilter] = useState("All");
  const [searchText, setSearchText] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  /*
   * ---------------------------------------------------------
   * GET USER INFORMATION + DASHBOARD DATA
   * ---------------------------------------------------------
   */
  useEffect(() => {
    const token = localStorage.getItem("accessToken");

    if (!token) {
      navigate("/login");
      return;
    }

    try {
      const payload = JSON.parse(atob(token.split(".")[1]));

      const role =
        payload.role ||
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] ||
        "User";

      const name =
        payload.name ||
        payload[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
        ] ||
        payload.email ||
        "User";

      setUserRole(role);
      setUserName(name);
    } catch {
      setUserRole("User");
      setUserName("User");
    }

    loadDashboard();
  }, [navigate]);

  /*
   * ---------------------------------------------------------
   * LOAD DASHBOARD
   * ---------------------------------------------------------
   */
  const loadDashboard = async (filter = "All") => {
    try {
      setLoading(true);
      setError("");

      const filters = {};

      if (filter === "To Do") {
        filters.status = "ToDo";
      }

      if (filter === "In Progress") {
        filters.status = "InProgress";
      }

      if (filter === "Done") {
        filters.status = "Done";
      }

      const data = await dashboardService.getDashboard(filters);

      console.log(
        "DASHBOARD API RESPONSE:",
        JSON.stringify(data, null, 2)
      );

      setDashboardData(data);
    } catch (error) {
      console.error("DASHBOARD API ERROR:", error);
      setError("Unable to load dashboard data.");
    } finally {
      setLoading(false);
    }
  };

  /*
   * ---------------------------------------------------------
   * ROLE
   * ---------------------------------------------------------
   */
  const canManage =
    userRole === "Admin" || userRole === "Manager";

  /*
   * ---------------------------------------------------------
   * LOGOUT
   * ---------------------------------------------------------
   */
  const handleLogout = () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    navigate("/login");
  };

  /*
   * ---------------------------------------------------------
   * FILTER
   * ---------------------------------------------------------
   */
  const handleFilter = (filter) => {
    setActiveFilter(filter);
    loadDashboard(filter);
  };

  /*
   * ---------------------------------------------------------
   * STATUS / PRIORITY HELPERS
   * ---------------------------------------------------------
   */
  const getStatusText = (status) => {
    switch (Number(status)) {
      case 1:
        return "To Do";

      case 2:
        return "In Progress";

      case 3:
        return "Completed";

      default:
        return "Unknown";
    }
  };

  const getStatusClass = (status) => {
    switch (Number(status)) {
      case 1:
        return "todo";

      case 2:
        return "progress-status";

      case 3:
        return "completed";

      default:
        return "todo";
    }
  };

  const getPriorityText = (priority) => {
    switch (Number(priority)) {
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

  const getPriorityClass = (priority) => {
    switch (Number(priority)) {
      case 1:
        return "low";

      case 2:
        return "medium";

      case 3:
        return "high";

      default:
        return "low";
    }
  };

  /*
   * ---------------------------------------------------------
   * DATE HELPERS
   * ---------------------------------------------------------
   */
  const formatDueDate = (dueDate, status) => {
    if (!dueDate) {
      return "No date";
    }

    const due = new Date(dueDate);
    const today = new Date();

    const dueDateOnly = new Date(
      due.getFullYear(),
      due.getMonth(),
      due.getDate()
    );

    const todayOnly = new Date(
      today.getFullYear(),
      today.getMonth(),
      today.getDate()
    );

    const tomorrowOnly = new Date(todayOnly);
    tomorrowOnly.setDate(todayOnly.getDate() + 1);

    if (Number(status) !== 3 && dueDateOnly < todayOnly) {
      return "Overdue";
    }

    if (dueDateOnly.getTime() === todayOnly.getTime()) {
      return "Today";
    }

    if (dueDateOnly.getTime() === tomorrowOnly.getTime()) {
      return "Tomorrow";
    }

    return due.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
    });
  };

  const isOverdue = (dueDate, status) => {
    if (!dueDate || Number(status) === 3) {
      return false;
    }

    const due = new Date(dueDate);
    const today = new Date();

    due.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);

    return due < today;
  };

  /*
   * ---------------------------------------------------------
   * TASKS
   * ---------------------------------------------------------
   */
  const allTasks = dashboardData?.tasks || [];

  const filteredTasks = useMemo(() => {
    if (!searchText.trim()) {
      return allTasks;
    }

    const search = searchText.toLowerCase();

    return allTasks.filter(
      (task) =>
        task.title?.toLowerCase().includes(search) ||
        task.teamName?.toLowerCase().includes(search)
    );
  }, [allTasks, searchText]);

  /*
   * ---------------------------------------------------------
   * COMPLETION %
   * ---------------------------------------------------------
   */
  const totalTasks = dashboardData?.totalTasks || 0;
  const completedTasks = dashboardData?.completedTasks || 0;

  const completionPercentage =
    totalTasks > 0
      ? Math.round((completedTasks / totalTasks) * 100)
      : 0;

  /*
   * ---------------------------------------------------------
   * MENU
   * ---------------------------------------------------------
   */
  const menuItems = [
  {
    label: "Dashboard",
    icon: "dashboard",
    path: "/dashboard",
  },

  {
    label: "My Tasks",
    icon: "tasks",
    path: "/tasks",
  },

  ...(canManage
    ? [
        {
          label: "Teams",
          icon: "teams",
          path: "/teams",
        },
      ]
    : []),

  {
    label: "Comments",
    icon: "comments",
    path: "/comments",
  },

  {
    label: "Notifications",
    icon: "notification",
    path: "/notifications",
  },
];

  /*
   * ---------------------------------------------------------
   * TODAY
   * ---------------------------------------------------------
   */
  const todayText = new Date().toLocaleDateString("en-US", {
    weekday: "short",
    day: "numeric",
    month: "short",
    year: "numeric",
  });

  /*
   * ---------------------------------------------------------
   * UI
   * ---------------------------------------------------------
   */
  return (
    <div className="app-shell">

      {/* =====================================================
          SIDEBAR
      ====================================================== */}

      <aside className="sidebar">

        <div className="sidebar-brand">

          <div className="brand-mark">
            <Icon name="bolt" size={22} />
          </div>

          <div>
            <div className="brand-name">
              TaskFlow
            </div>

            <div className="brand-subtitle">
              Team Management
            </div>
          </div>

        </div>

        <div className="sidebar-section-title">
          WORKSPACE
        </div>

        <nav className="sidebar-menu">

          {menuItems.map((item) => (
            <button
              key={item.label}
              className={`menu-item ${
                item.label === "Dashboard"
                  ? "active"
                  : ""
              }`}
              onClick={() => navigate(item.path)}
            >
              <span className="menu-icon">
                <Icon name={item.icon} size={19} />
              </span>

              <span className="menu-label">
                {item.label}
              </span>
            </button>
          ))}

        </nav>

        {userRole === "Admin" && (
          <>
            

            <nav className="sidebar-menu">
              <button
                className="menu-item"
                onClick={() => navigate("/users")}
              >
                <span className="menu-icon">
                  <Icon name="users" size={19} />
                </span>

                <span className="menu-label">
                  Users
                </span>
              </button>
            </nav>
          </>
        )}

        <div className="sidebar-section-title account-title">
          ACCOUNT
        </div>

        <nav className="sidebar-menu">

          <button
            className="menu-item"
            onClick={() => navigate("/settings")}
          >
            <span className="menu-icon">
              <Icon name="settings" size={19} />
            </span>

            <span className="menu-label">
              Settings
            </span>
          </button>

        </nav>

        <div className="sidebar-bottom">

          <div className="sidebar-profile">

            <div className="sidebar-avatar">
              {userName.charAt(0).toUpperCase()}
            </div>

            <div className="sidebar-profile-info">

              <strong>
                {userName}
              </strong>

              <span>
                {userRole}
              </span>

            </div>

          </div>

          <button
            className="logout-button"
            onClick={handleLogout}
          >
            <Icon name="logout" size={18} />
            <span>
              Logout
            </span>
          </button>

        </div>

      </aside>

      {/* =====================================================
          MAIN
      ====================================================== */}

      <main className="main-content">

        {/* HEADER */}

        <header className="top-header">

          <div className="search-box">

            <Icon name="search" size={18} />

            <input
              placeholder="Search tasks, teams..."
              value={searchText}
              onChange={(e) =>
                setSearchText(e.target.value)
              }
            />

          </div>

          <div className="header-actions">

            <button
              className="header-notification"
              onClick={() =>
                navigate("/notifications")
              }
            >
              <Icon
                name="notification"
                size={20}
              />

              <span className="notification-dot"></span>
            </button>

            <div className="header-line"></div>

            <div className="header-user">

              <div className="header-avatar">
                {userName.charAt(0).toUpperCase()}
              </div>

              <div className="header-user-details">

                <strong>
                  {userName}
                </strong>

              </div>

            </div>

          </div>

        </header>

        <div className="dashboard-container">

          {/* =================================================
              HERO
          ================================================== */}

          <section className="hero-card">

            <div className="hero-content">

              <div className="hero-title">
                Welcome back, {userName}{" "}
                <span>👋</span>
              </div>

              <p>
                Here's what's happening across your
                workspace today.
              </p>

              <div className="hero-meta">

                <span>
                  <Icon
                    name="calendar"
                    size={15}
                  />

                  {todayText}
                </span>

                <span className="hero-separator"></span>

                <span>
                  {userRole}
                </span>

              </div>

            </div>

            <div className="hero-decoration">

              <div className="deco-circle one"></div>
              <div className="deco-circle two"></div>

              <div className="deco-document">

                <div className="document-check">
                  <Icon
                    name="check"
                    size={32}
                  />
                </div>

                <div className="document-line line-long"></div>
                <div className="document-line line-medium"></div>
                <div className="document-line line-short"></div>

                <div className="document-status">
                  <span></span>
                  Done
                </div>

              </div>

            </div>

          </section>

          {/* =================================================
              LOADING / ERROR
          ================================================== */}

          {loading && (
            <div className="panel">
              <div style={{ padding: "30px", textAlign: "center" }}>
                Loading dashboard...
              </div>
            </div>
          )}

          {error && !loading && (
            <div className="panel">
              <div style={{ padding: "30px", textAlign: "center" }}>
                {error}
              </div>
            </div>
          )}

          {!loading && !error && (
            <>
              {/* =============================================
                  STATS
              ============================================== */}

              <section className="stats-row">

                {/* TOTAL */}

                <div className="stat-card blue-card">

                  <div className="stat-top">

                    <div>

                      <span className="stat-label">
                        Total Tasks
                      </span>

                      <strong className="stat-value">
                        {dashboardData?.totalTasks || 0}
                      </strong>

                    </div>

                    <div className="stat-symbol">
                      <Icon
                        name="tasks"
                        size={23}
                      />
                    </div>

                  </div>

                  <div className="stat-growth">
                    <span>
                      {dashboardData?.toDoTasks || 0}
                    </span>{" "}
                    to do
                  </div>

                </div>

                {/* IN PROGRESS */}

                <div className="stat-card orange-card">

                  <div className="stat-top">

                    <div>

                      <span className="stat-label">
                        In Progress
                      </span>

                      <strong className="stat-value">
                        {dashboardData?.inProgressTasks || 0}
                      </strong>

                    </div>

                    <div className="stat-symbol">
                      <Icon
                        name="progress"
                        size={23}
                      />
                    </div>

                  </div>

                  <div className="stat-growth">
                    <span>
                      {dashboardData?.highPriorityTasks || 0}
                    </span>{" "}
                    high priority
                  </div>

                </div>

                {/* OVERDUE */}

                <div className="stat-card purple-card">

                  <div className="stat-top">

                    <div>

                      <span className="stat-label">
                        Overdue
                      </span>

                      <strong className="stat-value">
                        {dashboardData?.overdueTasks || 0}
                      </strong>

                    </div>

                    <div className="stat-symbol">
                      <Icon
                        name="clock"
                        size={23}
                      />
                    </div>

                  </div>

                  <div className="stat-growth">
                    <span>
                      {dashboardData?.highPriorityTasks || 0}
                    </span>{" "}
                    high priority
                  </div>

                </div>

                {/* COMPLETED */}

                <div className="stat-card green-card">

                  <div className="stat-top">

                    <div>

                      <span className="stat-label">
                        Completed
                      </span>

                      <strong className="stat-value">
                        {dashboardData?.completedTasks || 0}
                      </strong>

                    </div>

                    <div className="stat-symbol">
                      <Icon
                        name="check"
                        size={23}
                      />
                    </div>

                  </div>

                  <div className="stat-growth">
                    <span>
                      {completionPercentage}%
                    </span>{" "}
                    completion
                  </div>

                </div>

              </section>

              {/* =============================================
                  MAIN CONTENT
              ============================================== */}

              <section className="content-grid">

                {/* ===========================================
                    MY TASKS
                ============================================ */}

                <div className="panel tasks-panel">

                  <div className="panel-header">

                    <div>

                      <h2>
                        My Tasks
                      </h2>

                      <p>
                        Tasks assigned to you
                      </p>

                    </div>

                    <button
                      className="view-link"
                      onClick={() =>
                        navigate("/tasks")
                      }
                    >
                      View all
                    </button>

                  </div>

                  {/* FILTERS */}

                  <div className="task-filters">

                    {[
                      "All",
                      "To Do",
                      "In Progress",
                      "Done",
                    ].map((filter) => (
                      <button
                        key={filter}
                        className={`filter ${
                          activeFilter === filter
                            ? "active"
                            : ""
                        }`}
                        onClick={() =>
                          handleFilter(filter)
                        }
                      >
                        {filter}
                      </button>
                    ))}

                  </div>

                  {/* TASK TABLE */}

                  <div className="task-table">

                    <div className="task-row task-header">

                      <span>
                        Task
                      </span>

                      <span>
                        Status
                      </span>

                      <span>
                        Priority
                      </span>

                      <span>
                        Due
                      </span>

                    </div>

                    {filteredTasks.length === 0 && (
                      <div
                        style={{
                          padding: "30px",
                          textAlign: "center",
                        }}
                      >
                        No tasks found.
                      </div>
                    )}

                    {filteredTasks.map((task) => {

                      const overdue = isOverdue(
                        task.dueDate,
                        task.status
                      );

                      const statusClass =
                        getStatusClass(
                          task.status
                        );

                      const priorityClass =
                        getPriorityClass(
                          task.priority
                        );

                      return (
                        <div
                          className="task-row"
                          key={task.id}
                        >

                          <div className="task-name">

                            <span
                              className={`task-dot ${
                                statusClass ===
                                "completed"
                                  ? "green"
                                  : statusClass ===
                                    "progress-status"
                                  ? "orange"
                                  : "purple"
                              }`}
                            ></span>

                            <div>
                              <strong>
                                {task.title}
                              </strong>

                              {task.teamName && (
                                <small
                                  style={{
                                    display: "block",
                                    marginTop: "3px",
                                    opacity: 0.65,
                                  }}
                                >
                                  {task.teamName}
                                </small>
                              )}
                            </div>

                          </div>

                          <span
                            className={`status ${statusClass}`}
                          >
                            {getStatusText(
                              task.status
                            )}
                          </span>

                          <span
                            className={`priority ${priorityClass}`}
                          >
                            {getPriorityText(
                              task.priority
                            )}
                          </span>

                          <span
                            className={`due ${
                              overdue
                                ? "danger"
                                : ""
                            }`}
                          >
                            {formatDueDate(
                              task.dueDate,
                              task.status
                            )}
                          </span>

                        </div>
                      );
                    })}

                  </div>

                </div>

                {/* =========================================
                    RIGHT COLUMN
                ========================================== */}

                <div className="right-content">

                  {/* =======================================
                      PROGRESS
                  ======================================== */}

                  <div className="panel progress-panel">

                    <div className="panel-header">

                      <div>

                        <h2>
                          Task Progress
                        </h2>

                        <p>
                          Your overall task completion
                        </p>

                      </div>

                      <span className="view-link">
                        Overall
                      </span>

                    </div>

                    <div className="progress-content">

                      <div
                        className="donut"
                        style={{
                          background: `conic-gradient(
                            var(--success-color, #22c55e) ${
                              completionPercentage * 3.6
                            }deg,
                            #eef2f7 ${
                              completionPercentage * 3.6
                            }deg
                          )`,
                        }}
                      >

                        <div className="donut-inner">

                          <strong>
                            {completionPercentage}%
                          </strong>

                          <span>
                            Complete
                          </span>

                        </div>

                      </div>

                      <div className="progress-legend">

                        <div>
                          <span className="legend-dot legend-blue"></span>
                          <span>
                            To Do
                          </span>
                          <strong>
                            {dashboardData?.toDoTasks || 0}
                          </strong>
                        </div>

                        <div>
                          <span className="legend-dot legend-purple"></span>
                          <span>
                            In Progress
                          </span>
                          <strong>
                            {dashboardData?.inProgressTasks || 0}
                          </strong>
                        </div>

                        <div>
                          <span className="legend-dot legend-green"></span>
                          <span>
                            Completed
                          </span>
                          <strong>
                            {dashboardData?.completedTasks || 0}
                          </strong>
                        </div>

                      </div>

                    </div>

                  </div>

                  {/* =======================================
                      RECENT + NOTIFICATIONS
                  ======================================== */}

                  <div className="small-panels">

                    {/* RECENT ACTIVITY */}

                    <div className="panel small-panel">

                      <div className="small-panel-header">

                        <h3 className="small-title">
                          Recent Activity
                        </h3>

                        <button
                          className="small-view"
                          onClick={() =>
                            navigate("/tasks")
                          }
                        >
                          View all
                        </button>

                      </div>

                      <div className="activity-list">

                        <div className="activity-item">

                          <div className="activity-avatar purple-avatar">
                            {userName
                              .charAt(0)
                              .toUpperCase()}
                          </div>

                          <div>

                            <strong>
                              {userName}
                            </strong>

                            <p>
                              Dashboard loaded
                            </p>

                            <span>
                              Just now
                            </span>

                          </div>

                        </div>

                        {dashboardData?.tasks
                          ?.slice(0, 2)
                          .map((task) => (
                            <div
                              className="activity-item"
                              key={`activity-${task.id}`}
                            >

                              <div className="activity-avatar blue-avatar">
                                {task.title
                                  ?.charAt(0)
                                  .toUpperCase()}
                              </div>

                              <div>

                                <strong>
                                  {task.title}
                                </strong>

                                <p>
                                  {getStatusText(
                                    task.status
                                  )}
                                </p>

                                <span>
                                  Due{" "}
                                  {formatDueDate(
                                    task.dueDate,
                                    task.status
                                  )}
                                </span>

                              </div>

                            </div>
                          ))}

                      </div>

                    </div>

                    {/* NOTIFICATIONS */}

                    <div className="panel small-panel">

                      <div className="small-panel-header">

                        <h3 className="small-title">
                          Notifications
                        </h3>

                        <button
                          className="small-view"
                          onClick={() =>
                            navigate(
                              "/notifications"
                            )
                          }
                        >
                          View all
                        </button>

                      </div>

                      <div className="notification-list">

                        <div className="notification-item">

                          <div className="notification-icon notification-blue">
                            <Icon
                              name="tasks"
                              size={16}
                            />
                          </div>

                          <div>

                            <strong>
                              Task Overview
                            </strong>

                            <p>
                              You have{" "}
                              {dashboardData?.totalTasks ||
                                0}{" "}
                              assigned tasks.
                            </p>

                          </div>

                        </div>

                        <div className="notification-item">

                          <div className="notification-icon notification-purple">
                            <Icon
                              name="clock"
                              size={16}
                            />
                          </div>

                          <div>

                            <strong>
                              Due Soon
                            </strong>

                            <p>
                              {
                                dashboardData?.inProgressTasks ||
                                  0
                              }{" "}
                              task(s) currently in progress.
                            </p>

                          </div>

                        </div>

                        <div className="notification-item">

                          <div className="notification-icon notification-green">
                            <Icon
                              name="check"
                              size={16}
                            />
                          </div>

                          <div>

                            <strong>
                              Completed
                            </strong>

                            <p>
                              {
                                dashboardData?.completedTasks ||
                                  0
                              }{" "}
                              task(s) completed.
                            </p>

                          </div>

                        </div>

                      </div>

                    </div>

                  </div>

                </div>

              </section>
            </>
          )}

        </div>

      </main>

    </div>
  );
}
