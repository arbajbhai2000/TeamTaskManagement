import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import api from "../services/api";
import commentService from "../services/commentService";
import "./TaskDetails.css";

function TaskDetails() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [task, setTask] = useState(null);
  const [status, setStatus] = useState("");

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [comments, setComments] = useState([]);
  const [commentText, setCommentText] = useState("");
  const [commentsLoading, setCommentsLoading] = useState(false);
  const [commentSaving, setCommentSaving] = useState(false);
  const [commentError, setCommentError] = useState("");

  const getStatusName = (statusValue) => {
    switch (Number(statusValue)) {
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

  const formatDate = (date) => {
    if (!date) return "-";

    return new Date(date).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const formatCommentDate = (date) => {
    if (!date) return "";

    return new Date(date).toLocaleString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
    });
  };

  const loadTask = async () => {
    try {
      setLoading(true);
      setError("");

      const response = await api.get(`/Tasks/${id}`);

      setTask(response.data);
      setStatus(String(response.data.status));
    } catch (err) {
      console.error("Failed to load task:", err);

      setError(
        err.response?.data?.message ||
          "Unable to load task details. Please try again."
      );
    } finally {
      setLoading(false);
    }
  };

  const loadComments = async () => {
    try {
      setCommentsLoading(true);
      setCommentError("");

      const data = await commentService.getByTaskId(id);

      setComments(data || []);
    } catch (err) {
      console.error("Failed to load comments:", err);

      setCommentError(
        err.response?.data?.message ||
          "Unable to load comments."
      );
    } finally {
      setCommentsLoading(false);
    }
  };

  useEffect(() => {
    const token = localStorage.getItem("accessToken");

    if (!token) {
      navigate("/login");
      return;
    }

    loadTask();
    loadComments();
  }, [id, navigate]);

  const handleStatusUpdate = async () => {
    if (!task) return;

    try {
      setSaving(true);
      setError("");
      setSuccess("");

      const request = {
        title: task.title,
        description: task.description || "",
        status: Number(status),
        priority: Number(task.priority),
        dueDate: task.dueDate,
        assignedToId: task.assignedToId,
        teamId: task.teamId,
      };

      const response = await api.put(`/Tasks/${id}`, request);

      setTask(response.data);
      setStatus(String(response.data.status));

      setSuccess("Task status updated successfully.");
      setTimeout(() => {
        setMessage("");
      }, 2000);
    } catch (err) {
      console.error("Failed to update task:", err);

      setError(
        err.response?.data?.message ||
          "Unable to update task status. Please try again."
      );
    } finally {
      setSaving(false);
    }
  };

  const handleAddComment = async () => {
    const content = commentText.trim();

    if (!content) {
      return;
    }

    if (content.length > 500) {
      setCommentError("Comment cannot exceed 500 characters.");
      return;
    }

    try {
      setCommentSaving(true);
      setCommentError("");

      const newComment = await commentService.create(
        content,
        Number(id)
      );

      setComments((currentComments) => [
        ...currentComments,
        newComment,
      ]);

      setCommentText("");
    } catch (err) {
      console.error("Failed to add comment:", err);

      setCommentError(
        err.response?.data?.message ||
          "Unable to add comment."
      );
    } finally {
      setCommentSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="task-details-state">
        <div className="task-details-spinner"></div>
        <p>Loading task details...</p>
      </div>
    );
  }

  if (error && !task) {
    return (
      <div className="task-details-state task-details-error">
        <h2>Unable to load task</h2>
        <p>{error}</p>

        <button
          type="button"
          onClick={() => navigate("/tasks")}
        >
          ← Back
        </button>
      </div>
    );
  }

  if (!task) {
    return null;
  }

  return (
    <div className="task-details-page">

      {/* Back Button */}
      <div className="task-details-header">
        <button
          type="button"
          className="back-button"
          onClick={() => navigate("/tasks")}
        >
          ← Back
        </button>
      </div>

      {/* Task Details */}
      <div className="task-details-card">

        {/* Task Header */}
        <div className="task-details-title-section">
          <div>
            <span className="task-details-label">
              TASK DETAILS
            </span>

            <h1>{task.title}</h1>

            <p>
              {task.description ||
                "No description available for this task."}
            </p>
          </div>

          <span
            className={`task-status status-${task.status}`}
          >
            {getStatusName(task.status)}
          </span>
        </div>

        {/* Task Information */}
        <div className="task-details-grid">

          <div className="detail-item">
            <span>Team</span>

            <strong>
              {task.teamName || "No team"}
            </strong>
          </div>

          <div className="detail-item">
            <span>Priority</span>

            <strong
              className={`priority-${task.priority}`}
            >
              {getPriorityName(task.priority)}
            </strong>
          </div>

          <div className="detail-item">
            <span>Due Date</span>

            <strong>
              {formatDate(task.dueDate)}
            </strong>
          </div>

          <div className="detail-item">
            <span>Assigned To</span>

            <strong>
              {task.assignedToName || "You"}
            </strong>
          </div>

        </div>

        {/* Update Status */}
        <div className="task-status-update">

          <div>
            <h2>Update Status</h2>

            <p>
              Change the current status of this task.
            </p>
          </div>

          <div className="status-update-controls">

            <select
              value={status}
              onChange={(event) => {
                setStatus(event.target.value);
                setSuccess("");
                setError("");
              }}
              disabled={saving}
            >
              <option value="1">To Do</option>
              <option value="2">In Progress</option>
              <option value="3">Done</option>
            </select>

            <button
              type="button"
              onClick={handleStatusUpdate}
              disabled={
                saving ||
                Number(status) === Number(task.status)
              }
            >
              {saving ? "Saving..." : "Save Changes"}
            </button>

          </div>

          {success && (
            <div className="task-success">
              {success}
            </div>
          )}

          {error && (
            <div className="task-inline-error">
              {error}
            </div>
          )}

        </div>

        {/* Comments */}
        <div className="task-comments-section">

          <div className="task-comments-header">

            <div>
              <h2>Comments</h2>

              <p>
                Discuss this task with your team.
              </p>
            </div>

            <span className="comment-count">
              {comments.length}
            </span>

          </div>

          {/* Add Comment */}
          <div className="comment-input-area">

            <textarea
              value={commentText}
              onChange={(event) => {
                setCommentText(event.target.value);
                setCommentError("");
              }}
              placeholder="Write a comment..."
              rows={4}
              maxLength={500}
              disabled={commentSaving}
            />

            <div className="comment-input-footer">

              <span>
                {commentText.length}/500
              </span>

              <button
                type="button"
                onClick={handleAddComment}
                disabled={
                  commentSaving ||
                  !commentText.trim()
                }
              >
                {commentSaving
                  ? "Posting..."
                  : "Add Comment"}
              </button>

            </div>

          </div>

          {/* Comment Error */}
          {commentError && (
            <div className="task-inline-error">
              {commentError}
            </div>
          )}

          {/* Comments List */}
          <div className="comments-list">

            {commentsLoading ? (
              <div className="comments-loading">
                Loading comments...
              </div>
            ) : comments.length === 0 ? (
              <div className="comments-empty">

                <div className="comments-empty-icon">
                  💬
                </div>

                <h3>No comments yet</h3>

                <p>
                  Be the first to comment on this task.
                </p>

              </div>
            ) : (
              comments.map((comment) => (
                <div
                  className="comment-item"
                  key={comment.id}
                >

                  <div className="comment-avatar">
                    {comment.userName
                      ?.charAt(0)
                      ?.toUpperCase() || "U"}
                  </div>

                  <div className="comment-content">

                    <div className="comment-meta">

                      <strong>
                        {comment.userName}
                      </strong>

                      <span>
                        {formatCommentDate(
                          comment.createdAt
                        )}
                      </span>

                    </div>

                    <p>
                      {comment.content}
                    </p>

                  </div>

                </div>
              ))
            )}

          </div>

        </div>

      </div>
    </div>
  );
}

export default TaskDetails;