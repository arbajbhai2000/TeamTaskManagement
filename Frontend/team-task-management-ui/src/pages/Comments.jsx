import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../services/api";
import commentService from "../services/commentService";
import "./Comments.css";

const Comments = () => {
  const navigate = useNavigate();

  const [tasks, setTasks] = useState([]);
  const [comments, setComments] = useState([]);

  const [search, setSearch] = useState("");
  const [selectedTask, setSelectedTask] = useState("all");

  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState(null);
  const [error, setError] = useState("");

  const getTaskId = (task) => task.id ?? task.Id;

  const getTaskTitle = (task) =>
    task.title ??
    task.Title ??
    task.name ??
    task.Name ??
    `Task #${getTaskId(task)}`;

  const getCommentId = (comment) => comment.id ?? comment.Id;

  const getCommentTaskId = (comment) =>
    comment.taskItemId ?? comment.TaskItemId;

  const getCommentUserName = (comment) =>
    comment.userName ??
    comment.UserName ??
    "Unknown User";

  const getCommentContent = (comment) =>
    comment.content ??
    comment.Content ??
    "";

  const getCommentCreatedAt = (comment) =>
    comment.createdAt ??
    comment.CreatedAt;

  const loadComments = async () => {
    try {
      setLoading(true);
      setError("");

      const taskResponse = await api.get("/Tasks");

      const taskData = Array.isArray(taskResponse.data)
        ? taskResponse.data
        : taskResponse.data?.data ?? [];

      setTasks(taskData);

      if (taskData.length === 0) {
        setComments([]);
        return;
      }

      /*
       * GET /api/Comments/task/{taskId}
       *
       * Promise.allSettled prevents one inaccessible task
       * from breaking the complete Comments page.
       */
      const results = await Promise.allSettled(
        taskData.map(async (task) => {
          const taskId = getTaskId(task);

          const taskComments =
            await commentService.getByTaskId(taskId);

          return {
            task,
            comments: Array.isArray(taskComments)
              ? taskComments
              : [],
          };
        })
      );

      const combinedComments = [];

      results.forEach((result) => {
        if (result.status !== "fulfilled") {
          return;
        }

        const { task, comments: taskComments } = result.value;

        taskComments.forEach((comment) => {
          combinedComments.push({
            ...comment,
            taskTitle: getTaskTitle(task),
            taskId: getTaskId(task),
          });
        });
      });

      combinedComments.sort((a, b) => {
        const dateA = new Date(
          getCommentCreatedAt(a) || 0
        ).getTime();

        const dateB = new Date(
          getCommentCreatedAt(b) || 0
        ).getTime();

        return dateB - dateA;
      });

      setComments(combinedComments);
    } catch (err) {
      console.error("Failed to load comments:", err);

      setError(
        err.response?.data?.message ||
          "Unable to load comments. Please try again."
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadComments();
  }, []);

  const filteredComments = useMemo(() => {
    const searchValue = search.trim().toLowerCase();

    return comments.filter((comment) => {
      const content =
        getCommentContent(comment).toLowerCase();

      const userName =
        getCommentUserName(comment).toLowerCase();

      const taskTitle =
        (comment.taskTitle || "").toLowerCase();

      const matchesSearch =
        !searchValue ||
        content.includes(searchValue) ||
        userName.includes(searchValue) ||
        taskTitle.includes(searchValue);

      const commentTaskId =
        String(getCommentTaskId(comment));

      const matchesTask =
        selectedTask === "all" ||
        commentTaskId === String(selectedTask);

      return matchesSearch && matchesTask;
    });
  }, [comments, search, selectedTask]);

  const uniqueTaskCount = useMemo(() => {
    return new Set(
      comments.map((comment) =>
        getCommentTaskId(comment)
      )
    ).size;
  }, [comments]);

  const handleDelete = async (commentId) => {
    const confirmed = window.confirm(
      "Are you sure you want to delete this comment?"
    );

    if (!confirmed) {
      return;
    }

    try {
      setDeletingId(commentId);

      await commentService.remove(commentId);

      setComments((previous) =>
        previous.filter(
          (comment) =>
            getCommentId(comment) !== commentId
        )
      );
    } catch (err) {
      console.error("Failed to delete comment:", err);

      alert(
        err.response?.data?.message ||
          "Unable to delete the comment."
      );
    } finally {
      setDeletingId(null);
    }
  };

  const formatDate = (dateValue) => {
    if (!dateValue) {
      return "Unknown date";
    }

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
      return "Unknown date";
    }

    return date.toLocaleString("en-IN", {
      day: "2-digit",
      month: "short",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const getInitials = (name) => {
    if (!name) {
      return "?";
    }

    const parts = name.trim().split(/\s+/);

    if (parts.length === 1) {
      return parts[0].substring(0, 2).toUpperCase();
    }

    return (
      parts[0][0] +
      parts[parts.length - 1][0]
    ).toUpperCase();
  };

  return (
    <div className="comments-page">
      {/* Header */}
      <div className="comments-header">
        <div>
          <button
            type="button"
            className="comments-back-button"
            onClick={() => navigate("/dashboard")}
          >
            ← Back
          </button>

          <div className="comments-title-row">
            <div className="comments-title-icon">
              💬
            </div>

            <div>
              <h1>Comments</h1>
              <p>
                View and manage conversations across your tasks.
              </p>
            </div>
          </div>
        </div>

        <div className="comments-summary">
          <div className="summary-item">
            <span className="summary-number">
              {comments.length}
            </span>
            <span className="summary-label">
              Comments
            </span>
          </div>

          <div className="summary-divider" />

          <div className="summary-item">
            <span className="summary-number">
              {uniqueTaskCount}
            </span>
            <span className="summary-label">
              Tasks
            </span>
          </div>
        </div>
      </div>

      {/* Controls */}
      <div className="comments-controls">
        <div className="comments-search">
          <span className="search-icon">⌕</span>

          <input
            type="text"
            placeholder="Search comments, users or tasks..."
            value={search}
            onChange={(event) =>
              setSearch(event.target.value)
            }
          />

          {search && (
            <button
              type="button"
              className="clear-search"
              onClick={() => setSearch("")}
            >
              ×
            </button>
          )}
        </div>

        <div className="task-filter">
          <label htmlFor="task-filter">
            Task
          </label>

          <select
            id="task-filter"
            value={selectedTask}
            onChange={(event) =>
              setSelectedTask(event.target.value)
            }
          >
            <option value="all">
              All Tasks
            </option>

            {tasks.map((task) => (
              <option
                key={getTaskId(task)}
                value={getTaskId(task)}
              >
                {getTaskTitle(task)}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="comments-error">
          <div className="error-icon">!</div>

          <div>
            <strong>Something went wrong</strong>
            <p>{error}</p>
          </div>

          <button
            type="button"
            onClick={loadComments}
          >
            Try Again
          </button>
        </div>
      )}

      {/* Loading */}
      {loading ? (
        <div className="comments-list">
          {[1, 2, 3].map((item) => (
            <div
              className="comment-skeleton"
              key={item}
            >
              <div className="skeleton-avatar" />

              <div className="skeleton-content">
                <div className="skeleton-line skeleton-small" />
                <div className="skeleton-line" />
                <div className="skeleton-line skeleton-medium" />
              </div>
            </div>
          ))}
        </div>
      ) : filteredComments.length === 0 ? (
        <div className="comments-empty">
          <div className="empty-icon">
            💬
          </div>

          <h2>
            {comments.length === 0
              ? "No comments yet"
              : "No comments found"}
          </h2>

          <p>
            {comments.length === 0
              ? "Comments added to your tasks will appear here."
              : "Try changing your search or task filter."}
          </p>

          {(search || selectedTask !== "all") && (
            <button
              type="button"
              onClick={() => {
                setSearch("");
                setSelectedTask("all");
              }}
            >
              Clear Filters
            </button>
          )}
        </div>
      ) : (
        <div className="comments-content">
          <div className="comments-list-header">
            <div>
              <h2>Recent Comments</h2>
              <span>
                {filteredComments.length} conversation
                {filteredComments.length !== 1
                  ? "s"
                  : ""}
              </span>
            </div>
          </div>

          <div className="comments-list">
            {filteredComments.map((comment) => {
              const commentId =
                getCommentId(comment);

              const userName =
                getCommentUserName(comment);

              return (
                <div
                  className="comment-card"
                  key={commentId}
                >
                  <div className="comment-avatar">
                    {getInitials(userName)}
                  </div>

                  <div className="comment-main">
                    <div className="comment-top">
                      <div>
                        <div className="comment-user">
                          {userName}
                        </div>

                        <div className="comment-date">
                          {formatDate(
                            getCommentCreatedAt(comment)
                          )}
                        </div>
                      </div>

                      <div className="comment-task-badge">
                        <span>▣</span>
                        {comment.taskTitle}
                      </div>
                    </div>

                    <div className="comment-message">
                      {getCommentContent(comment)}
                    </div>

                    <div className="comment-actions">
                      <button
                        type="button"
                        className="view-task-button"
                        onClick={() =>
                          navigate(
                            `/tasks/${getCommentTaskId(comment)}`
                          )
                        }
                      >
                        View Task →
                      </button>

                      <button
                        type="button"
                        className="delete-comment-button"
                        disabled={deletingId === commentId}
                        onClick={() =>
                          handleDelete(commentId)
                        }
                      >
                        {deletingId === commentId
                          ? "Deleting..."
                          : "Delete"}
                      </button>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
};

export default Comments;