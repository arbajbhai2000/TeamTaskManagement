import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import notificationService from "../services/notificationService";
import "./Notifications.css";

const Notifications = () => {
  const navigate = useNavigate();

  const [notifications, setNotifications] = useState([]);
  const [filter, setFilter] = useState("all");
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [readingId, setReadingId] = useState(null);

  const loadNotifications = async () => {
    try {
      setLoading(true);
      setError("");

      const data =
        await notificationService.getMyNotifications();

      const notificationData = Array.isArray(data)
        ? data
        : data?.data ?? [];

      const sorted = [...notificationData].sort(
        (a, b) =>
          new Date(b.createdAt) -
          new Date(a.createdAt)
      );

      setNotifications(sorted);
    } catch (err) {
      console.error(
        "Failed to load notifications:",
        err
      );

      setError(
        err.response?.data?.message ||
          "Unable to load notifications. Please try again."
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadNotifications();
  }, []);

  const unreadCount = useMemo(() => {
    return notifications.filter(
      (notification) => !notification.isRead
    ).length;
  }, [notifications]);

  const readCount = notifications.length - unreadCount;

  const filteredNotifications = useMemo(() => {
    const searchValue = search
      .trim()
      .toLowerCase();

    return notifications.filter((notification) => {
      const message =
        notification.message?.toLowerCase() || "";

      const matchesSearch =
        !searchValue ||
        message.includes(searchValue);

      const matchesFilter =
        filter === "all" ||
        (filter === "unread" && !notification.isRead) ||
        (filter === "read" && notification.isRead);

      return matchesSearch && matchesFilter;
    });
  }, [notifications, filter, search]);

  const markAsRead = async (id) => {
    try {
      setReadingId(id);

      await notificationService.markAsRead(id);

      setNotifications((previous) =>
        previous.map((notification) =>
          notification.id === id
            ? {
                ...notification,
                isRead: true,
              }
            : notification
        )
      );
    } catch (err) {
      console.error(
        "Failed to mark notification as read:",
        err
      );

      alert(
        err.response?.data?.message ||
          "Unable to mark notification as read."
      );
    } finally {
      setReadingId(null);
    }
  };

  const markAllAsRead = async () => {
    const unreadNotifications =
      notifications.filter(
        (notification) => !notification.isRead
      );

    if (unreadNotifications.length === 0) {
      return;
    }

    try {
      for (const notification of unreadNotifications) {
        await notificationService.markAsRead(
          notification.id
        );
      }

      setNotifications((previous) =>
        previous.map((notification) => ({
          ...notification,
          isRead: true,
        }))
      );
    } catch (err) {
      console.error(
        "Failed to mark all notifications as read:",
        err
      );

      alert(
        "Some notifications could not be marked as read."
      );

      loadNotifications();
    }
  };

    const parseNotificationDate = (dateValue) => {
    if (!dateValue) {
      return null;
    }

    let value = String(dateValue);

    // Backend stores DateTime in UTC.
    // If the API does not include timezone information,
    // explicitly treat the value as UTC.
    if (
      !value.endsWith("Z") &&
      !/[+-]\d{2}:\d{2}$/.test(value)
    ) {
      value += "Z";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return null;
    }

    return date;
  };

  const formatDate = (dateValue) => {
    const date = parseNotificationDate(dateValue);

    if (!date) {
      return "Unknown date";
    }

    return date.toLocaleString("en-IN", {
      timeZone: "Asia/Kolkata",
      day: "2-digit",
      month: "short",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const getRelativeTime = (dateValue) => {
    const date = parseNotificationDate(dateValue);

    if (!date) {
      return "";
    }

    const difference =
      Date.now() - date.getTime();

    const minutes = Math.floor(
      difference / 60000
    );

    if (minutes < 1) {
      return "Just now";
    }

    if (minutes < 60) {
      return `${minutes}m ago`;
    }

    const hours = Math.floor(minutes / 60);

    if (hours < 24) {
      return `${hours}h ago`;
    }

    const days = Math.floor(hours / 24);

    if (days < 7) {
      return `${days}d ago`;
    }

    return formatDate(dateValue);
  };

  const getNotificationIcon = (message) => {
    const value =
      message?.toLowerCase() || "";

    if (
      value.includes("assigned") ||
      value.includes("assign")
    ) {
      return "✓";
    }

    if (
      value.includes("completed") ||
      value.includes("done")
    ) {
      return "✓";
    }

    if (
      value.includes("status") ||
      value.includes("progress")
    ) {
      return "↻";
    }

    if (
      value.includes("deadline") ||
      value.includes("due")
    ) {
      return "!";
    }

    if (
      value.includes("comment") ||
      value.includes("commented")
    ) {
      return "💬";
    }

    return "●";
  };

  return (
    <div className="notifications-page">
      {/* Header */}
      <div className="notifications-header">
        <div>
          <button
            type="button"
            className="notifications-back-button"
            onClick={() => navigate("/dashboard")}
          >
            ← Back
          </button>

          <div className="notifications-title-row">
            <div className="notifications-title-icon">
              🔔
            </div>

            <div>
              <h1>Notifications</h1>
              <p>
                Stay updated with your latest task activity.
              </p>
            </div>
          </div>
        </div>

        <div className="notification-summary">
          <div className="notification-summary-item">
            <span className="summary-number">
              {notifications.length}
            </span>

            <span className="summary-label">
              Total
            </span>
          </div>

          <div className="summary-divider" />

          <div className="notification-summary-item unread-summary">
            <span className="summary-number">
              {unreadCount}
            </span>

            <span className="summary-label">
              Unread
            </span>
          </div>

          <div className="summary-divider" />

          <div className="notification-summary-item">
            <span className="summary-number">
              {readCount}
            </span>

            <span className="summary-label">
              Read
            </span>
          </div>
        </div>
      </div>

      {/* Controls */}
      <div className="notifications-toolbar">
        <div className="notifications-search">
          <span className="notification-search-icon">
            ⌕
          </span>

          <input
            type="text"
            placeholder="Search notifications..."
            value={search}
            onChange={(event) =>
              setSearch(event.target.value)
            }
          />

          {search && (
            <button
              type="button"
              className="notification-clear-search"
              onClick={() => setSearch("")}
            >
              ×
            </button>
          )}
        </div>

        <div className="notification-filters">
          <button
            type="button"
            className={
              filter === "all"
                ? "notification-filter active"
                : "notification-filter"
            }
            onClick={() => setFilter("all")}
          >
            All
          </button>

          <button
            type="button"
            className={
              filter === "unread"
                ? "notification-filter active"
                : "notification-filter"
            }
            onClick={() => setFilter("unread")}
          >
            Unread
            {unreadCount > 0 && (
              <span className="filter-count">
                {unreadCount}
              </span>
            )}
          </button>

          <button
            type="button"
            className={
              filter === "read"
                ? "notification-filter active"
                : "notification-filter"
            }
            onClick={() => setFilter("read")}
          >
            Read
          </button>
        </div>

        <button
          type="button"
          className="mark-all-button"
          disabled={unreadCount === 0}
          onClick={markAllAsRead}
        >
          ✓ Mark all as read
        </button>
      </div>

      {/* Error */}
      {error && (
        <div className="notifications-error">
          <div className="notifications-error-icon">
            !
          </div>

          <div>
            <strong>
              Unable to load notifications
            </strong>

            <p>{error}</p>
          </div>

          <button
            type="button"
            onClick={loadNotifications}
          >
            Try Again
          </button>
        </div>
      )}

      {/* Loading */}
      {loading ? (
        <div className="notifications-card">
          {[1, 2, 3, 4].map((item) => (
            <div
              className="notification-skeleton"
              key={item}
            >
              <div className="notification-skeleton-icon" />

              <div className="notification-skeleton-content">
                <div className="skeleton-line notification-line-small" />
                <div className="skeleton-line" />
                <div className="skeleton-line notification-line-medium" />
              </div>
            </div>
          ))}
        </div>
      ) : filteredNotifications.length === 0 ? (
        <div className="notifications-empty">
          <div className="notifications-empty-icon">
            🔔
          </div>

          <h2>
            {notifications.length === 0
              ? "You're all caught up"
              : "No notifications found"}
          </h2>

          <p>
            {notifications.length === 0
              ? "New task activity and updates will appear here."
              : "Try changing your search or notification filter."}
          </p>

          {(search || filter !== "all") && (
            <button
              type="button"
              onClick={() => {
                setSearch("");
                setFilter("all");
              }}
            >
              Clear Filters
            </button>
          )}
        </div>
      ) : (
        <div className="notifications-card">
          <div className="notifications-card-header">
            <div>
              <h2>Recent Notifications</h2>
              <span>
                {filteredNotifications.length} notification
                {filteredNotifications.length !== 1
                  ? "s"
                  : ""}
              </span>
            </div>

            <div className="live-indicator">
              <span />
              Live updates
            </div>
          </div>

          <div className="notifications-list">
            {filteredNotifications.map(
              (notification) => (
                <div
                  key={notification.id}
                  className={
                    notification.isRead
                      ? "notification-item"
                      : "notification-item unread"
                  }
                >
                  <div
                    className={
                      notification.isRead
                        ? "notification-icon"
                        : "notification-icon unread-icon"
                    }
                  >
                    {getNotificationIcon(
                      notification.message
                    )}
                  </div>

                  <div className="notification-body">
                    <div className="notification-message">
                      {notification.message}
                    </div>

                    <div className="notification-meta">
                      <span>
                        {getRelativeTime(
                          notification.createdAt
                        )}
                      </span>

                      <span className="meta-dot">
                        •
                      </span>

                      <span>
                        {formatDate(
                          notification.createdAt
                        )}
                      </span>
                    </div>
                  </div>

                  <div className="notification-action">
                    {!notification.isRead ? (
                      <button
                        type="button"
                        className="read-button"
                        disabled={
                          readingId ===
                          notification.id
                        }
                        onClick={() =>
                          markAsRead(
                            notification.id
                          )
                        }
                      >
                        {readingId ===
                        notification.id
                          ? "..."
                          : "Mark as read"}
                      </button>
                    ) : (
                      <span className="read-status">
                        ✓ Read
                      </span>
                    )}
                  </div>

                  {!notification.isRead && (
                    <span className="unread-dot" />
                  )}
                </div>
              )
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default Notifications;